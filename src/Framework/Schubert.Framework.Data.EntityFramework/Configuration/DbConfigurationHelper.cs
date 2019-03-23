using Schubert.Framework.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Schubert;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace Schubert.Framework.Configuration
{
    internal static class DbConfigurationHelper
    {
        public static IDictionary<String, String> ReadData(IRepository<SettingRecord> repository, string region = "global")
        {
            Guard.ArgumentNullOrWhiteSpaceString(region, nameof(region));
            Guard.ArgumentNotNull(repository, nameof(repository));
            Dictionary<String, String> dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            var settings = repository.TableNoTracking.Where(s=>s.Region == region).OrderBy(s => s.Name).ToArray();
            foreach (var s in settings)
            {
                dictionary[s.Name] = s.RawValue;
            }
            return dictionary;
        }

        /// <summary>
        /// 写入配置数据，仅写入不存在的节点数据，存在的节点数据即使有变化也不会写入。
        /// </summary>
        public static void WirteData(IRepository<SettingRecord> repository, IDictionary<String, String> data, string region = "global")
        {
            Guard.ArgumentNullOrWhiteSpaceString(region, nameof(region));
            Guard.ArgumentNotNull(repository, nameof(repository));
            IDictionary<String, String> dictionary = data ?? new Dictionary<String, String>();
            
            if (data.Count > 0)
            {
                var names = dictionary.Keys.ToArray();
                var exsitingSettings = repository.Table.Where(s => s.Region == region).Where(s => names.Contains(s.Name)).ToArray();
                
                foreach (var name in names)
                {
                    var settings = exsitingSettings.FirstOrDefault(s => s.Name.CaseInsensitiveEquals(name));
                    if (settings != null)
                    {
                        settings.RawValue = dictionary[name]?.Trim();
                    }
                    else
                    {
                        repository.Insert(new SettingRecord { Region = region, Name = name, RawValue = dictionary[name] });
                    }
                }
                repository.CommitChanges();
            }
        }
    }
}
