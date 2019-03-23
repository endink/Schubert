/*  This source file title is generate by a tool "CodeHeader" that made by Sharping  */
/*
===============================================================================
 Sharping Software Factory Addin
 Author:Sharping      CreateTime:2009-03-13 23:51:54 
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
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Schubert
{
    public sealed class ProfilePropertyCollection : INotifyPropertyChanged
    {
        private Dictionary<String, Object> m_innerDict = null;
        public event PropertyChangedEventHandler PropertyChanged;

        public ProfilePropertyCollection() : base()
        {
            this.m_innerDict = new Dictionary<string, object>();
        }

        public ProfilePropertyCollection(ProfilePropertyCollection collection) : base()
        {
            foreach (string name in collection.Names)
            {
                this.BaseSet(name, collection[name]);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <remarks>
        /// Only string, byte[] or null object is supported.
        /// </remarks>
        /// <returns></returns>
        public object this[string name]
        {
            get 
            {
                return BaseGet(name);
            }
            set
            {
                if (value != null && !(value is String) && !(value is byte[]))
                {
                    throw new NotSupportedException("Only string, byte[] or null object is supported.");
                }
                if (this.ContainKey(name))
                {
                    object oldValue = this.BaseGet(name);
                    if (!value.SafeEquals(oldValue))
                    {
                        this.BaseSet(name, value);
                        this.OnPropertyChanged(name);
                    }
                }
                else
                {
                    this.Add(name, value);
                    this.OnPropertyChanged(name);
                }
            }
        }

        private void BaseSet(string name, object value)
        {
            object oldValue = this.BaseGet(name);
            if (!value.SafeEquals(oldValue))
            {
                this.m_innerDict.Set(name, value);
                this.OnPropertyChanged(name);
            }
        }

        private object BaseGet(string name)
        {
            object value = null;
            this.m_innerDict.TryGetValue(name, out value);
            return value;
        }

        public string GetString(string name)
        {
            return this.BaseGet(name) as String;
        }

        public byte[] GetBytes(string name)
        {
            return this.BaseGet(name) as byte[];
        }

        public int Count { get { return this.m_innerDict.Count; } }

        public void Clear()
        {
            if (this.Count > 0)
            {
                this.m_innerDict.Clear();
                this.OnPropertyChanged(String.Empty);
            }
        }

        public IEnumerable<String> Names
        {
            get { return this.m_innerDict.Keys; }
        }

        public IEnumerable<Object> Values
        {
            get { return this.m_innerDict.Values; }
        }

        internal void Add(string name, object value)
        {
            if (!this.ContainKey(name))
            {
                this.m_innerDict.Add(name, value);
                this.OnPropertyChanged(name);
            }
            else
            {
                new InvalidOperationException(String.Format(@"The value named ""{0}"" was exist.", name));
            }
        }

        public void Add(string name, string stringValue)
        {
            this.Add(name, (object)stringValue);
        }

        public void Add(string name, byte[] bytes)
        {
            this.Add(name, (object)bytes);
        }

        public void Remove(string name)
        {
            if (this.m_innerDict.Remove(name))
            {
                this.OnPropertyChanged(name);
            }
        }

        public bool ContainKey(string key)
        {
            return this.m_innerDict.ContainsKey(key);
        }

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        
    }
}
