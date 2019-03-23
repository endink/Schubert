using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Environment.Modules
{
    /// <summary>
    /// 表示未提供 <see cref="IShellDescriptorManager"/> 的实现。
    /// </summary>
    public class NullShellDescriptorManager : IShellDescriptorManager
    {
        public ShellDescriptor GetShellDescriptor()
        {
            return null;
        }

        public void UpdateShellDescriptor(IEnumerable<string> enabledFeatures, IEnumerable<ShellParameter> parameters)
        {
            
        }
    }
}
