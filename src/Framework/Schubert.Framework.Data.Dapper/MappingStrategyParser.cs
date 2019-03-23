using Microsoft.Extensions.Options;
using Schubert.Framework.Environment;
using System.Text;

namespace Schubert.Framework.Data
{
    public static class MappingStrategyParser
    {
        private static IOptions<DapperDatabaseOptions> _dapperDatabaseOptions;

        private static DapperOptions DapperOptions
        {
            get
            {
                _dapperDatabaseOptions = _dapperDatabaseOptions ??
                    SchubertEngine.Current.GetRequiredService<IOptions<DapperDatabaseOptions>>();
                return _dapperDatabaseOptions.Value.Dapper;
            }
        }

        /// <summary>
        /// 按照映射策略获取类型名所映射的表名或属性名所映射的列名
        /// </summary>
        /// <param name="name">类型名或者属性名</param> 
        /// <returns></returns>
        public static string Parse(string name)
        {
            var strategy = DapperOptions?.IdentifierMappingStrategy ?? IdentifierMappingStrategy.Underline;
            var rule = DapperOptions?.CapitalizationRule ?? CapitalizationRule.LowerCase;
            return Parse(name, strategy, rule);
        }
        private static string Parse(string name, IdentifierMappingStrategy strategy)
        {
            if (strategy == IdentifierMappingStrategy.PascalCase)
            {
                return name;
            }
            var array = name.ToCharArray();
            var length = array.Length;
            var builder = new StringBuilder().Append(array[0]);
            var position = -1;
            for (var i = 1; i < length; i++)
            {
                var current = array[i];
                var prev = array[i - 1];
                if (char.IsUpper(current))
                {
                    if (char.IsLower(prev))
                    {
                        builder.Append("_").Append(current);
                        position = -1;
                    }
                    else
                    {
                        builder.Append(current);
                        position = i;
                    }
                }
                else
                {
                    builder.Append(current);
                }
            }
            if (position > 0 && char.IsLower(array[position]))
            {
                builder.Insert(position - 1, "_");
            }
            return builder.ToString();
        }
        private static string Parse(string name, IdentifierMappingStrategy strategy, CapitalizationRule rule)
        {
            var result = Parse(name, strategy);
            switch (rule)
            {
                case CapitalizationRule.LowerCase:
                    return result.ToLower();
                case CapitalizationRule.UpperCase:
                    return result.ToUpper();
                case CapitalizationRule.Original:
                    return result;
            }
            return result.ToLower();
        }
    }
}
