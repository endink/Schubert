using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Schubert.Framework;
using Schubert.Framework.Domain;
using Schubert.Framework.Environment;
using Schubert.Framework.FileSystem;
using Schubert.Framework.FileSystem.AppData;
using Schubert.Framework.Localization;
using Schubert.Framework.Logging;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Schubert
{
    public static class SchubertUtility
    {
        private static bool? _isInVisualStudio;
        /// <summary>
        /// 匿名用户用户名。
        /// </summary>
        public const string AnonymousUserName = "Anonymous";

        /// <summary>
        /// 超级管理员用户名。
        /// </summary>
        public const string AdministratorUserName = "Administrator";

        private static IPAddress[] _ipAddresses = null;
        private static String _currentIpAddress = null;

        /// <summary>
        /// 获取配置中的环境。
        /// </summary>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static String GetConfiguredEnvironment(this IConfiguration configuration)
        {
            if (configuration == null)
            {
                return "production";
            }
            string aspEnv = configuration["ASPNETCORE_ENVIRONMENT"];
            return aspEnv.IfNullOrWhiteSpace(configuration["Schubert:Env"]).IfNullOrWhiteSpace("production");
        }

        public static String GetApplicationDirectory()
        {
            var assembly = Assembly.GetEntryAssembly();
            return assembly == null ? Directory.GetCurrentDirectory() : Path.GetDirectoryName(new Uri(assembly.CodeBase).LocalPath);
        }

        /// <summary>
        /// 提供跨平台的文件夹路径分隔符（对于 linux 是 '/'， 对于 windows 是 '\'）
        /// </summary>
        public static char DirectorySeparator
        {
            get { return RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ?  '\\' : '/'; }
        }

        /// <summary>
        /// 判断用户是否是匿名用户。
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsAnonymous(this UserBase user)
        {
            return user.UserName.CaseInsensitiveEquals(AnonymousUserName);
        }

        /// <summary>
        /// 判断用户是否是超级管理员。
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static bool IsAdministrator(this UserBase user)
        {
            return user.UserName.CaseInsensitiveEquals(AdministratorUserName);
        }

        /// <summary>
        /// 获取系统中的文本本地化对象。
        /// </summary>
        /// <param name="context">当前的工作上下文。</param>
        /// <returns>文本本地化操作的委托。</returns>
        public static Localizer GetLocalizer(this WorkContext context)
        {
            var manager = context.Resolve<ILocalizedStringManager>();
            if (manager == null)
            {
                return NullLocalizer.Instance;
            }
            else
            {
                return (key, defaultString, args) =>
                {
                    Guard.ArgumentNotNull(key, nameof(key));
                    string result = manager.GetLocalizedString(context.CurrentLanguage.Culture, key).IfNullOrWhiteSpace(defaultString);
                    return result;
                };
            }
        }


        private static string GetSeoName(this CultureInfo culture)
        {
            if (culture == null || !culture.IsNeutralCulture)
            {
                return null;
            }
            CultureInfo info = culture;
            while (info.Parent != null && info.Parent.IsNeutralCulture)
            {
                info = culture.Parent;
            }
            return info.EnglishName;
        }

        private static string InnerCombinePath(String basePath, String relativePath)
        {
            Guard.ArgumentIsRelativePath(relativePath, nameof(relativePath));

            Uri root = new Uri(basePath.TrimEnd('/').TrimEnd('\\') + "/", UriKind.RelativeOrAbsolute);
            var combined = new Uri(root, relativePath);
            return combined.LocalPath;
        }

        public static string CombinePath(params string[] paths)
        {
            Guard.ArgumentNotNullOrEmptyArray(paths, nameof(paths));

            String path = null;
            for (int i = 0; i < (paths.Length); i++)
            {
                path = (path == null) ? paths[i] : InnerCombinePath(path, paths[i]);
            }
            return path;
        }


        public static Language CreateLanguage(string culture)
        {
            CultureInfo cultureInfo = null;
            try
            {
                cultureInfo = new CultureInfo(culture.IfNullOrWhiteSpace("zh-CN"));
            }
            catch (CultureNotFoundException)
            {
                cultureInfo = CultureInfo.InvariantCulture;
            }
            return new Language
            {
                Culture = culture,
                DisplayName = cultureInfo.DisplayName,
                UniqueSeoCode = cultureInfo.GetSeoName()
            };
        }

        /// <summary>
        /// 指示当前环境是否为 Visual Studio 开发环境。
        /// </summary>
        /// <returns></returns>
        public static bool InVisualStudio()
        {
            if (!_isInVisualStudio.HasValue)
            {
                _isInVisualStudio = false;
                string directory = Directory.GetCurrentDirectory();
                var baseDirectory = AppContext.BaseDirectory.Replace(directory, string.Empty).TrimStart('/', '\\');
                string folderName = Path.GetFileName(directory);
                if (folderName.CaseInsensitiveEquals("TESTWINDOW")) //Unit test 试环境
                {
                    return true;
                }
                string asp5Directory = Path.Combine(directory, "wwwroot");
                if (Directory.Exists(asp5Directory))
                {
                    _isInVisualStudio = Directory.EnumerateFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly)?.Any() ?? false;
                }
                {
                    if (folderName.CaseInsensitiveEquals("DEBUG") || folderName.CaseInsensitiveEquals("RELEASE"))
                    {
                        directory = Path.GetDirectoryName(directory);
                        string parentName = Path.GetFileName(directory);
                        if (parentName.CaseInsensitiveEquals("bin"))
                        {
                            string projectDire = Path.GetDirectoryName(directory);
                            _isInVisualStudio = Directory.EnumerateFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly)?.Any() ?? false;
                        }
                    }
                    else if (baseDirectory.IndexOf("DEBUG", StringComparison.OrdinalIgnoreCase) != -1 || baseDirectory.IndexOf("RELEASE", StringComparison.OrdinalIgnoreCase) != -1)
                    {
                        if (baseDirectory.StartsWith("bin", StringComparison.OrdinalIgnoreCase))
                        {
                            _isInVisualStudio = Directory.EnumerateFiles(directory, "*.csproj", SearchOption.TopDirectoryOnly)?.Any() ?? false;
                        }
                    }
                }
            }
            return _isInVisualStudio.Value;
        }

        /// <summary>
        /// 获取 Windows 用户文件夹。
        /// </summary>
        /// <returns></returns>
        public static string GetUserDirectory()
        {
            return System.Environment.GetEnvironmentVariable("USERPROFILE");
        }

        /// <summary>
        /// 获取文件的 Mime 类型。
        /// </summary>
        /// <returns></returns>
        public static string GetMimeType(this IFile file)
        {
            return Helpers.MimeTypeHelper.GetExtension(Path.GetExtension(file.Name));
        }

        /// <summary>
        /// 优先根据当前网络配置 （<see cref="NetworkOptions"/>）获取 ip 地址，如果失败获取第一个网卡的地址，如果还无法获取，返回 null。
        /// </summary>
        /// <returns></returns>
        public static String GetCurrentIPAddress()
        {
            if (_currentIpAddress == null)
            {
                if (SchubertEngine.Current.IsRunning)
                {
                    var networkOptions = SchubertEngine.Current.GetService<IOptions<NetworkOptions>>();
                    String ip = String.Empty;
                    if (networkOptions?.Value?.TryGetIPAddress(out ip) ?? false)
                    {
                        _currentIpAddress = ip;
                        return _currentIpAddress;
                    }
                }
                var addresses = GetLocalIPV4Addresses();
                if (addresses.IsNullOrEmpty())
                {
                    return String.Empty;
                }
                return addresses.FirstOrDefault().ToString();
            }
            return _currentIpAddress;
        }

        /// <summary>
        /// 获取本机 IPV4 地址。
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<IPAddress> GetLocalIPV4Addresses()
        {
            if (_ipAddresses == null)
            {
                var addresses = from item in NetworkInterface.GetAllNetworkInterfaces()
                                where item.NetworkInterfaceType == NetworkInterfaceType.Wireless80211
                                      || item.NetworkInterfaceType == NetworkInterfaceType.Ethernet
                                from ipc in item.GetIPProperties().UnicastAddresses
                                let address = ipc.Address
                                where address.AddressFamily == AddressFamily.InterNetwork
                                select address;
                _ipAddresses = addresses.ToArray();
            }
            return _ipAddresses;
        }

        /// <summary>
        /// 根据根路径获取相对路径。
        /// </summary>
        /// <param name="basePath">根路径。</param>
        /// <param name="fullPath">完整路径。</param>
        /// <param name="stringComparer">字符串比较策略。</param>
        /// <returns>获得一个基于根路径的相对路径。</returns>
        public static string GetRetrivePath(string basePath, string fullPath, StringComparison stringComparer = StringComparison.OrdinalIgnoreCase)
        {
            Guard.ArgumentNullOrWhiteSpaceString(basePath, nameof(basePath));
            if (fullPath.IsNullOrWhiteSpace())
            {
                return String.Empty;
            }
            int index = fullPath.IndexOf(basePath, stringComparer);
            if (index == -1)
            {
                return String.Empty;
            }
            return fullPath.Remove(0, basePath.Length).TrimStart('\\').TrimStart('/');
        }

        /// <summary>
        /// 将协调世界时（UTC）显示为本地时间（如果 <paramref name="timeUtc"/> 为空，返回空串）。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="timeUtc">要显示的 UTC 日期。</param>
        /// <param name="format">格式化字符串。</param>
        /// <returns></returns>
        public static string DisplayDateTimeUtc(this WorkContext context, DateTime? timeUtc, string format = "yyyy-MM-dd")
        {

            return timeUtc.HasValue ? context.DisplayDateTimeUtc(timeUtc.Value, format) : String.Empty;
        }

        /// <summary>
        /// 将协调世界时（UTC）显示为本地时间。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="timeUtc">要显示的 UTC 日期。</param>
        /// <param name="format">格式化字符串。</param>
        /// <returns></returns>
        public static string DisplayDateTimeUtc(this WorkContext context, DateTime timeUtc, string format = "yyyy-MM-dd")
        {
            format = format.IfNullOrWhiteSpace("yyyy-MM-dd");
            DateTime time = TimeZoneInfo.ConvertTime(timeUtc, TimeZoneInfo.Utc, context.CurrentTimeZone);

            return time.ToString(format);
        }

        /// <summary>
        /// 根据用户当前所在时区，获取UTC时间的本地时间。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="timeUtc"></param>
        /// <returns></returns>
        public static DateTime? GetLocalDateTime(this WorkContext context, DateTime? timeUtc)
        {
            return timeUtc.HasValue ? (DateTime?)context.GetLocalDateTime(timeUtc.Value) : null;
        }

        /// <summary>
        /// 根据用户当前所在时区，获取UTC时间的本地时间。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="timeUtc"></param>
        /// <returns></returns>
        public static DateTime? GetUtcDateTime(this WorkContext context, DateTime? timeUtc)
        {
            return timeUtc.HasValue ? (DateTime?)context.GetUtcDateTime(timeUtc.Value) : null;
        }

        /// <summary>
        /// 根据用户当前所在时区，获取UTC时间的本地时间。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="timeUtc"></param>
        /// <returns></returns>
        public static DateTime GetLocalDateTime(this WorkContext context, DateTime timeUtc)
        {
            if (timeUtc.Kind != DateTimeKind.Utc)
            {
                timeUtc.ChangeKind(DateTimeKind.Utc);
            }
            var destTimeZone = context.CurrentTimeZone;
            return TimeZoneInfo.ConvertTime(timeUtc, TimeZoneInfo.Utc, destTimeZone);
        }

        /// <summary>
        /// 根据用户当前所在时区，获取时间的UTC表示形式。
        /// </summary>
        /// <param name="context"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime GetUtcDateTime(this WorkContext context, DateTime time)
        {
            if (time.Kind == DateTimeKind.Unspecified)
            {
                time.ChangeKind(DateTimeKind.Unspecified);
            }
            var destTimeZone = context.CurrentTimeZone;
            return TimeZoneInfo.ConvertTime(time, destTimeZone, TimeZoneInfo.Utc);
        }
    }
}