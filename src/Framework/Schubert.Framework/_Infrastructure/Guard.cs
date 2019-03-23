/*  This source file title is generate by a tool "CodeHeader" that made by Sharping  */
/*
===============================================================================
 Sharping Software Factory Addin
 Author:Sharping      CreateTime:2008-06-13 23:55:56 
===============================================================================
 Copyright ? Sharping.  All rights reserved.
 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
 OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
 LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 FITNESS FOR A PARTICULAR PURPOSE.
===============================================================================
*/

using Schubert.Helpers;
using System;
using System.IO;
using System.Reflection;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace Schubert
{
    public static class Guard
    {
        /// <summary>
        /// 判断（路径）参数中是否包含非法字符。
        /// </summary>
        /// <param name="argument"></param>
        /// <param name="argumentName"></param>
        [System.Diagnostics.DebuggerHidden]
        public static void ArgumentContainsInvalidPathChars(string argument, string argumentName)
        {
            if (argument.IsNullOrWhiteSpace())
            {
                return;
            }
            var invliadChars = Path.GetInvalidPathChars();

            if (argument.Any(c => invliadChars.Contains(c)))
            {
                throw new ArgumentException($"@The provided String argument {argumentName} contains invalid path character.");
            }
        }

        /// <summary>
        /// 当条件不满足时抛出异常。
        /// </summary>
        /// <param name="condition">要测试的条件。</param>
        /// <param name="paramName">参数名称。</param>
        /// <param name="message">异常消息。</param>
        [System.Diagnostics.DebuggerHidden]
        public static void ArgumentCondition(bool condition, string message, string paramName = null)
        {
            if (!condition)
            {
                var ex = paramName.IsNullOrWhiteSpace() ? new ArgumentException(message) : new ArgumentException(message, paramName);
                throw ex;
            }
        }
        

        [System.Diagnostics.DebuggerHidden]
        public static void ArgumentIsUri(string argument, string argumentName, UriKind kind = UriKind.RelativeOrAbsolute)
        {
            if (!argument.IsNullOrWhiteSpace())
            {
                if (Uri.IsWellFormedUriString(Uri.EscapeUriString(argument), kind))
                {
                    return;
                }
            }
            throw new ArgumentException(String.Format(@"The provided string argument {0} must  be uri.", argumentName), argumentName);
        }
        [System.Diagnostics.DebuggerHidden]
        public static void AbsolutePhysicalPath(string argument, string argumentName)
        {
            if (argument.IsNullOrWhiteSpace())
            {
                throw new ArgumentException(String.Format(@"The provided string argument {0} must  be absolute physical path.", argumentName), argumentName);
            }
            if (!Path.IsPathRooted(argument))
            {
                throw new ArgumentException(String.Format(@"The provided string argument {0} must  be absolute physical path.", argumentName), argumentName);
            }
        }

        /// <summary>
        /// 当参数不是相对路径（包括文件系统路径和 Uri）是抛出异常。
        /// </summary>
        /// <param name="argument">参数。</param>
        /// <param name="argumentName">参数名。</param>
        [System.Diagnostics.DebuggerHidden]
        public static void ArgumentIsRelativePath(string argument, string argumentName)
        {
            if (argument.IsNullOrWhiteSpace())
            {
                throw new ArgumentException(String.Format(@"The provided string argument {0} must  be relative path.", argumentName), argumentName);
            }
            Guard.ArgumentContainsInvalidPathChars(argument, argumentName);
            var virtualPath = argumentName.Replace(@"\", @"/");
            if (Uri.IsWellFormedUriString(Uri.EscapeUriString(virtualPath), UriKind.Absolute))
            {
                throw new ArgumentException(String.Format(@"The provided string argument {0} must  be relative path.", argumentName), argumentName);
            }
            var path = argument.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            if (Path.IsPathRooted(path))
            {
                throw new ArgumentException(String.Format(@"The provided string argument {0} must  be relative path.", argumentName), argumentName);
            }
        }
        [System.Diagnostics.DebuggerHidden]
        public static void ArgumentNullOrWhiteSpaceString(string argumentValue, string argumentName)
        {
            Guard.ArgumentNotNullOrEmptyString(argumentValue, argumentName, true);
        }
        [System.Diagnostics.DebuggerHidden]
        public static void ArgumentNotNullOrEmptyString(string argumentValue, string argumentName)
        {
            Guard.ArgumentNotNullOrEmptyString(argumentValue, argumentName, false);
        }

        private static void ArgumentNotNullOrEmptyString(string argumentValue, string argumentName, bool trimString)
        {
            if ((trimString && argumentValue.IsNullOrWhiteSpace()) || (!trimString && argumentValue.IsNullOrEmpty()))
            {
                throw new ArgumentException(String.Format(@"The provided String argument {0} must not be empty.", argumentName));
            }
        }
        [System.Diagnostics.DebuggerHidden]
        public static void ArgumentNotNullOrEmptyArray<T>(IEnumerable<T> argumentValue, string argumentName)
        {
            if (argumentValue == null || !argumentValue.Any())
            {
                throw new ArgumentException(String.Format(@"The provided array argument {0} must not be null or empty array.", argumentName));
            }
        }

        /// <summary>
        /// Checks an argument to ensure it isn't null
        /// </summary>
        /// <param name="argumentValue">The argument value to check.</param>
        /// <param name="argumentName">The name of the argument.</param>
        [System.Diagnostics.DebuggerHidden]
        public static void ArgumentNotNull(object argumentValue, string argumentName)
        {
            if (argumentValue == null)
                throw new ArgumentNullException(argumentName);
        }

        /// <summary>
        /// Checks an Enum argument to ensure that its value is defined by the specified Enum type.
        /// </summary>
        /// <param name="enumType">The Enum type the value should correspond to.</param>
        /// <param name="value">The value to check for.</param>
        /// <param name="argumentName">The name of the argument holding the value.</param>
        [System.Diagnostics.DebuggerHidden]
        public static void EnumValueIsDefined(Type enumType, object value, string argumentName)
        {
            if (!enumType.HasAttribute<FlagsAttribute>())
            {
                if (Enum.IsDefined(enumType, value) == false)
                    throw new ArgumentException(String.Format("The value of the argument {0} provided for the enumeration {1} is invalid.",
                        argumentName, enumType.ToString()));
            }
            else
            {
                try
                {
                    ExpressionHelper.MakeConvertLambda(enumType).Compile().DynamicInvoke(value);
                }
                catch (TargetInvocationException)
                {
                    throw new ArgumentException(String.Format("The value of the argument {0} provided for the enumeration {1} is invalid.",
                        argumentName, enumType.ToString()));
                }
            }
        }

        /// <summary>
        ///判断类型是否能够从提供的类型分配实例（assignee 是否继承于 providedType）。
        /// </summary>
        /// <param name="assignee">参数类型。</param>
        /// <param name="providedType">要从中分配实例的类型，通常为接口或基类。</param>
        /// <param name="argumentName">参数名称。</param>
        [System.Diagnostics.DebuggerHidden]
        public static void TypeIsAssignableFromType(Type assignee, Type providedType, string argumentName)
        {
            if (!providedType.GetTypeInfo().IsAssignableFrom(assignee))
                throw new ArgumentException(String.Format("The provided type {0} is not compatible with {1}.", assignee, providedType), argumentName);
        }
        [System.Diagnostics.DebuggerHidden]
        public static void DateTimeKind(DateTimeKind kind, DateTime value, string argumentName)
        {
            if (value.Kind != kind)
            {
                throw new ArgumentException(String.Format("The datetime kind of the argument '{0}' must be {1}.", argumentName, kind.ToString()));
            }
        }
        [System.Diagnostics.DebuggerHidden]
        public static void DateTimeKind(DateTimeKind kind, DateTime? value, string argumentName)
        {
            if (value.HasValue && value.Value.Kind != kind)
            {
                throw new ArgumentException(String.Format("The datetime kind of the argument '{0}' must be {1}.", argumentName, kind.ToString()));
            }
        }
        [System.Diagnostics.DebuggerHidden]
        public static void InSecondRange(int data, string argumentName)
        {
            if (data > 59 || data < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName, "second must between 0 an 59");
            }
        }
        [System.Diagnostics.DebuggerHidden]
        public static void InMonthRange(int data, string argumentName)
        {
            if (data > 12 || data < 1)
            {
                throw new ArgumentOutOfRangeException(argumentName, "month must between 1 an 59");
            }
        }
        [System.Diagnostics.DebuggerHidden]
        public static void InMinuteRange(int data, string argumentName)
        {
            if (data > 59 || data < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName, "minute must between 0 an 59");
            }
        }
        [System.Diagnostics.DebuggerHidden]
        public static void InHourRange(int data, string argumentName)
        {
            if (data > 23 || data < 0)
            {
                throw new ArgumentOutOfRangeException(argumentName, "minute must between 0 an 23");
            }
        }
        [System.Diagnostics.DebuggerHidden]
        public static void InMonthDayRange(int data, string argumentName)
        {
            if (data > 31 || data < 1)
            {
                throw new ArgumentOutOfRangeException(argumentName, "month day must between 1 an 31");
            }
        }
    }
}
