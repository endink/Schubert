using Schubert;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Schubert.Framework.Scheduling
{
    /// <summary>
    /// 常用 Cron 表达式（参考：http://wenku.baidu.com/view/d67a3835eefdc8d376ee325a.html）
    /// 格式：Seconds Minutes Hours DayofMonth Month DayofWeek Year 
    /// </summary>
    public static class CronExpressions
    {
        /// <summary>
        /// 获取以分钟为单位的时间间隔表达式（如 <paramref name="minutes"/> 为 1 表示每分钟）。
        /// </summary>
        /// <param name="minutes">要间隔的分钟数。</param>
        /// <param name="atMinutes">从当前时间的第几分钟开始，例如为 5 ，如果当前时间为 6 点，表示 6 点过5分开始执行，如果当前时间为 6 点 10 分，将从 7 点 5 分开始执行。</param>
        /// <returns></returns>
        public static string IntervalMinute(int minutes, int atMinutes = 0)
        {
            Guard.InMinuteRange(atMinutes, nameof(atMinutes));
            minutes = Math.Max(1, minutes);
            return $"0 {atMinutes}/{minutes} * * *";
        }

        /// <summary>
        /// 获取表示在每天的 XX点XX分执行任务。
        /// </summary>
        /// <param name="hours">24小时制的小时数。</param>
        /// <param name="minutes">24小时制的小时数。</param>
        /// <returns></returns>
        public static string Daily(int hours, int minutes)
        {
            Guard.InHourRange(hours, nameof(hours));
            Guard.InMinuteRange(minutes, nameof(minutes));
            return $"0 {minutes} {hours} * * ?";
        }

        /// <summary>
        /// 获取表示在周几的 XX点XX分执行任务。
        /// </summary>
        /// <param name="week">表示要执行任务的周。</param>
        /// <param name="hours">24小时制的小时数。</param>
        /// <param name="minutes">24小时制的小时数。</param>
        /// <returns></returns>
        public static string Weekly(DayOfWeek week, int hours, int minutes)
        {
            Guard.InHourRange(hours, nameof(hours));
            Guard.InMinuteRange(minutes, nameof(minutes));
            return $"0 {minutes} {hours} ? * {((int)week) + 1}";
        }

        /// <summary>
        /// 获取表示在每月的指定日期和时间执行任务。
        /// </summary>
        /// <param name="daysOfMonth">要执行任务的号数（例如1号和30号则可以传入 int[]{1, 30}）。</param>
        /// <param name="hours">24小时制的小时数。</param>
        /// <param name="minutes">24小时制的小时数。</param>
        /// <returns></returns>
        public static string Weekly(IEnumerable<int> daysOfMonth, int hours, int minutes)
        {
            foreach (var d in daysOfMonth)
            {
                Guard.InMonthDayRange(d, nameof(daysOfMonth));
            }
            Guard.InHourRange(hours, nameof(hours));
            Guard.InMinuteRange(minutes, nameof(minutes));
            return $"0 {minutes} {hours} {daysOfMonth.ToArrayString(",")} * ?";
        }


    }
}
