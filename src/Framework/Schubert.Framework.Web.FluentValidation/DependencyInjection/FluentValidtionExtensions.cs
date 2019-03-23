using Schubert.Framework.Web;
using Schubert.Framework.Web.DependencyInjection;
using System;

namespace Schubert
{
    public static class FluentValidtionExtensions
    {
        private static Guid _module = Guid.NewGuid();

        /// <summary>
        /// 为 MVC 添加 FluentValidation 支持。
        /// </summary>
        /// <param name="builder"></param>
        public static void AddFluentValidationForMvc(this SchubertWebBuilder builder)
        {
            if (builder.AddedModules.Add(_module))
            {
                builder.AddStarter(new FluentValidationStarter());
            }
        }

        

    }
}
