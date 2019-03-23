using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Schubert.Helpers
{
    /// <summary>
    /// 综合工具类
    /// </summary>
    public static class ToolHelper
    {
        /// <summary>
        /// 转换为16位字符串（不重复）。
        /// </summary>
        /// <returns></returns>
        public static string NewShortId()
        {
            long i = 1;
            foreach (byte b in Guid.NewGuid().ToByteArray())
            {
                i *= ((int)b + 1);
            }
            return string.Format("{0:x}", i - DateTime.Now.Ticks);
        }

        /// <summary>
        /// Xml反序列化对象
        /// </summary>
        /// <typeparam name="T">要反序列化的对象类型</typeparam>
        /// <param name="xmlText">Xml字符串</param>
        /// <returns>反序列化得到的对象</returns>
        public static T XmlDeserialize<T>(string xmlText)
        {
            return (T)XmlDeserialize(xmlText, typeof(T));
        }
        /// <summary>
        /// Xml序列化对象
        /// </summary>
        /// <typeparam name="T">要序列化的对象类型</typeparam>
        /// <param name="instance">要序列化对象的实例</param>
        /// <returns>序列化后得到的XML字符串</returns>
        public static string XmlSerialize<T>(T instance)
        {
            return XmlSerialize(instance, typeof(T));
        }

        /// <summary>
        /// Xml反序列化对象
        /// </summary>
        public static object XmlDeserialize(string xmlText, Type type)
        {
            using (StringReader reader = new StringReader(xmlText))
            {
                XmlSerializer serializer = new XmlSerializer(type);
                object instance = serializer.Deserialize(reader);
                return instance;
            }
        }
        /// <summary>
        /// Xml序列化对象
        /// </summary>
        public static string XmlSerialize(object instance, Type type)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                XmlSerializer serializer = new XmlSerializer(type);
                serializer.Serialize(stream, instance);
                byte[] bytes = stream.ToArray();
                return Encoding.UTF8.GetString(bytes);
            }
        }



        /// <summary>
        /// 转换字节数组为16进制字符串
        /// </summary>
        public static string ByteToHex(byte[] byteArray)
        {
            string outString = "";

            foreach (Byte b in byteArray)
                outString += b.ToString("X2");

            return outString;
        }

        /// <summary>
        /// 转换16进制字符串为字节数组
        /// </summary>
        public static byte[] HexToByte(string hexString)
        {
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Byte.Parse(hexString.Substring(i * 2, 2), NumberStyles.HexNumber);
            return returnBytes;
        }

        /// <summary>
        /// 以UTF8编码方式编写XML。
        /// </summary>
        /// <param name="writeAction">编写操作。</param>
        /// <param name="indent">是否缩进。</param>
        /// <param name="conformanceLevel">指示XML是文档还是片段。</param>
        /// <returns>XML 字符串。</returns>
        public static string WriteUtf8Xml(Action<XmlWriter> writeAction, bool indent = true, ConformanceLevel conformanceLevel = ConformanceLevel.Document)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = indent;
            settings.ConformanceLevel = conformanceLevel;
            settings.Encoding = Encoding.UTF8;
            using (MemoryStream stream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(stream, settings))
                {
                    if (writeAction != null)
                    {
                        writeAction.Invoke(writer);
                    }
                    byte[] content = stream.ToArray();
                    string xml = Encoding.UTF8.GetString(content, 0, content.Length);
                    return xml;
                }
            }
        }

        /// <summary>
        /// 将一个html颜色转还为rgb颜色（#ffffff => 255,255,255）
        /// </summary>
        /// <param name="htmlColor">HTML 颜色代码</param>
        /// <returns></returns>
        public static string HtmlColorToRgb(string htmlColor)
        {
            Guard.ArgumentNullOrWhiteSpaceString(htmlColor, nameof(htmlColor));
            byte a, r, g, b;
            GetHtmlColorARgb(htmlColor, out a, out r, out g, out b);
            return $"{r},{g},{b}";
        }

        /// <summary>
        /// 获取HTML颜色的 ARGB 值。
        /// </summary>
        /// <param name="htmlColor">HTML 颜色代码</param>
        /// <param name="a">A</param>
        /// <param name="r">R</param>
        /// <param name="g">G</param>
        /// <param name="b">B</param>
        public static void GetHtmlColorARgb(string htmlColor, out byte a, out byte r, out byte g, out byte b)
        {
            Guard.ArgumentNullOrWhiteSpaceString(htmlColor, nameof(htmlColor));
            if (htmlColor.CaseInsensitiveEquals("transparent"))
            {
                a = 0;
                r = 255;
                g = 255;
                b = 255;
                return;
            }
            string code = htmlColor.Replace("#", String.Empty);
            if (code.Length == 6)
            {
                code = $"FF{code}";
            }
            if (code.Length != 8)
            {
                throw new ArgumentException(nameof(htmlColor), "HTML 颜色代码没有使用标准6位或8位格式（透明颜色必须为 transparent）。");
            }
            a = byte.Parse(code.Substring(0, 2), NumberStyles.HexNumber);
            r = byte.Parse(code.Substring(2, 2), NumberStyles.HexNumber);
            g = byte.Parse(code.Substring(4, 2), NumberStyles.HexNumber);
            b = byte.Parse(code.Substring(6, 2), NumberStyles.HexNumber);
        }

        /// <summary>
        /// 获取 HTML 颜色的灰度（灰阶值，用来表示颜色明亮）。
        /// </summary>
        /// <returns></returns>
        public static int GetColorGreyScale(string htmlColor)
        {
            Guard.ArgumentNullOrWhiteSpaceString(htmlColor, nameof(htmlColor));
            byte a, r, g, b;
            GetHtmlColorARgb(htmlColor, out a, out r, out g, out b);
            return GetColorGreyScale(r, g, b);
        }

        /// <summary>
        /// 获取 RGB颜色的灰度（灰阶值，用来表示颜色明亮）。
        /// </summary>
        /// <returns></returns>
        public static int GetColorGreyScale(byte r, byte g, byte b)
        {
            Double dY = r * 0.299 + g * 0.587 + b * 0.114;
            return (int)Math.Round(dY, 0);
        }

    }
}
