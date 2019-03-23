/*  This source file title is generate by a tool "CodeHeader" that made by Sharping  */
/*
===============================================================================
 Sharping Software Factory Addin
 Author:Sharping      CreateTime:2009-03-10 15:00:17 
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
using Schubert;
using System.Collections.Concurrent;

namespace System
{
    static partial class ExtensionMethods
    {
        /// <summary>
        /// 获取日期的最小时间表示形式(00:00:00)
        /// </summary>
        public static DateTimeOffset ToMinTimeDate(this DateTimeOffset date)
        {
            DateTimeOffset result = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, 0, date.Offset);
            return result;
        }

        /// <summary>
        /// 获取日期的最小时间表示形式(00:00:00)
        /// </summary>
        public static DateTime ToMinTimeDate(this DateTime date)
        {
            DateTime result = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0, date.Kind);
            return result;
        }

        /// <summary>
        /// 获取日期的最大时间表示形式(23:59:59)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTime ToMaxTimeDate(this DateTime date)
        {
            DateTime result = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999, date.Kind);
           
            return result;
        }

        /// <summary>
        /// 返回自 1970 年 1 月 1 日 00:00:00 GMT 以来此 <see cref="DateTime"/> 对象表示的毫秒数。 
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static long GetUnixTime(this DateTime date)
        {
            TimeSpan diff = date.ToUniversalTime() - Schubert.Framework.SnowflakeWorker.Jan1st1970;
            return (long)Math.Floor(diff.TotalMilliseconds);
        }

        /// <summary>
        /// 获取日期的最大时间表示形式(23:59:59)
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public static DateTimeOffset ToMaxTimeDate(this DateTimeOffset date)
        {
            DateTimeOffset result = new DateTimeOffset(date.Year, date.Month, date.Day, 23, 59, 59, 999, date.Offset);

            return result;
        }

        /// <summary>
        /// 获取当前时间所在周的日期范围。
        /// </summary>
        public static RangeOfDateTime CurrentWeek(this DateTime dateTime)
        {
            DateTime start = dateTime.AddDays(1 - int.Parse(dateTime.DayOfWeek.ToString("d"))).ToMinTimeDate();
            DateTime end = start.AddDays(6).ToMaxTimeDate();
            return new RangeOfDateTime(start, end);
        }

        /// <summary>
        /// 获取当前时间所在月的日期范围。
        /// </summary>
        public static RangeOfDateTime CurrentMonth(this DateTime dateTime)
        {
            DateTime start = dateTime.AddDays(1 - dateTime.Day).ToMinTimeDate();
            DateTime end = start.AddMonths(1).AddDays(-1).ToMaxTimeDate();
            return new RangeOfDateTime(start, end);
        }

        /// <summary>
        /// 获取当前时间所在季度的日期范围。
        /// </summary>
        public static RangeOfDateTime CurrentQuarter(this DateTime dateTime)
        {
            DateTime start = dateTime.AddMonths(0 - (dateTime.Month - 1) % 3).AddDays(1 - dateTime.Day).ToMinTimeDate();
            DateTime end = start.AddMonths(3).AddDays(-1).ToMaxTimeDate();
            return new RangeOfDateTime(start, end);
        }

        /// <summary>
        /// 获取当前日期处于一年之中的哪个季度。
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static int QuarterOfYear(this DateTime dateTime)
        {
            return dateTime.Month / 4 + 1;
        }

        public static DateTime ChangeKind(this DateTime date, DateTimeKind kind)
        {
            DateTime result = new DateTime(date.Ticks, kind);
            return result;
        }

        public static DateTime? ChangeKind(this DateTime? date, DateTimeKind kind)
        {
            return date.HasValue ? new  Nullable<DateTime>(date.Value.ChangeKind(kind)) : null;
        }

        public static DateTime? ToUniversalTime(this DateTime? date)
        {
            return date.HasValue ? new Nullable<DateTime>(date.Value.ToUniversalTime()) : null;
        }

        public static DateTime WithoutMillis(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Kind);
        }

        /// <summary>
        /// 四舍五入到指定的小数位。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals">要保留的精度（小数位数）</param>
        /// <returns></returns>
        public static decimal Precision(this decimal value, int decimals)
        {
            if (value < 0)
            {
                return Math.Round(value + 5 / (decimal)Math.Pow(10, decimals + 1), decimals, MidpointRounding.AwayFromZero);
            }
            else
            {
                return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
            }
        }

        /// <summary>
        /// 四舍五入到指定的小数位。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals">要保留的精度（小数位数）</param>
        /// <returns></returns>
        public static double Precision(this double value, int decimals)
        {
            if (value < 0)
            {
                return Math.Round(value + 5 / Math.Pow(10, decimals + 1), decimals, MidpointRounding.AwayFromZero);
            }
            else
            {
                return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
            }
        }

        /// <summary>
        /// 四舍五入到指定的小数位。
        /// </summary>
        /// <param name="value"></param>
        /// <param name="decimals">要保留的精度（小数位数）</param>
        /// <returns></returns>
        public static double Precision(this float value, int decimals)
        {
            if (value < 0)
            {
                return Math.Round(value + 5 / Math.Pow(10, decimals + 1), decimals, MidpointRounding.AwayFromZero);
            }
            else
            {
                return Math.Round(value, decimals, MidpointRounding.AwayFromZero);
            }
        }

        /// <summary>
        /// 判断是否为基数
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static bool IsOdd(this int n)
        {
            return (n % 2) != 0;
        }

        /// <summary>
        /// 判断是否为偶数
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public static bool IsEven(this int n)
        {
            return !n.IsOdd();
        }

        private static readonly TimeZoneInfo BeiJingTimeZone = TimeZoneHelper.GetTimeZoneInfo("Asia/Shanghai");

        /// <summary>
        /// 将北京时间转换为 UTC 时间。
        /// </summary>
        /// <param name="time">要转换的北京时间。</param>
        /// <returns></returns>
        public static DateTime ToUtcFromBeiJingTime(this DateTime time)
        { 
            return TimeZoneInfo.ConvertTime(time, BeiJingTimeZone, TimeZoneInfo.Utc);
        }

        /// <summary>
        /// 将UTC时间转换为北京时间。
        /// </summary>
        /// <param name="time"></param>
        /// <returns></returns>
        public static DateTime ToBeiJingTimeFromUtc(this DateTime time)
        {
            return TimeZoneInfo.ConvertTime(time, TimeZoneInfo.Utc, BeiJingTimeZone);
        }

        private static readonly ConcurrentDictionary<Enum, HashSet<Enum>> EnumFlagCache = new ConcurrentDictionary<Enum, HashSet<Enum>>();

        /// <summary>
        /// 获得枚举的位值（主要用于位枚举）。
        /// </summary>
        /// <param name="enumValue"></param>
        /// <returns></returns>
        public static IEnumerable<Enum> GetEnumBitValues(this Enum enumValue)
        {
            Type enumType = enumValue.GetType();
            bool isFlagEnum = enumType.HasAttribute<FlagsAttribute>();
            var enumValues = EnumFlagCache.GetOrAdd(enumValue, v =>
            {
                HashSet<Enum> values = new HashSet<Enum>();
                if (isFlagEnum)
                {
                    foreach (var d in Enum.GetValues(enumType).Cast<Enum>())
                    {
                        if (Convert.ToInt32(d) == 0 && enumValue.Equals(d))
                        {
                            values.Add(d);
                            break;
                        }
                        if (v.HasFlag(d) && Convert.ToInt32(d) != 0)
                        {
                            values.Add(d);
                        }
                    }
                }
                else
                {
                    values.Add(v);
                }
                return values;
            });
            return enumValues;
        }
    }
}
