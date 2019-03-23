using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schubert.Framework.Caching;
using Schubert.Framework.Data;
using Schubert.Framework.Environment;
using Schubert.Framework.Environment.Modules;
using Schubert.Framework.Environment.Modules.Finders;
using Schubert.Framework.Environment.Modules.Loaders;
using Schubert.Framework.Environment.ShellBuilders;
using Schubert.Framework.Environment.ShellBuilders.BuiltinExporters;
using Schubert.Framework.Events;
using Schubert.Framework.FileSystem;
using Schubert.Framework.FileSystem.AppData;
using Schubert.Framework.Localization;
using Schubert.Framework.Services;
using Schubert.Framework.Web;
using System.Collections.Generic;

namespace Schubert.Framework.DependencyInjection
{
    /// <summary>
    /// 提供 Schubert Framework 必须的底层服务。
    /// </summary>
    /// 为什么需要这个类而不用 IDependency 接口自动注册？
    /// 因为微软目前提供的 IServiceCollection 不能提供注册顺序。
    /// 我们希望自己的扩展重写一些框架的实现（例如：MVC, EntityFramework）
    /// MVC 等框架在注册是使用 TryAdd 方法，因此我们只需要在他之前注册服务就可以覆盖他的原生服务。
    public static class SchubertServices
    {
        /// <summary>
        /// 获取框架中的基础服务。
        /// </summary>
        public static IEnumerable<SmartServiceDescriptor> GetServices(IConfiguration configuration)
        {
            //基础运行环境。

            yield return ServiceDescriber.Singleton(typeof(IOptions<>), typeof(OptionsManager<>));
            yield return ServiceDescriber.Singleton<IInstanceIdProvider, DefaultInstanceIdProvider>();
            string configValue = configuration["Schubert:Env"];
            yield return ServiceDescriber.Transient<ISchubertEnvironment>(s => 
            new DefaultRuntimeEnvironment(configValue ?? "Production", s.GetRequiredService<IInstanceIdProvider>()));
            
            //系统内置服务
            yield return ServiceDescriber.Singleton<ICacheManager, MemoryCacheManager>();

            //事件通知
            yield return ServiceDescriber.Singleton<IEventNotification, EventNotification>();

            //文件系统
            yield return ServiceDescriber.Singleton<IFileStorageManager, FileStorageManager>();
            yield return ServiceDescriber.Transient<IAppDataFolderRoot, AppDataFolderRoot>();
            yield return ServiceDescriber.Transient<IAppDataFolder, AppDataFolder>();

            //运行环境基础服务
            yield return ServiceDescriber.Singleton<IIdGenerationService, SnowflakeIdGenerationService>();
            yield return ServiceDescriber.Transient<IShellDescriptorManager, NullShellDescriptorManager>();
            yield return ServiceDescriber.Transient<IAssemblyReader, DefaultAssemblyReader>();
            yield return ServiceDescriber.Transient<IPathProvider, DefaultPathProvider>();

            yield return ServiceDescriber.Singleton<IWorkContextAccessor, DefaultWorkContextAccessor>();
            yield return ServiceDescriber.Scoped<WorkContext>(sp=>sp.GetRequiredService<IWorkContextAccessor>().GetContext());
            yield return ServiceDescriber.Scoped<IWorkContextStateProvider, UserStateProvider>(SmartOptions.Append);
            yield return ServiceDescriber.Scoped<IWorkContextStateProvider, LanguageStateProvider>(SmartOptions.Append);
            yield return ServiceDescriber.Scoped<IWorkContextStateProvider, TimeZoneStateProvider>(SmartOptions.Append);
            yield return ServiceDescriber.Scoped<IWorkContextStateProvider, TransactionStateProvider>(SmartOptions.Append);

            //模块化功能
            yield return ServiceDescriber.Transient<IModuleHarvester, XmlHarvester>(SmartOptions.Append); // IModuleHarvester 可以存在多个实例
            yield return ServiceDescriber.Transient<IModuleHarvester, JsonHarvester>(SmartOptions.Append);

            yield return ServiceDescriber.Transient<IModuleFinder, EntryPointFinder>(SmartOptions.Append);
            yield return ServiceDescriber.Transient<IModuleFinder, RunningFolderFinder>(SmartOptions.Append);
            yield return ServiceDescriber.Transient<IModuleLoader, ClrModuleLoader>(SmartOptions.Append);  //该服务可以存在多个实例

            yield return ServiceDescriber.Transient<IShellDescriptorCache, NullShellDescriptorCache>();
            yield return ServiceDescriber.Transient<IModuleManager, ModuleManager>(); 
            yield return ServiceDescriber.Transient<IFeatureManager, FeatureManager>();
            
            yield return ServiceDescriber.Transient<IShellBlueprintItemExporter, DependencyDescriberExporter>(SmartOptions.Append);
            yield return ServiceDescriber.Transient<IShellBlueprintItemExporter, DependencyExporter>(SmartOptions.Append);
            yield return ServiceDescriber.Transient<IShellBlueprintItemExporter, OptionsExporter>(SmartOptions.Append);

            yield return ServiceDescriber.Singleton<IInstanceIdProvider, DefaultInstanceIdProvider>();
            yield return ServiceDescriber.Singleton<IInstanceIdProvider, DefaultInstanceIdProvider>();

            //Shell 服务
            yield return ServiceDescriber.Transient<ICompositionStrategy, CompositionStrategy>(); 
            yield return ServiceDescriber.Transient<IShellContextFactory, ShellContextFactory>();

            //全球化
            yield return ServiceDescriber.Singleton<ILocalizedStringManager, LocalizedStringManager>();

            yield return ServiceDescriber.Scoped<IPermissionService, NullPermissionService>();
            yield return ServiceDescriber.Scoped<ILanguageService, NullLanguageService>();
            yield return ServiceDescriber.Scoped<IIdentityService, NullIdentityService>();
            yield return ServiceDescriber.Singleton<IDistributedOptimisticStoreService, DebugOnlyFileStoreService>();
            

            //日志
            yield return ServiceDescriber.Scoped<ILogService, NullLogService>();

        }
    }
}