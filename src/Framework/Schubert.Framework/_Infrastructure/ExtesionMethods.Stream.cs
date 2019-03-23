/*
===============================================================================
 The comments is generate by a tool.

 Author:Sharping      CreateTime:2011-01-27 11:29 
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
using System.IO;

namespace System
{
    static partial class ExtensionMethods
    {
        public static StreamReader GetReader(this Stream stream)
        {
            return stream.GetReader(null);
        }

        public static StreamReader GetReader(this Stream stream, Encoding encoding)
        {
            if (stream.CanRead == false)
                throw new InvalidOperationException("Stream does not support reading.");
            encoding = (encoding ?? Encoding.UTF8);
            return new StreamReader(stream, encoding);
        }

        public static StreamWriter GetWriter(this Stream stream)
        {
            return stream.GetWriter(null);
        }

        public static StreamWriter GetWriter(this Stream stream, Encoding encoding)
        {
            if (stream.CanWrite == false)
                throw new InvalidOperationException("Stream does not support writing.");
            
            encoding = (encoding ?? Encoding.UTF8);
            return new StreamWriter(stream, encoding);
        }

        public static string ReadToEnd(this Stream stream)
        {
            return stream.ReadToEnd(null);
        }

        public static string ReadToEnd(this Stream stream, Encoding encoding)
        {
            using (var reader = stream.GetReader(encoding))
            {
                return reader.ReadToEnd();
            }
        }

        public static byte[] ReadBytesToEnd(this Stream stream, int bufferSize = 4049)
        {
            if (bufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(bufferSize), "Reading buffer size must rather than 0 .");
            }

            List<byte> bytes = new List<byte>();
            byte[] buffer = new byte[bufferSize];
            int byteCount;
            using (var reader = new BinaryReader(stream))
            {
                while ((byteCount = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    byte[] readBytes = new byte[byteCount];
                    Buffer.BlockCopy(buffer, 0, readBytes, 0, readBytes.Length);
                    bytes.AddRange(readBytes);
                }
            }
            return bytes.ToArray();
        }
    }
}
