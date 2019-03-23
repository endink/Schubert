using Microsoft.Extensions.Logging;
using Schubert.Framework.Environment;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Schubert.Helpers;

namespace Schubert.Framework
{
    /// <summary>
    /// Schubert 框架选项。
    /// </summary>
    public class SchubertOptions
    {
        private string _defaultCulture;
        private string _defaultTimeZone;
        private string _version;
        private string _systemName;
        private string _groupName;
        private static readonly Regex GroupNamePattern = new Regex(@"^[a-z0-9]+$");
        private static readonly Regex AppSystemNameNamePattern = new Regex(@"^[a-z0-9-]+$");


        public SchubertOptions()
        {
            _systemName = "UnnameApp";
            _groupName = "UnnamedGroup";
        }

        public string GetFullAppSystemName()
        {
            return String.Format($"{this.Group}-{this.AppSystemName}");
        }

        public string Group
        {
            get { return _groupName; }
            set
            {
                if (_groupName != value)
                {
                    if (SchubertEngine.Current.ShellCreated)
                    {
                        new SchubertException("不能在 Shell 创建完成后更改组织名称（Group）名称。");
                    }
                    if (value.IsNullOrWhiteSpace())
                    {
                        throw new ArgumentNullException("应用程序系统名称不能为空。");
                    }
                    string valueString = value.Trim();
                    if (!GroupNamePattern.IsMatch(valueString))
                    {
                        throw new ArgumentException($"应用组织名称中不能包含非法字符（只能包含小写字母，数字）。");
                    }

                    _groupName = valueString;
                }
            }
        }

        /// <summary>
        /// 应用程序系统名称（默认为 "UnnamedApp"）。
        /// </summary> 
        public string AppSystemName
        {
            get { return _systemName.IfNullOrWhiteSpace("UnnamedApp"); }
            set
            {
                if (_systemName != value)
                {
                    if (SchubertEngine.Current.ShellCreated)
                    {
                        new SchubertException("不能在 Shell 创建完成后更改应用程序系统名称。");
                    }
                    if (value.IsNullOrWhiteSpace())
                    {
                        throw new ArgumentNullException("应用程序系统名称不能为空。");
                    }
                    string valueString = value.Trim();
                    if (!AppSystemNameNamePattern.IsMatch(valueString))
                    {
                        throw new ArgumentException($"应用程序系统名称中不能包含非法字符（只能包含小写字母，数字，减号）。");
                    }

                    _systemName = valueString;
                }
            }
        }

        /// <summary>
        /// 应用程序名称（默认为空）。
        /// </summary>
        public string AppName { get; set; } = String.Empty;

        /// <summary>
        /// 应用程序版本（默认为 1.0）。
        /// </summary>
        public string Version
        {
            get { return _version.IfNullOrWhiteSpace("1.0"); }
            set { _version = value; }
        }

        /// <summary>
        /// 获取或设置网站默认的特定区域名称（默认为 "zh-Hans" ， 即简体中文）。
        /// </summary>
        public string DefaultCulture
        {
            get { return _defaultCulture.IfNullOrWhiteSpace("zh-Hans"); }
            set { _defaultCulture = value; }
        }

        /// <summary>
        ///获取或设置网站默认时区（默认: Asia/Shanghai，即中国北京时间）。
        /// </summary>
        public string DefaultTimeZone
        {
            get { return _defaultTimeZone.IfNullOrWhiteSpace("Asia/Shanghai"); }
            set { _defaultTimeZone = value; }
        }


    }
}
