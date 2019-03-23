using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Json
{
    public class ExtendedCamelCaseContractResolver : CamelCasePropertyNamesContractResolver
    {
        private JsonResolverSettings _settings;
        /// <summary>
        /// 创建 <see cref="ExtendedCamelCaseContractResolver"/> 的新实例。
        /// </summary>
        /// <param name="jsonResolverSettings">配置特殊的转换处理。</param>
        public ExtendedCamelCaseContractResolver(JsonResolverSettings jsonResolverSettings) :base()
        {
            Guard.ArgumentNotNull(jsonResolverSettings, nameof(jsonResolverSettings));
            this._settings = jsonResolverSettings;
        }

        private static bool IsLong(Type objectType)
        {
            return (objectType.Equals(typeof(long)) || objectType.Equals(typeof(long?)));
        }

        private static bool IsDateTime(Type objectType)
        {
            return (objectType.Equals(typeof(DateTime)) || objectType.Equals(typeof(DateTime?)) || objectType.Equals(typeof(DateTimeOffset)) || objectType.Equals(typeof(DateTimeOffset?)));
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            if (IsLong(objectType) && _settings.LongToString.Enable)
            {
                return new LongToStringContract(objectType);
            }
            if (IsDateTime(objectType) && _settings.DateTimeToString.Enable)
            {
                return new DateTimeToStringContract(objectType, _settings.DateTimeToString.InputFormat, _settings.DateTimeToString.OutputFormat);
            }
            return base.CreateContract(objectType);
        }

    }

    public class ExtendedContractResolver : DefaultContractResolver
    {
        private JsonResolverSettings _settings;
        /// <summary>
        /// 创建 <see cref="ExtendedCamelCaseContractResolver"/> 的新实例。
        /// </summary>
        /// <param name="jsonResolverSettings">配置特殊的转换处理。</param>
        public ExtendedContractResolver(JsonResolverSettings jsonResolverSettings) : base()
        {
            Guard.ArgumentNotNull(jsonResolverSettings, nameof(jsonResolverSettings));
            this._settings = jsonResolverSettings;
        }

        private static bool IsLong(Type objectType)
        {
            return (objectType.Equals(typeof(long)) || objectType.Equals(typeof(long?)));
        }

        private static bool IsDateTime(Type objectType)
        {
            return (objectType.Equals(typeof(DateTime)) || objectType.Equals(typeof(DateTime?)) || objectType.Equals(typeof(DateTimeOffset)) || objectType.Equals(typeof(DateTimeOffset?)));
        }

        protected override JsonContract CreateContract(Type objectType)
        {
            if (IsLong(objectType) && _settings.LongToString.Enable)
            {
                return new LongToStringContract(objectType);
            }
            if (IsDateTime(objectType) && _settings.DateTimeToString.Enable)
            {
                return new DateTimeToStringContract(objectType, _settings.DateTimeToString.InputFormat, _settings.DateTimeToString.OutputFormat);
            }
            return base.CreateContract(objectType);
        }

    }
}
