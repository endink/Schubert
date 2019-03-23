/*  This source file title is generate by a tool "CodeHeader" that made by Sharping  */
/*
===============================================================================
 Sharping Software Factory Addin
 Author:Sharping      CreateTime:2009-03-10 15:03:43 
===============================================================================
 Copyright ? Sharping.  All rights reserved.
 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
 OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
 LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 FITNESS FOR A PARTICULAR PURPOSE.
===============================================================================
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Schubert.Helpers;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Security;
using System.Runtime.InteropServices;
using Schubert;

namespace System
{
    static partial class ExtensionMethods
    {
        /// <summary>
        /// 自动转换为正则表达式内可直接使用的字符串（自动插入转义字符）。
        /// </summary>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static string EscapeForRegex(this string instance)
        {
            if (instance.IsNullOrWhiteSpace())
            {
                return instance;
            }

            StringBuilder builder = new StringBuilder();
            //Regex.Escape 不会对]、} 处理。
            foreach (char c in instance)
            {
                switch (c)
                {
                    case '\\':
                    case '(':
                    case ')':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                    case '.':
                    case '-':
                    case '*':
                    case '+':
                    case '?':
                    case '|':
                    case '^':
                    case '$':
                        builder.Append('\\');
                        builder.Append(c);
                        break;
                    default:
                        builder.Append(c);
                        break;
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// 大小写敏感比较。
        /// </summary>
        public static bool CaseSensitiveEquals(this string instance, string comparing)
        {
            if (instance == null && comparing == null)
            {
                return true;
            }
            if ((instance != null && comparing == null) || (instance == null && comparing != null))
            {
                return false;
            }
            return String.CompareOrdinal(instance, comparing) == 0;
        }

        /// <summary>
        /// 大小写忽略比较。
        /// </summary>
        public static bool CaseInsensitiveEquals(this string instance, string comparing)
        {
            if (instance == null && comparing == null)
            {
                return true;
            }
            if ((instance != null && comparing == null) || (instance == null && comparing != null))
            {
                return false;
            }
            return instance.Equals(comparing, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 指示指定的 <see cref="System.String"/> 对象是 null 还是 System.String.Empty 字符串。
        /// </summary>
        public static bool IsNullOrEmpty(this String data)
        {
            return String.IsNullOrEmpty(data);
        }

        /// <summary>
        /// 指示指定的字符串是 null、空还是仅由空白字符组成。
        /// </summary>
        public static bool IsNullOrWhiteSpace(this String data)
        {
               return String.IsNullOrWhiteSpace(data);
        }

        /// <summary>
        /// 	如果指定的 <see cref="System.String"/> 对象字符串是 null、空还是仅由空白字符组成则返回默认值。
        /// </summary>
        public static string IfNullOrWhiteSpace(this string value, string defaultValue)
        {
            return (!value.IsNullOrWhiteSpace() ? value : defaultValue);
        }

        /// <summary>
        /// 	如果指定的 <see cref="System.String"/> 对象是 null 或 System.Empty 字符串则返回默认值。
        /// </summary>
        public static string IfNullOrEmpty(this string value, string defaultValue)
        {
            return (!value.IsNullOrEmpty() ? value : defaultValue);
        }


        /// <summary>
        /// 获取一个值，指示当前字符串是否是 IPV4地址 ( XXX.XXX.XXX.XXX )。
        /// </summary>
        /// <param name="input"></param>
        public static bool IsIPV4Address(this string input)
        {
            return !input.IsNullOrWhiteSpace() && Regex.IsMatch(input, "^(((2[0-4]\\d)|(25[0-5]))|(1\\d{2})|([1-9]\\d)|(\\d))[.](((2[0-4]\\d)|(25[0-5]))|(1\\d{2})|([1-9]\\d)|(\\d))[.](((2[0-4]\\d)|(25[0-5]))|(1\\d{2})|([1-9]\\d)|(\\d))[.](((2[0-4]\\d)|(25[0-5]))|(1\\d{2})|([1-9]\\d)|(\\d))$");
        }

        public static SecureString ToSecurityString(this String input)
        {
            char[] pChar = input.ToCharArray();

            SecureString ss = new SecureString();

            foreach (char c in pChar)
            {
                ss.AppendChar(c);
            }
            return ss;
        }

        public static String TrimEnd(this String source, String str)
        {
            Guard.ArgumentNotNull(source, nameof(source));
            if (str.IsNullOrEmpty() || str.Length > source.Length)
            {
                return str;
            }
            if (source.Substring(source.Length - str.Length).Equals(str))
            {
                return source.Substring(0, source.Length - str.Length - 1);
            }
            return source;
        }

        public static String TrimStart(this String source, String str)
        {
            Guard.ArgumentNotNull(source, nameof(source));
            if (str.IsNullOrEmpty() || str.Length > source.Length)
            {
                return str;
            }
            if (source.Substring(0, str.Length).Equals(str))
            {
                return source.Substring(str.Length);
            }
            return source;
        }

        #region Serialize

        public static T XmlDeserialize<T>(this string str)
        {
            return ToolHelper.XmlDeserialize<T>(str);
        }

        #endregion
    }
}
