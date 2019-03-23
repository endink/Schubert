using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Web
{
    internal class UserAgent
    {
        public static bool IsWindows(string agentString)
        {
            return (agentString?.IndexOf("windows", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }

        public static bool IsWindowsTablet(string agentString)
        {
            return IsWindows(agentString) && (agentString?.IndexOf("touch", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }

        public static bool IsWindowsPhone(string agentString)
        {
            return IsWindows(agentString) && (agentString?.IndexOf("mobile", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }

        public static bool IsMacOS(string agentString)
        {
            return !IsWindows(agentString)
                && !IsIPhone(agentString)
                && !IsIPad(agentString)
                && (agentString?.IndexOf("Mac OS X", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }

        public static bool IsIPhone(string agentString)
        {
            return !IsWindows(agentString) && (agentString?.IndexOf("iphone", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }

        public static bool IsIPad(string agentString)
        {
            return (agentString?.IndexOf("ipad", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }

        public static bool IsAndroid(string agentString)
        {
            return !IsWindows(agentString) && (agentString?.IndexOf("android", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }

        public static bool IsAndroidPhone(string agentString)
        {
            return IsAndroid(agentString) && (agentString?.IndexOf("mobile", StringComparison.OrdinalIgnoreCase) ?? -1) >= 0;
        }

        public static bool IsAndroidTablet(string agentString)
        {
            return IsAndroid(agentString) && (agentString?.IndexOf("mobile", StringComparison.OrdinalIgnoreCase) ?? -1) == -1;
        }
    }
}
