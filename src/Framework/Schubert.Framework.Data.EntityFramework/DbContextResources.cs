using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using Microsoft.EntityFrameworkCore;

namespace Schubert.Framework.Data
{
    internal sealed class DbContextResources : IDatabaseContext
    {
        private IDictionary<String, Type> _contexts;
        private IServiceProvider _serviceProvider;
        private String _defaultConnectionName;

        public DbContextResources(IServiceProvider serviceProvider, String defaultConnectionName, IDictionary<String, Type> dbContexts)
        {
            Guard.ArgumentNotNull(dbContexts, nameof(dbContexts));
            _contexts = dbContexts;
            _serviceProvider = serviceProvider;
            _defaultConnectionName = defaultConnectionName;
        }

        public IDatabaseTransaction BeginTransaction(IsolationLevel level = IsolationLevel.ReadCommitted, string dbConnectionName = null)
        {
            var dbContext = this.GetDbContext(dbConnectionName);
            return dbContext.BeginTransaction(level);
        }

        public DbContext GetDbContext(String connectionName = null)
        {
            var name = connectionName ?? _defaultConnectionName;
            Type dbContextType = null;
            if (name.IsNullOrWhiteSpace())
            {
                var keypair = _contexts.FirstOrDefault();
                dbContextType = keypair.Value;
            }
            else
            {
                _contexts.TryGetValue(name, out dbContextType);
            }
            if (dbContextType == null)
            {
                throw new SchubertException($"没有配置任何的数据库连接字符串，或者没有配置任何 DbContext。");
            }
            return (DbContext)this._serviceProvider.GetRequiredService(dbContextType);
        }
    }
}
