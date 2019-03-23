using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework.Caching
{
    public class RedisCacheManager : ICacheManager, IDisposable
    {
        private const string RemoveScript = (@"
                return redis.call('DEL', KEYS[1])");

        private const string ExpireScript = (@"
                return redis.call('EXPIRE', KEYS[1], ARGV[1])");

        // KEYS[1] = = key
        // ARGV[1] = absolute-expiration - ticks as long (-1 for none)
        // ARGV[2] = sliding-expiration - ticks as long (-1 for none)
        // ARGV[3] = relative-expiration (long, in seconds, -1 for none) - Min(absolute-expiration - Now, sliding-expiration)
        // ARGV[4] = data - byte[]
        // ARGV[5] = type - type string
        // ARGV[6] = compress - bool wether use gzip
        // this order should not change LUA script depends on it
        private const string SetScript = (@"
                local result = 1 
                redis.call('HMSET', KEYS[1], 'absexp', ARGV[1], 'sldexp', ARGV[2], 'data', ARGV[4], 'type', ARGV[5], 'compress', ARGV[6], 'ser', ARGV[7])
                if ARGV[3] ~= '-1' then
                  result = redis.call('EXPIRE', KEYS[1], ARGV[3]) 
                end
                return result");


        ///// <summary>
        ///// ARGV[1] : key pattern
        ///// 此命令无法被执行，因为 redis 无法做 redo
        ///// </summary>
        //private const string DeletePatternKeys = @"
        //            local cursor = 0
        //            local pattern = ARGV[1]
        //            local keys = {}
        //            local result = 0
        //            repeat
        //                local r = redis.call('SCAN', cursor, 'MATCH', pattern)
        //                cursor = tonumber(r[1])
        //                for k,v in ipairs(r[2]) do
        //                    table.insert(keys, v)
        //                end
        //            until cursor == 0
        //            if next(keys) ~= nil then 
        //                 result = redis.call('DEL', unpack(keys)) 
        //            end
        //            return results";

        /// <summary>
        /// ARGV[1] : key pattern
        /// </summary>
        private const string QueryKeysScript = @"
                    local cursor = 0
                    local pattern = ARGV[1]
                    local keys = {}
                    repeat
                        local r = redis.call('SCAN', cursor, 'MATCH', pattern)
                        cursor = tonumber(r[1])
                        for k,v in ipairs(r[2]) do
                            table.insert(keys, v)
                        end
                    until cursor == 0
                    return keys";

        private const string AbsoluteExpirationKey = "absexp";
        private const string SlidingExpirationKey = "sldexp";
        private const string DataKey = "data";
        private const string TypeKey = "type";
        private const string CompressKey = "compress";
        private const string SerializerKey = "ser";
        private const long NotPresent = -1;

        private const string RegionSpliterChar = "-";

        public const string ClearScript = @"redis.call('FLUSHALL')";

        private Lazy<ConnectionMultiplexer> _connection;
        private IDatabase _database;

        private readonly object ConnectionSyncRoot = new object();

        private readonly SchubertRedisOptions _options;
        private ILogger _logger;
        private Timer _timer = null;
        private Object _retryLock = new object();
        private IDictionary<String, IRedisCacheSerializer> _redisCacheSerializers;

        private ICacheManager _backCache = null;
        private readonly object BackCacheSyncRoot = new object();
        private readonly object DatabaseSyncRoot = new object();

        private bool _existsConnectionError = false;
        private String _defaultRegion = null;

        public RedisCacheManager(
            IOptions<SchubertOptions> schubertOptions,
            IOptions<SchubertRedisOptions> redisOptions,
            IEnumerable<IRedisCacheSerializer> serializers = null,
            ILoggerFactory loggerFactory = null)
        {
            Guard.ArgumentNotNull(schubertOptions, nameof(schubertOptions));
            Guard.ArgumentNotNull(redisOptions, nameof(redisOptions));
            _connection = new Lazy<ConnectionMultiplexer>(this.CreateConnection, true);
            _logger = loggerFactory?.CreateLogger<RedisCacheManager>() ?? (ILogger)NullLogger.Instance;
            _options = redisOptions.Value;
            _defaultRegion = $"{schubertOptions.Value.Group}{RegionSpliterChar}{ schubertOptions.Value.AppSystemName}";
            _redisCacheSerializers = serializers?.ToDictionary(s => s.Name, s => s, StringComparer.OrdinalIgnoreCase);
            if (_redisCacheSerializers.IsNullOrEmpty())
            {
                _redisCacheSerializers =
                new Dictionary<string, IRedisCacheSerializer>(StringComparer.OrdinalIgnoreCase)
                {
                    { RedisSerializerNames.JsonNet, new JsonNetSerializer() }
                };
            }
        }

        private string GetCurrentSerializerName()
        {
            return _options.SerializerName.IfNullOrWhiteSpace(RedisSerializerNames.JsonNet);
        }

        private IRedisCacheSerializer FindSerializer(string name, bool throwIfMissed)
        {
            string id = name.IfNullOrWhiteSpace(RedisSerializerNames.JsonNet);
            IRedisCacheSerializer ser = null;
            if (_redisCacheSerializers.TryGetValue(id, out ser))
            {
                return ser;
            }
            else
            {
                if (throwIfMissed)
                {
                    throw new SchubertException($"找不到名为 {name} 的 Redis 序列化提供程序。");
                }
                else
                {
                    _logger.WriteWarning($"找不到名为 {name} 的 Redis 序列化提供程序。");
                }
                return null;
            }
        }


        private ConnectionMultiplexer CreateConnection()
        {
            try
            {
                var rawConnection = ConnectionMultiplexer.Connect(_options.ConnectionString);
                return rawConnection;
            }
            catch (Exception ex)
            {
                _logger.WriteError("ConnectionMultiplexer.Connect 发生错误。", ex);
                throw;
            }
        }

        protected IDatabase GetDatabase(bool createNew = false)
        {
            if (createNew)
            {
                return _connection.Value.GetDatabase();
            }
            if (_database == null)
            {
                lock (ConnectionSyncRoot)
                {
                    if (_database == null)
                    {
                        _database = _connection.Value.GetDatabase();
                    }
                }
            }
            return _database;
        }

        /// <summary>
        /// 创建备用缓存。
        /// </summary>
        /// <returns></returns>
        private void CreateBackCache()
        {
            if (_backCache == null)
            {
                lock (BackCacheSyncRoot)
                {
                    if (_backCache == null)
                    {
                        _backCache = new MemoryCacheManager(new OptionsManager<MemoryCacheOptions>(
                            new OptionsFactory<MemoryCacheOptions>(
                            new IConfigureOptions<MemoryCacheOptions>[]
                            {
                                new ConfigureOptions<MemoryCacheOptions>(o=>
                                {
                                    o.ExpirationScanFrequency = TimeSpan.FromMinutes(1);
                                })
                            }, Enumerable.Empty<IPostConfigureOptions<MemoryCacheOptions>>())));
                    }
                }
            }
        }

        private void ReleaseBackCache()
        {
            if (_backCache != null)
            {
                lock (BackCacheSyncRoot)
                {
                    if (_backCache != null)
                    {
                        (_backCache as MemoryCacheManager).Dispose();
                        _backCache = null;
                    }
                }
            }
        }

        private void StartNoCacheMode(RedisConnectionException ex)
        {
            if (!_existsConnectionError)
            {
                _existsConnectionError = true;
                this.CreateBackCache();
                this._logger.WriteError($"无法连接 Redis 服务器 {_options.ConnectionString} ，进入本机缓存模式，{_options.ReconnectFrequencySeconds} 秒后断线重连开始执行。", ex);

                lock (_retryLock)
                {
                    _timer?.Dispose();
                    _timer = null;
                }
                _timer = new Timer(this.RetryConnection, null,
                    TimeSpan.FromSeconds(_options.ReconnectFrequencySeconds),
                    TimeSpan.FromSeconds(_options.ReconnectFrequencySeconds));
            }
        }

        private void RetryConnection(object state)
        {
            if (Monitor.TryEnter(_retryLock))
            {
                try
                {
                    if (_connection.Value.IsConnected)
                    {
                        var db = this.GetDatabase();

                        if (db.IsConnected(default(RedisKey)))
                        {
                            _existsConnectionError = false;
                            this.ReleaseBackCache();
                            this._logger.WriteInformation("Redis 断线重连成功，缓存开始正常工作。");
                            _timer?.Dispose();
                            _timer = null;
                            return;
                        }
                    }
                    this._logger.WriteInformation($"Redis 断线重连失败，{_options.ReconnectFrequencySeconds} 秒后再次重试。");
                    _connection = null;
                }
                catch (Exception ex)
                {
                    this._logger.WriteInformation($"Redis 断线重连失败，{_options.ReconnectFrequencySeconds} 秒后再次重试。", ex);
                    ex.ThrowIfNecessary();
                }
                finally
                {
                    Monitor.Exit(_retryLock);
                }
            }
        }

        protected string GetKeyPrefix(string region)
        {
            return $"H{RegionSpliterChar}{region.IfNullOrWhiteSpace(_defaultRegion)}{RegionSpliterChar}";
        }

        private string GetFullKey(string region, string key)
        {
            return $"{GetKeyPrefix(region)}{key}";
        }

        private void EnsureKeyValid(string key, string paramName)
        {
            Guard.ArgumentNullOrWhiteSpaceString(key, paramName);
        }

        public void Clear()
        {
            if (!_existsConnectionError)
            {
                try
                {
                    Retry.RunRetry<Exception>(() => this.GetDatabase().ScriptEvaluate(ClearScript), _options.RetryCount, _options.RetryDelayMilliseconds);
                }
                catch (RedisConnectionException ex)
                {
                    this.StartNoCacheMode(ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.WriteError("清理缓存时 Redis 发生错误。", ex);
                }
                catch (RedisException ex)
                {
                    _logger.WriteError("清理缓存时 Redis 发生错误", ex);
                }
            }
            else
            {
                _backCache?.Clear();
            }
        }

        public void ClearRegion(string region = "")
        {
            if (!_existsConnectionError)
            {
                var parttern = new RedisValue[] { $"{GetKeyPrefix(region)}*" };
                try
                {
                    var database = this.GetDatabase();
                    string[] keys = Retry.RunRetry<String[], Exception>(() => (string[])database.ScriptEvaluate(QueryKeysScript, values: parttern), _options.RetryCount, _options.RetryDelayMilliseconds);
                    if (!keys.IsNullOrEmpty())
                    {
                        RedisKey[] keyValues = keys.Distinct().Select(k => (RedisKey)k).ToArray();
                        string keyArgs = Enumerable.Range(1, keyValues.Count()).Select(i => $"KEYS[{i}]").ToArrayString(", ");

                        string script = $@"return redis.call('DEL', {keyArgs})";
                        database.ScriptEvaluate(script, keyValues);

                        //database.KeyDelete(keyValues);
                    }
                }
                catch (RedisConnectionException ex)
                {
                    this.StartNoCacheMode(ex);
                }
                catch (RedisException ex)
                {
                    _logger.WriteError($"清除缓存区域 {region} 时 Redis 发生错误。", ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.WriteError($"清除缓存区域 {region} 时 Redis 发生错误。", ex);
                }
            }
            else
            {
                _backCache.ClearRegion(region);
            }
        }



        public object Get(string key, string region = "")
        {
            if (!_existsConnectionError)
            {
                this.EnsureKeyValid(key, nameof(key));

                string fullKey = GetFullKey(region, key);

                try
                {
                    string t = null;
                    RedisValue v = RedisValue.Null;
                    bool c = false;
                    String s = null;
                    var getted = Retry.RunRetry<bool, Exception>(() => this.GetAndRefresh(fullKey, true, out t, out v, out c, out s), _options.RetryCount, _options.RetryDelayMilliseconds);
                    if (getted)
                    {
                        //考虑程序变更后类型可能已经不存在或更名。
                        Type type = GetTypeFromTypeString(t);
                        if (type == null)
                        {
                            this.Remove(key, region);
                            _logger.WriteWarning($"缓存数据中的类型 {t} 可能已经变更，导致缓存 {key} 失效。");
                            return null;
                        }
                        //考虑数据结构更新后缓存反序列化的问题。
                        if (this.DeserializeData(key, s, type, v, c, out object result))
                        {
                            return result;
                        }
                        else
                        {
                            this.Remove(key, region);
                        }
                    }
                }
                catch (RedisConnectionException ex)
                {
                    this.StartNoCacheMode(ex);
                }
                catch (RedisException ex)
                {
                    _logger.WriteError($"获取缓存时 Redis 发生错误，缓存键：{key}，区域： {region}。", ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.WriteError($"获取缓存时 Redis 发生错误，缓存键：{key}，区域： {region}。", ex);
                }
            }
            return _backCache?.Get(key, region);
        }

        public void Remove(string key, string region = "")
        {
            if (!_existsConnectionError)
            {
                this.EnsureKeyValid(key, nameof(key));
                try
                {
                    Retry.RunRetry<Exception>(()=> this.GetDatabase().ScriptEvaluate(RemoveScript, new RedisKey[] { this.GetFullKey(region, key) }),
                        _options.RetryCount, 
                        _options.RetryDelayMilliseconds);
                }
                catch (RedisConnectionException ex)
                {
                    this.StartNoCacheMode(ex);
                }
                catch (RedisException ex)
                {
                    _logger.WriteError($"移除缓存时 Redis 发生错误，缓存键：{key}，区域： {region}。", ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.WriteError($"移除缓存时 Redis 发生错误，缓存键：{key}，区域： {region}。", ex);
                }
            }
            else
            {
                _backCache?.Remove(key, region);
            }
        }

        public void Set(string key, object data, TimeSpan? timeout = default(TimeSpan?), string region = "", bool useSlidingExpiration = false)
        {
            if (!_existsConnectionError)
            {
                this.EnsureKeyValid(key, nameof(key));
                try
                {
                    Retry.RunRetry<Exception>(() => this.SetCore(key, data, timeout, region, useSlidingExpiration), _options.RetryCount, _options.RetryDelayMilliseconds);
                }
                catch (RedisConnectionException ex)
                {
                    this.StartNoCacheMode(ex);
                }
                catch (RedisException ex)
                {
                    _logger.WriteError($"添加缓存时 Redis 发生错误，缓存键：{key}，区域： {region}。", ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.WriteError($"添加缓存时 Redis 发生错误，缓存键：{key}，区域： {region}。", ex);
                }
            }
            _backCache?.Set(key, data, timeout, region, useSlidingExpiration);
        }

        public bool Refresh(string key, string region = "")
        {
            if (!_existsConnectionError)
            {
                this.EnsureKeyValid(key, nameof(key));
                string fullKey = GetFullKey(region, key);
                string type;
                RedisValue v;
                bool compressed;
                string ser;
                try
                {
                    Retry.RunRetry<Exception>(() => GetAndRefresh(fullKey, false, out type, out v, out compressed, out ser), _options.RetryCount, _options.RetryDelayMilliseconds);
                }
                catch (RedisConnectionException ex)
                {
                    this.StartNoCacheMode(ex);
                }
                catch (RedisException ex)
                {
                    _logger.WriteError($"刷新缓存区域 {region} 时 Redis 发生错误", ex);
                }
                catch (TimeoutException ex)
                {
                    _logger.WriteError($"刷新缓存区域 {region} 时 Redis 发生错误", ex);
                }
            }
            return _backCache?.Refresh(key, region) ?? false;

        }

        private bool GetAndRefresh(string fullKey, bool getData, out string type, out RedisValue data, out bool compressed, out string serializer)
        {
            type = String.Empty;
            data = RedisValue.Null;
            compressed = false;
            // This also resets the LRU status as desired.
            // TODO: Can this be done in one operation on the server side? Probably, the trick would just be the DateTimeOffset math.
            RedisValue[] results;
            serializer = null;
            var database = this.GetDatabase();
            if (getData)
            {
                results = database.HashMemberGet(fullKey, AbsoluteExpirationKey, SlidingExpirationKey, DataKey, TypeKey, CompressKey, SerializerKey);
                //results = this.Database.HashGet(fullKey, new RedisValue[] { AbsoluteExpirationKey, SlidingExpirationKey, DataKey, TypeKey, CompressKey });
            }
            else
            {
                results = database.HashMemberGet(fullKey, AbsoluteExpirationKey, SlidingExpirationKey);
            }

            // TODO: Error handling
            if (results.Length >= 2)
            {
                // Note we always get back two results, even if they are all null.
                // These operations will no-op in the null scenario.
                DateTimeOffset? absExpr;
                TimeSpan? sldExpr;
                MapMetadata(results, out absExpr, out sldExpr);
                RefreshExpire(database, fullKey, absExpr, sldExpr);
            }

            if (results.Length >= 6 && results[2].HasValue && results[3].HasValue && results[4].HasValue)
            {
                type = results[3];
                data = results[2];
                compressed = (bool)results[4];
                serializer = (string)results[5];
                return true;
            }
            return false;
        }

        private void RefreshExpire(IDatabase database, string fullKey, DateTimeOffset? absExpr, TimeSpan? sldExpr)
        {
            // Note Refresh has no effect if there is just an absolute expiration (or neither).
            TimeSpan? expr = null;
            if (sldExpr.HasValue)
            {
                if (absExpr.HasValue)
                {
                    var relExpr = absExpr.Value - DateTimeOffset.Now;
                    expr = relExpr <= sldExpr.Value ? relExpr : sldExpr;
                }
                else
                {
                    expr = sldExpr;
                }
                //database.KeyExpire(fullKey, expr);
                var result = database.ScriptEvaluate(ExpireScript,
                    new RedisKey[] { fullKey },
                    new RedisValue[] { (long)expr.Value.TotalSeconds.Precision(0) });
                if (result.IsNull)
                {
                    _logger.WriteWarning($"Redis 脚本执行返回了错误的结果（lua: {ExpireScript}）。");
                }
                // TODO: Error handling
            }
        }

        private bool DeserializeData(string key, string serName, Type type, RedisValue data, bool gzipCompress, out object item)
        {
            item = null;
            if (data.IsNull)
            {
                return true;
            }
            if (type.Equals(typeof(String)))
            {
                item = (string)data;
                return true;
            }
            if (type.Equals(typeof(Int32)))
            {
                item = (int)data;
                return true;
            }
            if (type.Equals(typeof(Double)))
            {
                item = (double)data;
                return true;
            }
            if (type.Equals(typeof(Single)))
            {
                item = (float)data;
                return true;
            }
            if (type.Equals(typeof(Byte)))
            {
                item = (byte)data;
                return true;
            }
            if (type.Equals(typeof(Boolean)))
            {
                item = (bool)data;
                return true;
            }
            if (type.Equals(typeof(byte[])))
            {
                item = (byte[])data;
                return true;
            }
            byte[] bytes = (byte[])data;
            IRedisCacheSerializer serializer = this.FindSerializer(serName, false);
            if (serializer == null)
            {
                return false;
            }
            using (MemoryStream stream = new MemoryStream(bytes))
            {
                Stream gzs = null;
                if (gzipCompress)
                {
                    gzs = new GZipStream(stream, CompressionMode.Decompress, true);
                }
                try
                {
                    using (StreamReader reader = new StreamReader(gzipCompress ? gzs : stream, Encoding.UTF8))
                    {
                        item = serializer.Deserialize(reader, type);
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    var oex = ex.GetOriginalException();
                    _logger.WriteWarning(0, $@"Redies 获取对象时发生反序列错误，可能由于数据结构变化，类型 {type.Name}。",
                                            extensions: new
                                            {
                                                RedisKey = key,
                                                RawValue = Encoding.UTF8.GetString(data),
                                                Exception = $"（{oex.GetType().Name}）{oex.Message}",
                                                StackTrace = oex.StackTrace
                                            });
                    ex.ThrowIfNecessary();
                    return false;
                }
                finally
                {
                    if (gzs != null)
                    {
                        gzs.Dispose();
                    }
                }
            }
        }

        private RedisValue SerializeData(object data, string serializerName, bool gzipCompress = false)
        {
            if (data.GetType().Equals(typeof(String)))
            {
                return (String)data;
            }
            if (data.GetType().Equals(typeof(Int32)))
            {
                return (int)data;
            }
            if (data.GetType().Equals(typeof(Double)))
            {
                return (double)data;
            }
            if (data.GetType().Equals(typeof(Single)))
            {
                return (float)data;
            }
            if (data.GetType().Equals(typeof(Byte)))
            {
                return (byte)data;
            }
            if (data.GetType().Equals(typeof(Boolean)))
            {
                return (bool)data;
            }
            if (data.GetType().Equals(typeof(byte[])))
            {
                return (byte[])data;
            }
            IRedisCacheSerializer serializer = this.FindSerializer(serializerName, false) ?? this._redisCacheSerializers.FirstOrDefault().Value;
            using (MemoryStream stream = new MemoryStream())
            {
                Stream gzs = null;
                if (gzipCompress)
                {
                    gzs = new GZipStream(stream, CompressionMode.Compress, true);
                }
                try
                {
                    using (StreamWriter writer = new StreamWriter(gzipCompress ? gzs : stream, Encoding.UTF8, 1024, true))
                    {
                        serializer.Serialize(writer, data);
                    }
                    if (gzipCompress)
                    {
                        gzs.Dispose();
                        gzs = null;
                    }
                    stream.Position = 0;
                    return stream.ToArray();
                }
                finally
                {
                    if (gzs != null)
                    {
                        gzs.Dispose();
                    }
                }
            }
        }

        private static JsonSerializer CreateSerializer()
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Ignore;
            serializer.DefaultValueHandling = DefaultValueHandling.Ignore;
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
            serializer.TypeNameHandling = TypeNameHandling.Objects | TypeNameHandling.Arrays;
            return serializer;
        }

        private Type GetTypeFromTypeString(string typeString)
        {
            return Type.GetType(typeString, false);
        }

        protected void SetCore(string key, object data, TimeSpan? timeout = default(TimeSpan?), string region = "", bool useSlidingExpiration = false)
        {
            this.EnsureKeyValid(key, nameof(key));
            if (data == null)
            {
                this.Remove(key, region);
                return;
            }
            var creationTime = DateTimeOffset.UtcNow;
            var values = new RedisValue[]
                {
                    (!useSlidingExpiration && timeout.HasValue) ? creationTime.Add(timeout.Value).Ticks : NotPresent,
                    (useSlidingExpiration && timeout.HasValue) ? timeout.Value.Ticks : NotPresent,
                    timeout.HasValue ? (int)timeout.Value.TotalSeconds.Precision(0) : NotPresent,
                    this.SerializeData(data, this.GetCurrentSerializerName(), _options.GZipCompress),
                    data.GetType().AssemblyQualifiedName,
                    _options.GZipCompress,
                    this.GetCurrentSerializerName()
                };

            var result = this.GetDatabase().ScriptEvaluate(SetScript, new RedisKey[] { this.GetFullKey(region, key) }, values);
            if (result.IsNull)
            {
                throw new SchubertException("Redis 缓存返回了错误的结果。");
            }
        }

        private void MapMetadata(RedisValue[] results, out DateTimeOffset? absoluteExpiration, out TimeSpan? slidingExpiration)
        {
            absoluteExpiration = null;
            slidingExpiration = null;
            var absoluteExpirationTicks = (long?)results[0];
            if (absoluteExpirationTicks.HasValue && absoluteExpirationTicks.Value != NotPresent)
            {
                absoluteExpiration = new DateTimeOffset(absoluteExpirationTicks.Value, TimeSpan.Zero);
            }
            var slidingExpirationTicks = (long?)results[1];
            if (slidingExpirationTicks.HasValue && slidingExpirationTicks.Value != NotPresent)
            {
                slidingExpiration = new TimeSpan(slidingExpirationTicks.Value);
            }
        }

        public void Dispose()
        {
            this.ReleaseBackCache();
            if (_timer != null)
            {
                _timer?.Dispose();
                _timer = null;
            }
            _connection?.Value?.Dispose();
            _connection = null;
        }
    }
}
