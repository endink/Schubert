/*
===============================================================================
 The comments is generate by a tool.

 Author:Sharping      CreateTime:2011-01-27 11:17 
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
using System.Linq.Expressions;
using System.Reflection;

namespace System
{
    static partial class ExtensionMethods
    {
        public static string GetMemberName<TItem, TProperty>(this TItem item, Expression<Func<TItem, TProperty>> memberAccessExpression)
        {
            return memberAccessExpression.GetMemberName();
        }

        public static string GetMemberName<T>(this Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null)
            {
                throw new ArgumentNullException("propertyExpression");
            }
            MemberExpression memberExpression = propertyExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("the expression not a MemberAccessExpression", "propertyExpression");
            }
            MemberInfo propertyInfo = memberExpression.Member as MemberInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("the expression not a MemberAccessExpression", "propertyExpression");
            }

            return memberExpression.Member.Name;
        }

        public static string GetMemberName<TItem, TMember>(this Expression<Func<TItem, TMember>> memberAccessExpression)
        {
            MemberExpression memberExpression = memberAccessExpression.Body as MemberExpression;
            if (memberExpression == null)
            {
                UnaryExpression unaryExpression = memberAccessExpression.Body as UnaryExpression;
                if (unaryExpression != null)
                {
                    memberExpression = unaryExpression.Operand as MemberExpression;
                }
            }
            if (memberExpression != null)
            {
                StringBuilder builder = new StringBuilder();
                QueryMemeberExpression(memberExpression, builder);
                if (builder.Length > 0)
                {
                    builder = builder.Remove(0, 1);
                }
                return builder.ToString();
            }
            throw new NotSupportedException("Only MemberExpression can use. Example m=>m.PropertyA");
        }

        private static void QueryMemeberExpression(MemberExpression expression, StringBuilder builder)
        {
            builder.Insert(0, String.Format(".{0}", expression.Member.Name));
            if (expression.Expression.NodeType == ExpressionType.MemberAccess)
            {
                QueryMemeberExpression((MemberExpression)expression.Expression, builder);
            }
        }
    }
}
