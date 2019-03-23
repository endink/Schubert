/*  This source file title is generate by a tool "CodeHeader" that made by Sharping  */
/*
===============================================================================
 Sharping Software Factory Addin
 Author:Sharping      CreateTime:2009-03-10 14:57:07 
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
using System.Reflection;
using Schubert;

namespace System
{
    static partial class ExtensionMethods
    {
        public static bool CanUseForDb(this Type type)
        {
            return type == typeof(string) ||
                 type == typeof(int) ||
                 type == typeof(long) ||
                 type == typeof(uint) ||
                 type == typeof(ulong) ||
                 type == typeof(float) ||
                 type == typeof(double) ||
                 type == typeof(Guid) ||
                 type == typeof(byte[]) ||
                 type == typeof(decimal) ||
                 type == typeof(char) ||
                 type == typeof(bool) ||
                 type == typeof(byte) ||
                 type == typeof(DateTime) ||
                 type == typeof(TimeSpan) ||
                 type == typeof(DateTimeOffset)||
                 type.GetTypeInfo().IsEnum ||
                 (Nullable.GetUnderlyingType(type) != null && CanUseForDb(Nullable.GetUnderlyingType(type)));
        }
        public static bool IsNullableType(this Type type, Type genericParameterType)
        {
            Guard.ArgumentNotNull(genericParameterType, nameof(genericParameterType));

            genericParameterType.Equals(Nullable.GetUnderlyingType(type));
            return false;
        }

        public static bool IsNullableEnum(this Type type)
        {
            return (Nullable.GetUnderlyingType(type)?.GetTypeInfo().IsEnum ?? false);
        }

        public static bool HasAttribute<T>(this Type provider, bool inherit = false) where T : Attribute
        {
            return provider.GetTypeInfo().IsDefined(typeof(T), inherit);
        }

        public static IEnumerable<T> GetAttributes<T>(this Type provider, bool inherit = false) where T : Attribute
        {
            return provider.GetTypeInfo().GetCustomAttributes<T>(inherit);
        }

        public static T GetAttribute<T>(this Type provider, bool inherit = false) where T : Attribute
        {
            return provider.GetTypeInfo().GetCustomAttributes<T>(inherit)?.FirstOrDefault();
        }
    }
}
