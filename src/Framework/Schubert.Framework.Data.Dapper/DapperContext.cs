using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using System.Collections.Concurrent;
using Schubert.Framework.Data.Internal;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace Schubert.Framework.Data
{

    /// <summary>
    /// 表示一个 Dapper 上下文。
    /// </summary>
    public class DapperContext : IDatabaseContext, IDisposable
    {
        private bool _disposed = false;

        private ConcurrentDictionary<String, DbConnection> _connections = null;
        private ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;

        public DapperContext(DapperRuntime runtime, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            this.Runtime = runtime;
            _logger = loggerFactory.CreateLogger<DapperContext>();
            _connections = new ConcurrentDictionary<string, DbConnection>();
        }

        /// <summary>
        /// 根据配置名称获取数据库连接。
        /// </summary>
        /// <param name="connName">配置名称，为空表示获取默认连接。</param>
        /// <param name="ensureOpenned">是否同时确保数据连接已经被打开。</param>
        /// <returns></returns>
        public DbConnection GetConnection(string connName = null, bool ensureOpenned = true)
        {
            try
            {
                var provider = this.Runtime.GetDatabaseProvider(connName);
                string cacheKey = connName.IfNullOrWhiteSpace("default");
                TrackingDbConnection temConn = null;
                var dbConnecion = _connections.GetOrAdd(cacheKey, name =>
                {
                    string connectionString = this.Runtime.GetDbConnectionString(name);
                    ThrowIfConnectionNotFound(name, connectionString);
                    var innerConn = provider.CreateConnection(connectionString);
                    temConn = new TrackingDbConnection(innerConn, _loggerFactory);
                    return temConn;
                });
                if (temConn != null && !Object.ReferenceEquals(dbConnecion, temConn))
                {
                    temConn.Dispose();
                }
                if (ensureOpenned && dbConnecion.State == ConnectionState.Closed)
                {
                    dbConnecion.Open();
                }
                return dbConnecion;
            }
            catch (Exception exception)
            { 
                _logger.LogError(new EventId(GetHashCode(), "DapperContext"), exception, $"connName:{connName},ensureOpenned:{ensureOpenned}");
                throw exception.InnerException;
            }
        }

        private static void ThrowIfConnectionNotFound(string connName, string connectionString)
        {
            if (connectionString.IsNullOrWhiteSpace())
            {
                if (connName.IsNullOrWhiteSpace())
                {
                    throw new ArgumentException($"找不到默认数据库连接字符串配置。");
                }
                else
                {
                    throw new ArgumentException($"找不到名为 {connName} 的数据库连接字符串配置。");
                }
            }
        }

        /// <summary>
        /// 获取 Dapper 运行时环境。
        /// </summary>
        public DapperRuntime Runtime { get; }

        ~DapperContext()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                return;
            }
            this._disposed = true;
            if (disposing)
            {
                var conns = _connections.Values.ToArray();
                foreach (var c in conns)
                {
                    if (c.State == ConnectionState.Open)
                    {
                        c.Close();
                    }
                    c.Dispose();
                }

                _connections.Clear();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }


        public IDatabaseTransaction BeginTransaction(IsolationLevel level, string dbConnectionName)
        {
            var tran = this.GetConnection(dbConnectionName).BeginTransaction(level);
            return new DbTransactionWrapper(tran);
        }
    }
}
