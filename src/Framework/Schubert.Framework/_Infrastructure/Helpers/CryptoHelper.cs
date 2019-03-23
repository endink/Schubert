using System;
using System.Security.Cryptography;
using System.Text;

namespace Schubert.Helpers
{
    /// <summary>
    /// 加密解密工具类
    /// </summary>
    public static class CryptoHelper
    {
        #region MD5

        /// <summary>
        /// 采用 MD5 32位加密数据
        /// </summary>
        /// <param name="data">要加密的数据</param>
        /// <param name="encoding">编码，为空使用 UTF-8 编码。</param>
        /// <param name="useBase64String">指示是否对结果使用BASE64编码。</param>
        /// <returns>加密后的数据</returns>
        public static string Encrypt32MD5(string data, Encoding encoding = null, bool useBase64String = true)
        {
            if (data.IsNullOrWhiteSpace())
            {
                return String.Empty;
            }
            encoding = encoding ?? Encoding.UTF8;
            using (MD5 md5 = MD5.Create())
            {
                byte[] buffer = md5.ComputeHash(encoding.GetBytes(data));
                string result =useBase64String ? Convert.ToBase64String(buffer) : encoding.GetString(buffer);
                return result;
            }
        }

        #endregion

    }
}
