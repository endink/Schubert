using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    /// <summary>
    /// 依赖项装配事件。
    /// </summary>
    public class DependencySetupEventArgs
    { 
        private ServiceDescriptor _actualDependency;

        public DependencySetupEventArgs(ServiceDescriptor serviceDescriptor)
        {
            this.OriginalDependency = serviceDescriptor;
            this.ActualDependency = ActualDependency;
        }

        /// <summary>
        /// 获取框架加载的原始的依赖项。
        /// </summary>
		public ServiceDescriptor OriginalDependency { get; }

        /// <summary>
        /// 获取或设置注入到 DI 中的实际依赖项（该属性默认和 <see cref="OriginalDependency"/> 是同一引用，可以设置该属性替换）。
        /// </summary>
        public ServiceDescriptor ActualDependency
        {
            get => this._actualDependency ?? this.OriginalDependency;
            set => _actualDependency = value;
        }
    }
}
