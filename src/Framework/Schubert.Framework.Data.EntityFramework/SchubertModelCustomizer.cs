using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Schubert.Framework.Data.Mappings;
using System;
using System.Collections.Generic;
using System.Text;

namespace Schubert.Framework.Data
{
    class SchubertModelCustomizer : RelationalModelCustomizer
    {
        public SchubertModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            var contextType = context.GetType();
            var options = context.GetDbOptions();
            var dbProvider = context.GetDbProvider();

            //当 Shell 没创建时候不会产生任何的 Mapping
            var mappings = context.GetApplicationServiceProvider().GetServices<IEntityMappings>();
            foreach (var mapping in mappings)
            {
                DbContextSelectionAttribute att = mapping.GetType().GetAttribute<DbContextSelectionAttribute>();
                DbContextAttribute att2 = mapping.GetType().GetAttribute<DbContextAttribute>();
                if ((att == null && att2 == null && contextType.Equals(options.DefaultDbContext)) || (att?.ContextType == contextType || att2?.ContextType == contextType))
                {
                    mapping.ApplyMapping(modelBuilder, dbProvider);
                }
            }


            bool mapBuiltinEntities = options.IncludeBuiltinEntities(contextType);
            if (mapBuiltinEntities)
            {
                foreach (var mapping in this.GetBuiltinMappings())
                {
                    mapping.ApplyMapping(modelBuilder, dbProvider);
                }
            }

        }

        /// <summary>
        /// 获取 Schubert 框架内置的映射。
        /// </summary>
        /// <returns></returns>
        private IEnumerable<IEntityMappings> GetBuiltinMappings()
        {
            yield return new LanguageMapping();
            yield return new PermissionMapping();
            yield return new PermissionRoleMapping();
            yield return new StringResourceMapping();
        }
    }
}
