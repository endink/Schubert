/*  This source file title is generate by a tool "CodeHeader" that made by Sharping  */
/*
===============================================================================
 Sharping Software Factory Addin
 Author:Sharping      CreateTime:2009-03-14 03:22:44 
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
using System.Runtime.Serialization;
using System.ComponentModel;
using Newtonsoft.Json;

namespace Schubert
{
    public class ProfileSerializerData : IProfileStorage
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _propertyNames;
        private string _propertyValuesString;
        private byte[] _propertyValuesBinary;


        public ProfileSerializerData()
        {
            this._propertyNames = null;
            this._propertyValuesString = null;
            this._propertyValuesBinary = null;
        }
        
        public string PropertyNames
        {
            get { return this._propertyNames; }
            set
            {
                if (this._propertyNames != value)
                {
                    this._propertyNames = value;
                    this.OnPropertyChanged(this.GetMemberName(i => i.PropertyNames));
                }
            }
        }
        
        public string PropertyValuesString
        {
            get { return this._propertyValuesString; }
            set
            {
                if (this._propertyValuesString != value)
                {
                    this._propertyValuesString = value;
                    this.OnPropertyChanged(this.GetMemberName(i => i.PropertyValuesString));
                }
            }
        }
        
        public byte[] PropertyValuesBinary
        {
            get { return this._propertyValuesBinary; }
            set
            {
                if (this._propertyValuesBinary != value)
                {
                    this._propertyValuesBinary = value;
                    this.OnPropertyChanged(this.GetMemberName(i => i.PropertyValuesBinary));
                }
            }
        }
        

        private void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public static bool SerializerDataEquals(IProfileStorage x, IProfileStorage y)
        {
            if (Object.ReferenceEquals(x, y))
            {
                return true;
            }
            else if ((x == null && y != null) || (x != null && y == null))
            {
                return false;
            }
            else
            {
                return
                x.PropertyNames.SafeEquals(y.PropertyNames) && 
                y.PropertyValuesString.SafeEquals(y.PropertyValuesString) &&
                x.PropertyValuesBinary.SafeEquals(y.PropertyValuesBinary);
            }
        }
    }

}
