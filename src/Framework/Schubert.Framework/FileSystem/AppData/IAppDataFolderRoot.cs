using Schubert.Framework.Environment;

namespace Schubert.Framework.FileSystem.AppData {
    /// <summary>
    /// 实现对 "~/App_Data" 目录的抽象。
    /// </summary>
    public interface IAppDataFolderRoot {
        /// <summary>
        /// 根目录的应用程序路径 ("~/App_Data")。
        /// </summary>
        string RootPath { get; }
        /// <summary>
        /// 映射的物理的路径。
        /// </summary>
        string RootPhysicalFolder { get; }
    }

   
}