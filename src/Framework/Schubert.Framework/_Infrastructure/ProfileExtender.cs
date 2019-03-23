/*  This source file title is generate by a tool "CodeHeader" that made by Sharping  */
/*
===============================================================================
 Sharping Software Factory Addin
 Author:Sharping      CreateTime:2009-03-13 23:13:13 
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
using System.Reflection;
using System.ComponentModel;
using System.Dynamic;
using System.Linq.Expressions;

namespace Schubert
{
    public class ProfileExtender : IDynamicMetaObjectProvider
    {
        private ProfilePropertyCollection m_extendedAttributes = null;
        private IProfileStorage m_data = null;

        private bool _propertyChanged = false;
        private bool _serializerDataChanged = false;

        private bool _binarySupported = false;
        private bool _lazyRefresh = false;

        private string _propertyNamesPropertyName = null;
        private string _propertyStringPropertyName = null;
        private string _propertyBinaryPropertyName = null;

        public ProfileExtender(bool binarySupported = true, bool lazyRefresh = true, IProfileStorage storage = null)
        {
            this.m_extendedAttributes = new ProfilePropertyCollection();
            this.m_extendedAttributes.PropertyChanged += new PropertyChangedEventHandler(extendedAttributes_PropertyChanged);
            this.m_data = storage ?? new ProfileSerializerData();
            this.RefreshProperties();
            this.m_data.PropertyChanged += new PropertyChangedEventHandler(m_data_PropertyChanged);
            this._binarySupported = binarySupported;
            this._lazyRefresh = lazyRefresh;

            Expression<Func<IProfileStorage, String>> exNames = s => s.PropertyNames;
            this._propertyNamesPropertyName = exNames.GetMemberName();

            Expression<Func<IProfileStorage, String>> propertValueNames = s => s.PropertyValuesString;
            this._propertyStringPropertyName = propertValueNames.GetMemberName();

            Expression<Func<IProfileStorage, byte[]>> propertBinaryValueNames = s => s.PropertyValuesBinary;
            this._propertyBinaryPropertyName = propertBinaryValueNames.GetMemberName();
        }

        private bool IsStorageProperties(PropertyChangedEventArgs e)
        {
            return (e.PropertyName == this._propertyNamesPropertyName || e.PropertyName == this._propertyStringPropertyName || e.PropertyName == this._propertyBinaryPropertyName);
        }

        void m_data_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this.IsStorageProperties(e))
            {
                if (this._lazyRefresh)
                {
                    this._serializerDataChanged = true;
                }
                else
                {
                    this.RefreshProperties();
                }
            }
        }

        void extendedAttributes_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (this._lazyRefresh)
            {
                this._propertyChanged = true;
            }
            else
            {
                this.RefreshSerializerData();
            }
        }

        public bool BinarySupported
        {
            get { return this._binarySupported; }
        }

        public int ProfilePropertiesCount
        {
            get { return this.ExtendedAttributes.Count; }
        }

        public IEnumerable<String> GetProfilePropertyNames()
        {
            return ExtendedAttributes.Names;
        }

        public bool ContainsProfileProperty(string propertName)
        {
            return this.ExtendedAttributes.ContainKey(propertName);
        }

        public object GetProfileProperty(string name)
        {
            object returnValue = this.ExtendedAttributes[name];
            if (returnValue == null || returnValue is String)
            {
                return new ConvertibleString((String)returnValue);
            }
            else
            {
                return returnValue;
            }
        }

        private object InternalSetProfileProperty(string name, object value)
        {
            if (value == null)
            {
                this.ExtendedAttributes.Remove(name);
            }
            else
            {
                Type type = value.GetType();
                if (value is byte[])
                {
                    this.ExtendedAttributes[name] = value;
                }
                else
                {
                    this.ExtendedAttributes[name] = value.ToString();
                }
            }
            return value;
        }

        public void SetProfileProperty(string name, object value)
        {
            this.InternalSetProfileProperty(name, value);
        }

        protected ProfilePropertyCollection ExtendedAttributes
        {
            get
            {
                if (this._serializerDataChanged)
                {
                    this.RefreshProperties();
                    this._serializerDataChanged = false;
                }
                return this.m_extendedAttributes;
            }
        }

        public IProfileStorage SerializerData
        {
            get
            {
                if (this._propertyChanged)
                {
                    this.RefreshSerializerData();
                    this._propertyChanged = false;
                }
                return this.m_data;
            }
            set
            {
                if (this.m_data != value)
                {
                    this.m_data.PropertyChanged -= new PropertyChangedEventHandler(m_data_PropertyChanged);
                    this.m_data = value;
                    if (this._lazyRefresh)
                    {
                        this._serializerDataChanged = true;
                    }
                    else
                    {
                        this.RefreshProperties();
                    }
                    this.m_data.PropertyChanged -= new PropertyChangedEventHandler(m_data_PropertyChanged);
                    this.m_data.PropertyChanged += new PropertyChangedEventHandler(m_data_PropertyChanged);
                }
            }
        }

        #region Serialization

        protected virtual void RefreshSerializerData()
        {
            this.m_data.PropertyChanged -= new PropertyChangedEventHandler(m_data_PropertyChanged);

            string names = null;
            string values = null;
            byte[] bytes = null;

            ProfileSerializer.ConvertFromProfilePropertyCollection(this.m_extendedAttributes, out names, out values, out bytes, this.BinarySupported);
            this.m_data.PropertyNames = names;
            this.m_data.PropertyValuesString = values;
            this.m_data.PropertyValuesBinary = bytes;

            this.m_data.PropertyChanged -= new PropertyChangedEventHandler(m_data_PropertyChanged);
            this.m_data.PropertyChanged += new PropertyChangedEventHandler(m_data_PropertyChanged);
        }

        protected virtual void RefreshProperties()
        {
            this.m_extendedAttributes.PropertyChanged -= new PropertyChangedEventHandler(extendedAttributes_PropertyChanged);

            IProfileStorage data = this.SerializerData ?? new ProfileSerializerData();
            var attributes = ProfileSerializer.ConvertToProfilePropertyCollection(data.PropertyNames, data.PropertyValuesString, data.PropertyValuesBinary);

            ProfilePropertyCollection collection = attributes ?? new ProfilePropertyCollection();
            this.m_extendedAttributes.Clear();
            foreach (string name in collection.Names)
            {
                this.m_extendedAttributes.Add(name, collection[name]);
            }

            this.m_extendedAttributes.PropertyChanged -= new PropertyChangedEventHandler(extendedAttributes_PropertyChanged);
            this.m_extendedAttributes.PropertyChanged += new PropertyChangedEventHandler(extendedAttributes_PropertyChanged);
        }

        
        #endregion

        #region ICloneable 成员

        public object Clone()
        {
            ProfileExtender ex = new ProfileExtender();

            foreach (string name in this.m_extendedAttributes.Names)
            {
                ex.m_extendedAttributes.Add(name, this.m_extendedAttributes[name]);
            }
            return ex;
        }

        #endregion

        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return new ExtenderMetaObject(parameter, this);
        }

        private class ExtenderMetaObject : DynamicMetaObject
        {
            private ProfileExtender m_extender = null;

            internal ExtenderMetaObject(Expression parameter, ProfileExtender extender)
                : base(parameter, BindingRestrictions.Empty, extender)
            {
                m_extender = extender;
            }

            public string GetMemberName<TProperty>(Expression<Func<ProfileExtender, TProperty>> propertyExpression)
            {
                return propertyExpression.GetMemberName();
            }

            public override DynamicMetaObject BindConvert(ConvertBinder binder)
            {
                return base.BindConvert(binder);
            }

            public override DynamicMetaObject BindSetMember(SetMemberBinder binder, DynamicMetaObject value)
            {
                string methodName = nameof(ProfileExtender.InternalSetProfileProperty);
                BindingRestrictions restrictions = BindingRestrictions.GetTypeRestriction(Expression, LimitType);

                Expression[] args = new Expression[2];

                // First parameter is the name of the property to Set
                args[0] = Expression.Constant(binder.Name);
                // Second parameter is the value
                args[1] = Expression.Convert(value.Expression, typeof(object));
 

                // Setup the 'this' reference
                Expression self = Expression.Convert(Expression, LimitType);
                // Setup the method call expression
                Expression methodCall = Expression.Call(self, typeof(ProfileExtender).GetType().GetTypeInfo().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance), args);
 

                // Create a meta object to invoke Set later:
                DynamicMetaObject setProfileProperty = new DynamicMetaObject(methodCall, restrictions);

                return setProfileProperty;

            }

            public override DynamicMetaObject BindGetMember(GetMemberBinder binder)
            {
                string methodName = nameof(ProfileExtender.GetProfileProperty);

                // One parameter
                Expression[] parameters = new Expression[] { Expression.Constant(binder.Name) };


                Expression methodCall = Expression.Call(
                                                Expression.Convert(Expression, LimitType),
                                                typeof(ProfileExtender).GetTypeInfo().GetMethod(methodName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance),
                                                parameters);

                BindingRestrictions restriction = BindingRestrictions.GetTypeRestriction(Expression, LimitType);

                DynamicMetaObject getProfileProperty = new DynamicMetaObject(methodCall, restriction);
                                        
                return getProfileProperty;


            }

        }

    }
}
