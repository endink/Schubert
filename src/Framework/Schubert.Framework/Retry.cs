using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Schubert.Framework
{
    public static class Retry
    {
        #region private 

        private static T RunSyncCore<T, TException>(Func<T> func, int retryTimes, int currentTimes, int retryIntervalMilliseconds)
            where TException : Exception
        {
            try
            {
                return func();
            }
            catch (TException)
            {
                if (retryTimes > currentTimes)
                {
                    if (retryIntervalMilliseconds > 0)
                    {
                        Thread.Sleep(retryIntervalMilliseconds);
                    }
                    return RunSyncCore<T, TException>(func, retryTimes, currentTimes + 1, retryIntervalMilliseconds);
                }
                else
                {
                    throw;
                }
            }
        }

        private static void RunSyncCore<TException>(Action execution, int retryTimes, int currentTimes, int retryIntervalMilliseconds)
            where TException : Exception
        {
            try
            {
                execution();
            }
            catch (TException)
            {
                if (retryTimes > currentTimes)
                {
                    if (retryIntervalMilliseconds > 0)
                    {
                        Thread.Sleep(retryIntervalMilliseconds);
                    }
                    RunSyncCore<TException>(execution, retryTimes, currentTimes + 1, retryIntervalMilliseconds);
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task RunCoreAsync<TException>(Func<Task> execution, int retryTimes, int currentTimes, int retryIntervalMilliseconds)
            where TException : Exception
        {
            try
            {
                await execution();
            }
            catch (TException)
            {
                if (retryTimes > currentTimes)
                {
                    if (retryIntervalMilliseconds > 0)
                    {
                        Thread.Sleep(retryIntervalMilliseconds);
                    }
                    await RunCoreAsync<TException>(execution, retryTimes, currentTimes + 1, retryIntervalMilliseconds);
                }
                else
                {
                    throw;
                }
            }
        }

        private static async Task<T> RunCoreAsync<T, TException>(Func<Task<T>> execution, int retryTimes, int currentTimes, int retryIntervalMilliseconds)
           where TException : Exception
        {
            try
            {
                T result = await execution();
                return result;
            }
            catch (TException ex)
            {
                if (retryTimes > currentTimes)
                {
                    if (retryIntervalMilliseconds > 0)
                    {
                        Thread.Sleep(retryIntervalMilliseconds);
                    }
                    return await RunCoreAsync<T, TException>(execution, retryTimes, currentTimes + 1, retryIntervalMilliseconds);
                }
                else
                {
                    throw ex;
                }
            }
        }

        #endregion

        /// <summary>
        /// 对指定的异常执行操作重试。
        /// </summary>
        /// <param name="execution">要执行的操作。</param>
        /// <param name="retryTimes">重试次数。</param>
        /// <param name="retryIntervalMilliseconds">重试间隔时间。</param>
        /// <returns></returns>
        public static void RunRetry<TException>(Action execution, int retryTimes = 5, int retryIntervalMilliseconds = 2000)
            where TException : Exception
        {
            RunSyncCore<TException>(execution, retryTimes, 0, retryIntervalMilliseconds);
        }

        /// <summary>
        /// 对指定的异常执行操作重试。
        /// </summary>
        /// <param name="func">要执行的操作。</param>
        /// <param name="retryTimes">重试次数。</param>
        /// <param name="retryIntervalMilliseconds">重试间隔时间。</param>
        /// <returns></returns>
        public static T RunRetry<T, TException>(Func<T> func, int retryTimes = 5, int retryIntervalMilliseconds = 2000)
            where TException : Exception
        {
            return RunSyncCore<T, TException>(func, retryTimes, 0, retryIntervalMilliseconds);
        }

        /// <summary>
        /// 对指定的异常执行操作重试。
        /// </summary>
        /// <param name="execution">要执行的操作。</param>
        /// <param name="retryTimes">重试次数。</param>
        /// <param name="retryIntervalMilliseconds">重试间隔时间。</param>
        /// <returns></returns>
        public static Task RunRetryAsync<TException>(Func<Task> execution, int retryTimes = 5, int retryIntervalMilliseconds = 2000)
            where TException : Exception
        {
            return RunCoreAsync<TException>(execution, retryTimes, 0, retryIntervalMilliseconds);
        }

        /// <summary>
        /// 对指定的异常执行操作重试。
        /// </summary>
        /// <param name="execution">要执行的操作。</param>
        /// <param name="retryTimes">重试次数。</param>
        /// <param name="retryIntervalMilliseconds">重试间隔时间。</param>
        /// <returns></returns>
        public static Task<T> RunRetryAsync<T, TException>(Func<Task<T>> execution, int retryTimes = 5, int retryIntervalMilliseconds = 2000)
            where TException : Exception
        {
            return RunCoreAsync<T, TException>(execution, retryTimes, 0, retryIntervalMilliseconds);
        }
    }
}
