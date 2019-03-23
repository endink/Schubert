using System.Threading.Tasks;
using System;

namespace Schubert.Framework.Services
{
    /// <summary>
    /// 分布式乐观并发存储服务（在写入数据时支持多线程乐观并发）。
    /// </summary>
    public interface IDistributedOptimisticStoreService : ISingletonDependency
    {
        /// <summary>
        /// 当初次创建时候写入的数据。
        /// </summary>
        string FirstCreationData { get; set; } 

        /// <summary>
        /// 获取数据。
        /// </summary>
        /// <param name="scopeName">作用域名称。</param>
        /// <returns></returns>
        Task<String> GetDataAsync(string scopeName);

        /// <summary>
        /// 尝试写入数据（在类中实现时要求支持多线程乐观并发支持）。
        /// </summary>
        /// <param name="scopeName">作用域名称。</param>
        /// <param name="data">要写入的数据。</param>
        /// <returns>如果写入失败（并发），返回 flase，否则返回 true。</returns>
        Task<bool> TryWriteDataAsync(string scopeName, string data);
    }
}
