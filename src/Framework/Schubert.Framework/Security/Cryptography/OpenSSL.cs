using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Schubert.Framework.Security.Cryptography
{
    /// <summary>
    /// 提供 OpenSSL 兼容的加密解密和签名、验签等操作。
    /// 该类提供算法可以兼容 java 程序、支付宝等接口。
    /// </summary>
    public static class OpenSSL
    {
        /// <summary>
        /// 格式化 Pem 密钥。
        /// </summary>
        /// <param name="keyFileContent">PEM文件内容</param>
        /// <returns></returns>
        public static string FormatKey(string keyFileContent)
        {
            string key = System.Text.RegularExpressions.Regex.Replace(keyFileContent, @"(-+(\w|\s)*-+)", String.Empty);
            return key.Replace("\r\n", String.Empty).Replace("\n", String.Empty);
        }

        /// <summary>
        /// 使用 OpenSSL 兼容的方式对数据进行签名。
        /// </summary>
        /// <param name="data">要签名的数据。</param>
        /// <param name="privateKeyPem">pem 私钥（可以包含-----BEGIN RSA PRIVATE KEY----  和-----END RSA PRIVATE KEY-----）。</param>
        /// <param name="charset">签名使用的字符编码（默认UTF8）。</param>
        /// <returns></returns>
        public static string RSASign(string data, string privateKeyPem, string charset = "utf-8")
        {
            string keyString = FormatKey(privateKeyPem);
            var key = Convert.FromBase64String(keyString);
            using (RSA rsaCsp = DecodePemPrivateKey(key))
            {
                byte[] dataBytes = null;
                if (string.IsNullOrWhiteSpace(charset))
                {
                    dataBytes = Encoding.UTF8.GetBytes(data);
                }
                else
                {
                    dataBytes = Encoding.GetEncoding(charset).GetBytes(data);
                }

                byte[] signatureBytes = rsaCsp.SignData(dataBytes, HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);

                return Convert.ToBase64String(signatureBytes);
            }
        }

        /// <summary>
        /// 对 OpenSSL 数字签名进行验证。
        /// </summary>
        /// <param name="signContent">要验证的数据。</param>
        /// <param name="sign">用来验证数据的数字签名。</param>
        /// <param name="publicKeyPem">pem 公钥（可以包含-----BEGIN RSA PUBLIC KEY----  和-----END RSA PUBLIC KEY-----）。</param>
        /// <param name="charset">签名使用的字符编码（默认UTF8）。</param>
        /// <returns></returns>
        public static bool RSAVerify(string signContent, string sign, string publicKeyPem, string charset = "utf-8")
        {
            try
            {
                string keyString = FormatKey(publicKeyPem);
                charset = charset.IfNullOrWhiteSpace("utf-8");
                var key = Convert.FromBase64String(keyString);
                using (RSA rsa = DecodePemPublicKey(key))
                {
                    SetPersistKeyInCsp(rsa);
                    bool bVerifyResultOriginal = rsa.VerifyData(Encoding.GetEncoding(charset).GetBytes(signContent), Convert.FromBase64String(sign), HashAlgorithmName.SHA1, RSASignaturePadding.Pkcs1);
                    return bVerifyResultOriginal;

                }
            }
            catch(Exception ex)
            {
                ex.ThrowIfNecessary();
                return false;
            }

        }

        private static void SetPersistKeyInCsp(this RSA rsa)
        {
            RSACryptoServiceProvider p = rsa as RSACryptoServiceProvider;
            if (p != null)
            {
                p.PersistKeyInCsp = false;
            }
        }

        /// <summary>
        ///  使用 pem 公钥进行数据加密。
        /// </summary>
        /// <param name="content">要加密的内容。</param>
        /// <param name="publicKeyPem">使用的 PEM 公钥（可以包含-----BEGIN RSA PUBLIC KEY----  和-----END RSA PUBLIC KEY-----）。</param>
        /// <param name="charset">加密使用的字符编码（默认UTF8）。</param>
        /// <returns></returns>
        public static string RSAEncrypt(string content, string publicKeyPem, string charset = "utf-8")
        {
            try
            {
                charset = charset.IfNullOrWhiteSpace("utf-8");
                byte[] data = Encoding.GetEncoding(charset).GetBytes(content);
                var encrypted = RSAEncrypt(publicKeyPem, data);
                return Convert.ToBase64String(encrypted);
            }
            catch (Exception ex)
            {
                ex.ThrowIfNecessary();
                throw new SchubertException("EncryptContent = " + content + ",charset = " + charset, ex);
            }
        }

        /// <summary>
        ///  使用 pem 公钥进行数据加密。
        /// </summary>
        /// <param name="data">要加密的内容。</param>
        /// <param name="publicKeyPem">使用的 PEM 公钥（可以包含-----BEGIN RSA PUBLIC KEY----  和-----END RSA PUBLIC KEY-----）。</param>
        /// <returns></returns>
        public static byte[] RSAEncrypt(string publicKeyPem, byte[] data)
        {
            string keyString = FormatKey(publicKeyPem);
            var key = Convert.FromBase64String(keyString);
            using (RSA rsa = DecodePemPublicKey(key))
            {
                rsa.SetPersistKeyInCsp();
                int maxBlockSize = rsa.KeySize / 8 - 11; //加密块最大长度限制
                if (data.Length <= maxBlockSize)
                {
                    byte[] cipherbytes = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
                    return cipherbytes;
                }
                using (MemoryStream plaiStream = new MemoryStream(data))
                {
                    using (MemoryStream crypStream = new MemoryStream())
                    {
                        Byte[] buffer = new Byte[maxBlockSize];
                        int blockSize = plaiStream.Read(buffer, 0, maxBlockSize);
                        while (blockSize > 0)
                        {
                            Byte[] toEncrypt = new Byte[blockSize];
                            Array.Copy(buffer, 0, toEncrypt, 0, blockSize);
                            Byte[] cryptograph = rsa.Encrypt(toEncrypt, RSAEncryptionPadding.Pkcs1);
                            crypStream.Write(cryptograph, 0, cryptograph.Length);
                            blockSize = plaiStream.Read(buffer, 0, maxBlockSize);
                        }

                        return crypStream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        ///  使用 pem 私钥进行数据加密。
        /// </summary>
        /// <param name="content">已加密的数据。</param>
        /// <param name="privateKeyPem">使用的 PEM 私钥（可以包含-----BEGIN RSA PRIVATE KEY----  和-----END RSA PRIVATE KEY-----）。</param>
        /// <param name="charset">加密使用的字符编码（默认UTF8）。</param>
        /// <returns></returns>
        public static string RSADecrypt(string content, string privateKeyPem, string charset = "utf-8")
        {
            try
            {
                var data = RSADecrypt(Convert.FromBase64String(content), privateKeyPem);
                return Encoding.GetEncoding(charset.IfNullOrWhiteSpace("utf-8")).GetString(data);
            }
            catch (Exception ex)
            {
                ex.ThrowIfNecessary();
                throw new SchubertException("RSA 加密失败。", ex);
            }
        }

        /// <summary>
        /// 使用 pem 私钥进行数据加密。
        /// </summary>
        /// <param name="content">已加密的数据。</param>
        /// <param name="privateKeyPem">使用的 PEM 私钥（可以包含-----BEGIN RSA PRIVATE KEY----  和-----END RSA PRIVATE KEY-----）。</param>
        /// <returns></returns>
        public static byte[] RSADecrypt(byte[] content, string privateKeyPem)
        {
            string keyString = FormatKey(privateKeyPem);
            var key = Convert.FromBase64String(keyString);
            using (RSA rsaCsp = DecodePemPrivateKey(key))
            {
                byte[] data = content;
                int maxBlockSize = rsaCsp.KeySize / 8; //解密块最大长度限制
                if (data.Length <= maxBlockSize)
                {
                    byte[] cipherbytes = rsaCsp.Decrypt(data, RSAEncryptionPadding.Pkcs1);
                    return cipherbytes;
                }
                using (MemoryStream crypStream = new MemoryStream(data))
                {
                    using (MemoryStream plaiStream = new MemoryStream())
                    {
                        Byte[] buffer = new Byte[maxBlockSize];
                        int blockSize = crypStream.Read(buffer, 0, maxBlockSize);
                        while (blockSize > 0)
                        {
                            Byte[] toDecrypt = new Byte[blockSize];
                            Array.Copy(buffer, 0, toDecrypt, 0, blockSize);
                            Byte[] cryptograph = rsaCsp.Decrypt(toDecrypt, RSAEncryptionPadding.Pkcs1);
                            plaiStream.Write(cryptograph, 0, cryptograph.Length);
                            blockSize = crypStream.Read(buffer, 0, maxBlockSize);
                        }
                        return plaiStream.ToArray();
                    }
                }
            }
        }

        /// <summary>
        /// 读取 pem 私钥。
        /// </summary>
        /// <param name="pemFileContent">使用的 PEM 私钥（可以包含-----BEGIN RSA PRIVATE KEY----  和-----END RSA PRIVATE KEY-----）。</param>
        /// <returns>已经导入了私钥的 RSA 加密算法提供程序。</returns>
        public static RSA OpenRSAPrivateKey(string pemFileContent)
        {
            string keyString = FormatKey(pemFileContent);
            var key = Convert.FromBase64String(keyString);
            var rsaCsp = DecodePemPrivateKey(key);
            return rsaCsp; 
        }

        /// <summary>
        /// 读取 pem 公钥。
        /// </summary>
        /// <param name="pemFileContent">使用的 PEM 公钥（可以包含-----BEGIN RSA PUBLIC KEY----  和-----END RSA PUBLIC KEY-----）。</param>
        /// <returns>已经导入了公钥的 RSA 加密算法提供程序。</returns>
        public static RSA OpenRSAPublicKey(string pemFileContent)
        {
            string keyString = FormatKey(pemFileContent);
            var key = Convert.FromBase64String(keyString);
            var rsaCsp = DecodePemPublicKey(key);
            return rsaCsp;
        }

        private static RSA DecodePemPrivateKey(byte[] privkey)
        {
            byte[] MODULUS, E, D, P, Q, DP, DQ, IQ;

            // --------- Set up stream to decode the asn.1 encoded RSA private key ------
            using (MemoryStream mem = new MemoryStream(privkey))
            {
                using (BinaryReader binr = new BinaryReader(mem))  //wrap Memory Stream with BinaryReader for easy reading
                {
                    byte bt = 0;
                    ushort twobytes = 0;
                    int elems = 0;
                    try
                    {
                        twobytes = binr.ReadUInt16();
                        if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                            binr.ReadByte();    //advance 1 byte
                        else if (twobytes == 0x8230)
                            binr.ReadInt16();    //advance 2 bytes
                        else
                            return null;

                        twobytes = binr.ReadUInt16();
                        if (twobytes != 0x0102) //version number
                            return null;
                        bt = binr.ReadByte();
                        if (bt != 0x00)
                            return null;


                        //------ all private key components are Integer sequences ----
                        elems = GetIntegerSize(binr);
                        MODULUS = binr.ReadBytes(elems);

                        elems = GetIntegerSize(binr);
                        E = binr.ReadBytes(elems);

                        elems = GetIntegerSize(binr);
                        D = binr.ReadBytes(elems);

                        elems = GetIntegerSize(binr);
                        P = binr.ReadBytes(elems);

                        elems = GetIntegerSize(binr);
                        Q = binr.ReadBytes(elems);

                        elems = GetIntegerSize(binr);
                        DP = binr.ReadBytes(elems);

                        elems = GetIntegerSize(binr);
                        DQ = binr.ReadBytes(elems);

                        elems = GetIntegerSize(binr);
                        IQ = binr.ReadBytes(elems);


                        
                        RSAParameters rsaParams = new RSAParameters();
                        rsaParams.Modulus = MODULUS;
                        rsaParams.Exponent = E;
                        rsaParams.D = D;
                        rsaParams.P = P;
                        rsaParams.Q = Q;
                        rsaParams.DP = DP;
                        rsaParams.DQ = DQ;
                        rsaParams.InverseQ = IQ;
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                            CspParameters cspParameters = new CspParameters();
                            cspParameters.Flags = CspProviderFlags.UseMachineKeyStore;
                            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(cspParameters);
                            rsa.ImportParameters(rsaParams);
                            return rsa;
                        }
                        else
                        {
                            RSAOpenSsl ssl = new RSAOpenSsl(rsaParams);
                            return ssl;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ThrowIfNecessary();
                        throw new SchubertException("解析 pem 私钥失败。", ex);
                    }
                }
            }
        }

        private static RSA DecodePemPublicKey(byte[] publickey)
        {
            // encoded OID sequence for  PKCS #1 rsaEncryption szOID_RSA_RSA = "1.2.840.113549.1.1.1"
            byte[] SeqOID = { 0x30, 0x0D, 0x06, 0x09, 0x2A, 0x86, 0x48, 0x86, 0xF7, 0x0D, 0x01, 0x01, 0x01, 0x05, 0x00 };
            byte[] seq = new byte[15];
            // ---------  Set up stream to read the asn.1 encoded SubjectPublicKeyInfo blob  ------
            using (MemoryStream mem = new MemoryStream(publickey))
            {
                using (BinaryReader binr = new BinaryReader(mem))    //wrap Memory Stream with BinaryReader for easy reading
                {
                    byte bt = 0;
                    ushort twobytes = 0;

                    try
                    {

                        twobytes = binr.ReadUInt16();
                        if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                            binr.ReadByte();    //advance 1 byte
                        else if (twobytes == 0x8230)
                            binr.ReadInt16();   //advance 2 bytes
                        else
                            return null;

                        seq = binr.ReadBytes(15);       //read the Sequence OID
                        if (!CompareByteArrays(seq, SeqOID))    //make sure Sequence for OID is correct
                            return null;

                        twobytes = binr.ReadUInt16();
                        if (twobytes == 0x8103) //data read as little endian order (actual data order for Bit String is 03 81)
                            binr.ReadByte();    //advance 1 byte
                        else if (twobytes == 0x8203)
                            binr.ReadInt16();   //advance 2 bytes
                        else
                            return null;

                        bt = binr.ReadByte();
                        if (bt != 0x00)     //expect null byte next
                            return null;

                        twobytes = binr.ReadUInt16();
                        if (twobytes == 0x8130) //data read as little endian order (actual data order for Sequence is 30 81)
                            binr.ReadByte();    //advance 1 byte
                        else if (twobytes == 0x8230)
                            binr.ReadInt16();   //advance 2 bytes
                        else
                            return null;

                        twobytes = binr.ReadUInt16();
                        byte lowbyte = 0x00;
                        byte highbyte = 0x00;

                        if (twobytes == 0x8102) //data read as little endian order (actual data order for Integer is 02 81)
                            lowbyte = binr.ReadByte();  // read next bytes which is bytes in modulus
                        else if (twobytes == 0x8202)
                        {
                            highbyte = binr.ReadByte(); //advance 2 bytes
                            lowbyte = binr.ReadByte();
                        }
                        else
                            return null;
                        byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };   //reverse byte order since asn.1 key uses big endian order
                        int modsize = BitConverter.ToInt32(modint, 0);

                        byte firstbyte = binr.ReadByte();
                        binr.BaseStream.Seek(-1, SeekOrigin.Current);

                        if (firstbyte == 0x00)
                        {   //if first byte (highest order) of modulus is zero, don't include it
                            binr.ReadByte();    //skip this null byte
                            modsize -= 1;   //reduce modulus buffer size by 1
                        }

                        byte[] modulus = binr.ReadBytes(modsize);   //read the modulus bytes

                        if (binr.ReadByte() != 0x02)            //expect an Integer for the exponent data
                            return null;
                        int expbytes = (int)binr.ReadByte();        // should only need one byte for actual exponent data (for all useful values)
                        byte[] exponent = binr.ReadBytes(expbytes);

                        RSAParameters rsaParameters = new RSAParameters();
                        rsaParameters.Modulus = modulus;
                        rsaParameters.Exponent = exponent;

                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                        {
                            // ------- create RSACryptoServiceProvider instance and initialize with public key -----
                            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                            rsa.ImportParameters(rsaParameters);
                            return rsa;
                        }
                        else
                        {
                            RSAOpenSsl ssl = new RSAOpenSsl(rsaParameters);
                            return ssl;
                        }
                    }
                    catch (Exception ex)
                    {
                        ex.ThrowIfNecessary();
                        throw new SchubertException("解析 pem 公钥失败。", ex);
                    }

                }
            }
        }

        private static int GetIntegerSize(BinaryReader binr)
        {
            byte bt = 0;
            byte lowbyte = 0x00;
            byte highbyte = 0x00;
            int count = 0;
            bt = binr.ReadByte();
            if (bt != 0x02)		//expect integer
                return 0;
            bt = binr.ReadByte();

            if (bt == 0x81)
                count = binr.ReadByte();	// data size in next byte
            else
                if (bt == 0x82)
            {
                highbyte = binr.ReadByte(); // data size in next 2 bytes
                lowbyte = binr.ReadByte();
                byte[] modint = { lowbyte, highbyte, 0x00, 0x00 };
                count = BitConverter.ToInt32(modint, 0);
            }
            else
            {
                count = bt;     // we already have the data size
            }

            while (binr.ReadByte() == 0x00)
            {	//remove high order zeros in data
                count -= 1;
            }
            binr.BaseStream.Seek(-1, SeekOrigin.Current);		//last ReadByte wasn't a removed zero, so back up a byte
            return count;
        }

        private static bool CompareByteArrays(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return false;
            int i = 0;
            foreach (byte c in a)
            {
                if (c != b[i])
                    return false;
                i++;
            }
            return true;
        }
    }
}
