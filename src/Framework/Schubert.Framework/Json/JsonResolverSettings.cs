using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Json
{
    public class JsonResolverSettings
    {
        private LongToStringSettings _longToStringSettings;
        private DateTimeToStringSettings _dateTimeToStringSettings;

        /// <summary>
        /// 获取或设置 JSON 序列化和反序列化时对 <see cref="long"/> 处理的设置。
        /// </summary>
        public LongToStringSettings LongToString
        {
            get { return _longToStringSettings ?? (_longToStringSettings = new LongToStringSettings()); }
            set { _longToStringSettings = value; }
        }

        /// <summary>
        /// 获取或设置 JSON 序列化和反序列化时对 <see cref="DateTime"/> 处理的设置。
        /// </summary>
        public DateTimeToStringSettings DateTimeToString
        {
            get { return _dateTimeToStringSettings ?? (_dateTimeToStringSettings = new DateTimeToStringSettings()); }
            set { _dateTimeToStringSettings = value; }
        }


        public class LongToStringSettings
        {
            /// <summary>
            /// 是否在序列化时将 <see cref="long"/>  序列化为 <see cref="string"/>, 同时可以将 <see cref="string"/> 值处理为 <see cref="long"/>.
            /// </summary>
            public bool Enable { get; set; }
        }

        public class DateTimeToStringSettings
        {
            /// <summary>
            /// 是否在序列化时将 <see cref="DateTime"/>、<see cref="DateTimeOffset"/>  序列化为 <see cref="string"/>, 同时可以将 <see cref="string"/> 值处理为 <see cref="DateTime"/>、<see cref="DateTimeOffset"/> .
            /// </summary>
            public bool Enable { get; set; }

            /// <summary>
            /// 获取或设置日期时间输入（反序列化）格式。
            /// </summary>
            public String InputFormat { get; set; }

            /// <summary>
            /// 获取或设置日期时间输出（序列化）格式（如果不设置，默认为 yyyy-MM-dd HH:mm:ss）。
            /// </summary>
            public String OutputFormat { get; set; }
        }
    }
}
