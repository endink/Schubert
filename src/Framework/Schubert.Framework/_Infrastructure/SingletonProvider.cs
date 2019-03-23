/*  This source file title is generate by a tool "CodeHeader" that made by Sharping  */
/*
===============================================================================
 Sharping Software Factory Addin
 Author:Sharping      CreateTime:2009-01-06 10:25:50 
===============================================================================
 Copyright ? Sharping.  All rights reserved.
 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
 OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
 LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 FITNESS FOR A PARTICULAR PURPOSE.
===============================================================================
*/

using System;
using System.Linq;
using System.Reflection;

namespace Schubert
{

    public sealed class SingletonProvider<T> where T : class
    {
        private static T m_instance = null;
        
        private SingletonProvider() { }

        public static T Instance
        {
            get
            {
                if (m_instance == null)
                {
                    lock (typeof(T))
                    {
                        if (m_instance == null)
                        {
                            m_instance = CreateInstance();
                        }
                    }
                }
                return m_instance;
            }
        }

        private static T CreateInstance()
        {
            Type type = typeof(T);
            var constructor = type.GetTypeInfo().GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).FirstOrDefault();

            return (T)constructor.Invoke(null);
        }
    }
}
