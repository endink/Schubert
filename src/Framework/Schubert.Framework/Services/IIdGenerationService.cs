using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Services
{
    /// <summary>
    /// Id 生成服务。
    /// </summary>
    public interface IIdGenerationService
    {
        /// <summary>
        /// 获取一个唯一 Id。
        /// </summary>
        /// <returns></returns>
        long GenerateId();
    }
}
