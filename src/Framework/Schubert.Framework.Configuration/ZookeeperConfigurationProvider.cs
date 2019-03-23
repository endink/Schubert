using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Schubert.Framework.Environment;
using Schubert.Framework.Security.Cryptography;
using Schubert.Zookeeper;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Configuration
{
    /// <summary>
    /// 表示一个以 Zookeeper 作为配置源的配置提供程序。
    /// </summary>
    public class ZookeeperConfigurationProvider : ConfigurationProvider
    {
        public const string ConfigurationNode = "config";
        private string _rootNode = "";
        private bool? _nodeExisted;
        private ILogger _logger = null;
        private ISchubertEnvironment _environment = null;
        private bool _firstLoaded = true;
        private ConfigurationCenterOptions _options = null;
        private static ZookeeperClient _zkClient = null;
        private static readonly Object ZkClientSync = new Object();
        private static String _key = null;
        private string _nodeName = null;
        

        public ZookeeperConfigurationProvider(ConfigurationCenterOptions configurationCenterOptions, String node = null)
        {
            Guard.ArgumentNotNull(configurationCenterOptions, nameof(configurationCenterOptions));
            if (configurationCenterOptions.ConnectionString.IsNullOrWhiteSpace())
            {
                throw new ArgumentException($@"配置中心缺少连接字符串配置（configuration section: configuration : {nameof(ZookeeperClientSettings.ConnectionString)}）。");
            }
            _options = configurationCenterOptions;
            _nodeName = node.IfNullOrWhiteSpace("application");
            _rootNode = _options.NodeBasePath;
            if (_rootNode?.IndexOf('{') >= 0) _rootNode = "";
        }

        public override void Load()
        {
            if (!SchubertEngine.Current.IsRunning)
            {
                ShellEvents.EngineStarted += ShellEvents_OnEngineStarted;
            }
            else
            {
                var client = this.CreateOrGetZookeeperClient();
                this.LoadRemoteConfigurationAsync(client, _firstLoaded).GetAwaiter().GetResult();
            }
        }

        private async Task LoadRemoteConfigurationAsync(ZookeeperClient client, bool firstLoad = false)
        {
            _firstLoaded = false;
            _logger = (_logger ?? SchubertEngine.Current.GetService<ILoggerFactory>()?.CreateLogger(this.GetType().Name)) ?? NullLogger.Instance;
            _environment = (_environment ?? SchubertEngine.Current.GetRequiredService<ISchubertEnvironment>());

            string path = $"{this.RootNodePath.TrimEnd('/')}/{_nodeName}";
            if (await CheckNodeExistedAsync(client, path, firstLoad))
            {
                await LoadConfigurationDataAsync(client, firstLoad, path);
            }
        }

        private async Task LoadConfigurationDataAsync(ZookeeperClient client, bool firstLoad, string path)
        {
            try
            {
                var data = (await client.GetDataAsync(path)).ToArray();
                if (firstLoad)
                {
                    await client.SubscribeDataChange(path, this.OnNodeDataChangeAsync);
                }
                if (data != null)
                {
                     LoadKey();
                    
                    data = OpenSSL.RSADecrypt(data, _key);
                       
                    
                    //data = OpenSSL.RSADecrypt(data, _key);

                    string json = String.Empty;
                    if (data.Length <= 3) throw new Exception("配置中心返回了错误的配置数据");
                    if (data[0] == 0xef && data[1] == 0xbb && data[2] == 0xbf)
                        json = new UTF8Encoding(false).GetString(data, 3, data.Length - 3);
                    else
                        json = Encoding.UTF8.GetString(data);
                    var parser = new JsonConfigurationParser();

                    //配置中心不能影响本地的用于构造配置节点路径的配置，否则将造成拉取错误配置。
                    this.Data = parser.Parse(json,
                        ConfigurationPath.Combine("Schubert", "Group"),
                        ConfigurationPath.Combine("Schubert", "AppSystemName"),
                        ConfigurationPath.Combine("Schubert", "Version"),
                        ConfigurationPath.Combine("Schubert", "Env"),
                        ConfigurationPath.Combine("Schubert", "Configuration"));
                    OnReload();
                    //Microsoft.Extensions.Configuration.FileConfigurationProvider的源码有这个代码OnReload()
                    //OnReload会让GetReloadToken通知ChangeToken.OnChange去调用changeTokenConsumer的委托
                    //changeTokenConsumer的委托中如果走load等同逻辑的代码,机会再次调用OnReload,然后就挂
                    //ChangeToken.OnChange方案的用处是用来做外置事件源的处理,不能做内部事件调用
                    //Action reload = OnReload;
                    //reload();
                    //不加OnReload,完全不起作用,但是为什么叫OnReload
                }
            }
            catch (TimeoutException ex)
            {
                string error = $"连接到配置中心（zk_server: {client.Options.ConnectionString }）超时。";
                HandleError(firstLoad, error, ex);
            }
            catch (FormatException ex)
            {
                string error = $"从配置中心加载配置发生错误，可能由于配置文件格式错误（zk_server: {client.Options.ConnectionString }, paht:{path}）超时。";
                HandleError(firstLoad, error, ex);
            }
        }

        private static void LoadKey()
        {
            if (_key == null)
            {
                var assembly = typeof(ZookeeperConfigurationProvider).GetTypeInfo().Assembly;
                using (Stream keyStream = assembly.GetManifestResourceStream($"{typeof(ZookeeperConfigurationProvider).Namespace}.rsa_private_key.pem"))
                {
                    _key = Encoding.UTF8.GetString(keyStream.ReadBytesToEnd()).IfNull(String.Empty);
                }
            }
        }


        /// <summary>
        /// 处理服务器错误（仅在第一次启动时候抛出异常，watch 回掉中发生错误错误认为节点无变化）。
        /// </summary>
        private void HandleError(bool firstLoad, string error, Exception causeException = null)
        {
            _logger?.WriteError(error, causeException);
            if (!_environment.IsDevelopmentEnvironment && firstLoad)
            {
                throw new ConfigurationException(error, causeException);
            }
        }

        private async Task<bool> CheckNodeExistedAsync(ZookeeperClient client, string path, bool firstLoad)
        {
            if (!this._nodeExisted.HasValue)
            {
                try
                {
                    this._nodeExisted = await client.ExistsAsync(path);
                }
                catch (TimeoutException ex)
                {
                    string error = $"连接到配置中心（zk_server: {client.Options.ConnectionString }）超时。";
                    this.HandleError(firstLoad, error, ex);
                }
            }
            if (!(this._nodeExisted ?? false))
            {
                string error = $"配置中心中找不到指定节点（path: {path}, zk_server: {client.Options.ConnectionString }）超时。";
                this.HandleError(firstLoad, error);
            }
            return (this._nodeExisted ?? false);
        }

        private Task OnNodeDataChangeAsync(ZookeeperClient client, NodeDataChangeArgs args)
        {
            return this.LoadRemoteConfigurationAsync(client);
        }

        private ZookeeperClient CreateOrGetZookeeperClient()
        {
            ZookeeperClientSettings clientSettings = _options.Zookeeper;
            if (_zkClient == null)
            {
                lock (ZkClientSync)
                {
                    if (_zkClient == null)
                    {
                        _zkClient = new ZookeeperClient(clientSettings);
                    }
                }
            }
            return _zkClient;
        }

        /// <summary>
        /// 获取当前的应用程序节点路径。
        /// </summary>
        protected string RootNodePath
        {
            get
            {
                if (_rootNode.IsNullOrWhiteSpace())
                {
                    var options = SchubertEngine.Current.GetRequiredService<IOptions<SchubertOptions>>();
                    var env = SchubertEngine.Current.GetRequiredService<ISchubertEnvironment>().Environment;
                    _rootNode = SchubertEngine.Current.GetRequiredService<IOptions<ConfigurationCenterOptions>>().Value.NodeBasePath
                        .Replace("{env}", env.ToLower())
                        .Replace("{group}", options.Value.Group)
                        .Replace("{appname}", options.Value.AppSystemName)
                        .Replace("{version}", options.Value.Version)
                        .Replace("\\", "/")
                        .TrimEnd('/');
                }
                return _rootNode;
            }
        }

        private void ShellEvents_OnEngineStarted(SchubertOptions options, IServiceProvider serviceProvider)
        {
            var client = this.CreateOrGetZookeeperClient();
            this.LoadRemoteConfigurationAsync(client, true).GetAwaiter().GetResult();
        }
    }
}
