/*  This source file title is generate by a tool "CodeHeader" that made by Sharping  */
/*
===============================================================================
 Sharping Software Factory Addin
 Author:Sharping      CreateTime:2009-03-13 23:14:59 
===============================================================================
 Copyright ? Sharping.  All rights reserved.
 THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY
 OF ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT
 LIMITED TO THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
 FITNESS FOR A PARTICULAR PURPOSE.
===============================================================================
*/

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Schubert
{
    public static class ProfileSerializer
    {
        public static void ConvertFromProfilePropertyCollection(ProfilePropertyCollection properties, out string allNames, out string valuesString, out byte[] valuesBinary, bool binarySupported)
        {
            StringBuilder names = new StringBuilder();
            StringBuilder values = new StringBuilder();
            valuesBinary = null;
            MemoryStream ms = (binarySupported ? new System.IO.MemoryStream() : null);

            try
            {
                foreach (string name in properties.Names)
                {
                    int len = 0, startPos = 0;
                    string propValue = null;

                    object sVal = properties[name];

                    if (sVal == null)
                    {
                        len = -1;
                    }
                    else
                    {
                        if (!(sVal is string) && !binarySupported)
                        {
                            sVal = Convert.ToBase64String((byte[])sVal);
                        }

                        if (sVal is string)
                        {
                            propValue = (string)sVal;
                            len = propValue.Length;
                            startPos = values.Length;
                        }
                        else
                        {
                            byte[] b2 = (byte[])sVal;
                            startPos = (int)ms.Position;
                            ms.Write(b2, 0, b2.Length);
                            ms.Position = startPos + b2.Length;
                            len = b2.Length;
                        }
                    }

                    names.Append(name + ":" + ((propValue != null) ? "S" : "B") +
                                 ":" + startPos.ToString(CultureInfo.InvariantCulture) + ":" + len.ToString(CultureInfo.InvariantCulture) + ":");
                    if (propValue != null)
                        values.Append(propValue);
                }

                if (binarySupported)
                {
                    valuesBinary = ms.ToArray();
                }
            }
            finally
            {
                if (ms != null)
                    ms.Dispose();
            }
            allNames = names.ToString();
            valuesString = values.ToString();
        }

        public static ProfilePropertyCollection ConvertToProfilePropertyCollection(string allNames, string valuesString, byte[] valuesBinary)
        {
            ProfilePropertyCollection collection = new ProfilePropertyCollection();
            if (allNames.IsNullOrEmpty() || allNames.Split(':').Length < 4)
            {
                return collection;
            }
            string[] names = allNames.Split(':');
            string values = valuesString ?? String.Empty;
            byte[] buf = valuesBinary ?? new byte[0];
            for (int iter = 0; iter < names.Length / 4; iter++)
            {
                string name = names[iter * 4];
                object proValue = null;

                int startPos = Int32.Parse(names[iter * 4 + 2], CultureInfo.InvariantCulture);
                int length = Int32.Parse(names[iter * 4 + 3], CultureInfo.InvariantCulture);

                if (names[iter * 4 + 1] == "S" && startPos >= 0 && length > 0 && values.Length >= startPos + length)
                {
                    proValue = values.Substring(startPos, length);
                }

                if (names[iter * 4 + 1] == "B" && startPos >= 0 && length > 0 && buf.Length >= startPos + length)
                {
                    byte[] buf2 = new byte[length];

                    Buffer.BlockCopy(buf, startPos, buf2, 0, length);

                    proValue = buf2;
                }
                collection.Add(name, proValue);
            }
            return collection;
        }
    }
}
