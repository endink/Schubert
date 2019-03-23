/*
===============================================================================
 The comments is generate by a tool.

 Author:Sharping      CreateTime:2011-01-27 11:21 
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
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace System
{
    static partial class ExtensionMethods
    {
        public static Exception GetOriginalException(this Exception exception)
        {
            if (exception.InnerException == null) return exception;

            return exception.InnerException.GetOriginalException();
        }

        public static TException GetOriginalException<TException>(this Exception exception)
            where TException : Exception
        {
            TException ex = exception as TException;
            if (exception == null)
            {
                return null;
            }
            else if (ex != null)
            {
                return ex;
            }
            else
            {
                return exception.InnerException.GetOriginalException<TException>();
            }
        }

        public static IEnumerable<string> Messages(this Exception exception)
        {
            return exception != null ?
                    new List<string>(exception.InnerException.Messages()) { exception.Message } : Enumerable.Empty<string>();
        }

        public static IEnumerable<Exception> Exceptions(this Exception exception)
        {
            return exception != null ?
                    new List<Exception>(exception.InnerException.Exceptions()) { exception } : Enumerable.Empty<Exception>();
        }

        /// <summary>
        /// 抛出无法处理的异常，例如堆栈溢出、算术溢出等）。
        /// </summary>
        /// <param name="exception"></param>
        /// <returns></returns>
        public static void ThrowIfNecessary(this Exception exception)
        {
            if (exception is OutOfMemoryException || exception is OverflowException || exception is InvalidCastException)
            {
                throw new Exception(exception.StackTrace, exception);
            }
        }
    }
}
