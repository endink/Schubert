using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Data
{
    /// <summary>
    /// 表示数据库选项。
    /// </summary>
    public class DatabaseOptions
    {
        private string _defaultConnectionName = null;
        private Dictionary<String, String> _connectionStrings = null;

        /// <summary>
        /// 获取或设置默认数据库连接字符串的键名称。
        /// </summary>
        public virtual string DefaultConnectionName
        {
            get { return _defaultConnectionName.IfNullOrWhiteSpace(this.ConnectionStrings.Keys.FirstOrDefault()); }
            set { _defaultConnectionName = value; }
        }

        /// <summary>
        /// 获取或设置连接字符串集合。
        /// </summary>
        public virtual Dictionary<String, String> ConnectionStrings
        {
            get { return _connectionStrings ?? (_connectionStrings = new Dictionary<string, string>(StringComparer.Ordinal)); }
            set { _connectionStrings = value; }
        } 
        /// <summary>
        /// 获取数据库连接字符串。
        /// </summary>
        /// <returns></returns>
        public virtual string GetConnectionString(string connectionName = null, bool throwIfNotExisted = true)
        {
            connectionName = connectionName.IfNullOrWhiteSpace(_defaultConnectionName);
            if (!connectionName.IsNullOrWhiteSpace())
            {
                string conn = this.ConnectionStrings.GetOrDefault(connectionName);
                if (conn.IsNullOrWhiteSpace() && throwIfNotExisted)
                {
                    throw new SchubertException($"在配置中找不到名为 '{connectionName}' 的数据库连接字符串。");
                }
                return conn;
            }
            else
            {
                String conn = this.ConnectionStrings.FirstOrDefault().Value;
                if (conn.IsNullOrWhiteSpace() && throwIfNotExisted)
                {
                    throw new SchubertException($"没有配置任何可用的数据库连接字符串，或连接字符串为空。");
                }
                return conn;
            }
        }
    }
}
