/*  This source file title is generate by a tool "CodeHeader" that made by Sharping  */
/*
===============================================================================
 Sharping Software Factory Addin
 Author:Sharping      CreateTime:2009-05-05 14:30:08 
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
using System.Collections;
using Schubert;
using Schubert.Helpers;
using System.Linq.Expressions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;

namespace System
{
    static partial class ExtensionMethods
    {
        
        #region TableString

        /// <summary>
        /// 生成 ascii 表格。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="values">要生成表格的元素。</param>
        /// <param name="columnHeaders">列名。</param>
        /// <param name="valueSelectors">每一行数据取出的列数据。</param>
        /// <returns></returns>
        public static string ToStringTable<T>(this IEnumerable<T> values, string[] columnHeaders, params Func<T, object>[] valueSelectors)
        {
            return ToStringTable(values.ToArray(), columnHeaders, valueSelectors);
        }

        private static string ToStringTable<T>(this T[] values, string[] columnHeaders, params Func<T, object>[] valueSelectors)
        {
            Debug.Assert(columnHeaders.Length == valueSelectors.Length);

            var arrValues = new string[values.Length + 1, valueSelectors.Length];

            // Fill headers
            for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                arrValues[0, colIndex] = columnHeaders[colIndex];
            }

            // Fill table rows
            for (int rowIndex = 1; rowIndex < arrValues.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
                {
                    object value = valueSelectors[colIndex].Invoke(values[rowIndex - 1]);

                    arrValues[rowIndex, colIndex] = value != null ? value.ToString() : "null";
                }
            }

            return ToStringTable(arrValues);
        }

        public static string ToStringTable(this string[,] arrValues)
        {
            int[] maxColumnsWidth = GetMaxColumnsWidth(arrValues);
            int[] maxRowHeight = GetMaxRowHeight(arrValues);
            var headerSpliter = new string('-', maxColumnsWidth.Sum(i => i + 3) - 1);

            var sb = new StringBuilder();
            for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
            {
                WriteRow(arrValues, maxColumnsWidth, headerSpliter, sb, rowIndex, maxRowHeight[rowIndex]);
            }

            return sb.ToString();
        }

        private static void WriteRow(string[,] arrValues, int[] maxColumnsWidth, string headerSpliter, StringBuilder sb, int rowIndex, int rowHeight)
        {
            for (int i = 0; i < rowHeight; i++)
            {
                for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
                {
                    // Print cell
                    string cell = arrValues[rowIndex, colIndex];
                    string[] splitedValues =  cell.Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    cell = (splitedValues.Length > i) ? splitedValues[i] : " ";
                    cell = cell.PadRight(maxColumnsWidth[colIndex]);
                    sb.Append(" | ");
                    sb.Append(cell);
                }

                // Print end of line
                sb.Append(" | ");
                sb.AppendLine();

                // Print splitter
                if (rowIndex == 0)
                {
                    sb.AppendFormat(" |{0}| ", headerSpliter);
                    sb.AppendLine();
                }
            }
        }

        private static int[] GetMaxRowHeight(string[,] arrValues)
        {
            var maxRowHeight = new int[arrValues.GetLength(0)];
            for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
            {
                for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
                {
                    int newLenght = arrValues[rowIndex, colIndex].Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).Length;
                    maxRowHeight[rowIndex] = Math.Max(maxRowHeight[rowIndex], newLenght);
                } 
            }
            return maxRowHeight;
        }

        private static int[] GetMaxColumnsWidth(string[,] arrValues)
        {
            var maxColumnsWidth = new int[arrValues.GetLength(1)];
            for (int colIndex = 0; colIndex < arrValues.GetLength(1); colIndex++)
            {
                for (int rowIndex = 0; rowIndex < arrValues.GetLength(0); rowIndex++)
                {
                    var values = arrValues[rowIndex, colIndex].Split(new String[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                    int newLength = values.Length == 0 ? 0: values.Max(s=>s?.Length ?? 0);
                    int oldLength = maxColumnsWidth[colIndex];

                    maxColumnsWidth[colIndex] = Math.Max(newLength, oldLength);
                }
            }

            return maxColumnsWidth;
        }

        public static string ToStringTable<T>(this IEnumerable<T> values, params Expression<Func<T, object>>[] valueSelectors)
        {
            var headers = valueSelectors.Select(func => GetProperty(func)?.Name ?? String.Empty).ToArray();
            var selectors = valueSelectors.Select(exp => exp.Compile()).ToArray();
            return ToStringTable(values, headers, selectors);
        }

        private static PropertyInfo GetProperty<T>(Expression<Func<T, object>> expresstion)
        {
            if (expresstion.Body is UnaryExpression)
            {
                if ((expresstion.Body as UnaryExpression).Operand is MemberExpression)
                {
                    return ((expresstion.Body as UnaryExpression).Operand as MemberExpression).Member as PropertyInfo;
                }
            }

            if ((expresstion.Body is MemberExpression))
            {
                return (expresstion.Body as MemberExpression).Member as PropertyInfo;
            }
            return null;
        }
        
        #endregion

        public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ObservableCollection<T>(enumerable);
        }

        public static ReadOnlyCollection<T> ToReadOnlyCollection<T>(this IEnumerable<T> enumerable)
        {
            return new ReadOnlyCollection<T>(enumerable.ToList());
        }

        public static IEnumerable<TEntity> Pagination<TEntity>(this IEnumerable<TEntity> entities, int pageIndex, int pageSize)
        {
            var skipQuery = pageIndex > 0 ? entities.Skip(pageIndex * pageSize) : entities;
            var entries = skipQuery.Take(pageSize);
            return entries;
        }

        public static IQueryable<TEntity> Pagination<TEntity>(this IQueryable<TEntity> entities, int pageIndex, int pageSize)
            where TEntity : class
        {
            var skipQuery = pageIndex > 0 ? entities.Skip(pageIndex * pageSize) : entities;
            var entries = skipQuery.Take(pageSize);
            return entries;
        }

        public static void For<T>(this IEnumerable<T> enums, Action<int, T> action)
        {
            enums.For((index, item) => { action(index, item); return true; });
        }

        public static void ForEach<T>(this IEnumerable<T> enums, Action<T> action)
        {
            enums.ForEach(item => { action(item); return true; });
        }

        public static void For<T>(this IEnumerable<T> enums, Func<int, T, bool> action)
        {
            if (enums == null || action == null)
            {
                return;
            }
            int i = 0;
            var array = enums.ToArray();
            foreach (T item in enums)
            {
                if (!action(i, item))
                {
                    break;
                }
                i++;
            }
        }

        public static void ForEach<T>(this IEnumerable<T> enums, Func<T, bool> action)
        {
            if (enums == null || action == null)
            {
                return;
            }
            var array = enums.ToArray();
            foreach (T item in array)
            {
                if (!action(item))
                {
                    break;
                }
            }
        }

        public static bool IsNullOrEmpty(this IEnumerable source)
        {
            return source == null || !source.GetEnumerator().MoveNext();
        }

        public static IEnumerable<TSource> Distinct<TSource, TProperty>(this IEnumerable<TSource> source, Expression<Func<TSource, TProperty>> comparerProperty)
        {
            Expression<Func<TSource, TProperty>> expression = comparerProperty;
            return source.Distinct(new PropertyComparer<TSource>(comparerProperty.GetMemberName()));
        }

        public static IEnumerable<TSource> Intersect<TSource, TProperty>(this IEnumerable<TSource> source, IEnumerable<TSource> second, Expression<Func<TSource, TProperty>> comparerProperty)
        {
            return source.Intersect(second, new PropertyComparer<TSource>(comparerProperty.GetMemberName()));
        }

        public static IEnumerable<TSource> Except<TSource, TProperty>(this IEnumerable<TSource> source, IEnumerable<TSource> second, Expression<Func<TSource, TProperty>> comparerProperty)
        {
            return source.Except(second, new PropertyComparer<TSource>(comparerProperty.GetMemberName()));
        }

        public static string ToArrayString<T>(this IEnumerable<T> array, string separator = ",")
        {
            return String.Join(separator, array.Select(t => t.ToString()));
        }

        public static string ToArrayString<T>(this IEnumerable<T> array, Func<T, String> reduce, string separator = ",")
        {
            Guard.ArgumentNotNull(reduce, nameof(reduce));
            return String.Join(separator, array.Select(reduce));
        }
    }
}
