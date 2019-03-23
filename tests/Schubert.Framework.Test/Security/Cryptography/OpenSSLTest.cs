using Schubert.Framework.Security.Cryptography;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Schubert.Framework.Test.Security.Cryptography
{
    public class OpenSSLTest
    {
        private string _privateKey = @"-----BEGIN RSA PRIVATE KEY-----
MIICXgIBAAKBgQCa/7jms4JVGTkOzlkZ++t8CCw8DStNQOtywFLh4JWFx4a2LNjt
jpgYo/o7njq3SH+J/uXyMkGzfkxh3hvNsLrzY/T+F4V5fO9eunWsoa3lTyy+BAWm
Odk4MNNAGQ2lnt2De/EfkhPow2EyNpAVL8WLgakaw7Tq3Gb2stE0LljzIwIDAQAB
AoGBAIOkPvkJat81veUaUkikUjsUgeU61hUV1yKtv3cCFFv7uykMa+1PF1SOKF/s
Ijg6RcABEnEiR/TXhq30Qy7uM8hjMZQ0HPZbnsrIe/LtlNPamT99W8AMuCpvdBqw
Hg/k7kQjYqpNnHDI3wbEc+jriNSt1dBfrQKQy+63UHdRUObpAkEAyVAqzprgQZXU
VqdeU5cfB9ak5SL9M/jzHjwBVqlNgiVOVAkPbGqmREfvtr2NFBhd6DeLECGYCrXG
lnkozAsf/QJBAMUavoQUpcRK2BRYc8g0j982hOWsiL48oIa5eOb9UN0f7OvdKgtZ
lIXlsvHVzyowNdodxCnAgXFbrjeMOr+P+Z8CQAOVsN1y9pFUaK6OVmiopT3Pfaoy
4E1fnnyoVuHDLAUoQufOLX8huwo2ObeIUo3MDUgITSqhXoK6T+n4CFjxzcUCQQC5
+41PNehgerq/H+NIQwiKd3gY+58f2jciSLojQ11c+TXmLQ7yHLm/Skl6VeQfi9QU
lse1GddKLlcKRQBeJy1DAkEApXjWdSjWL6ApVZM/YZrGOvUV/41+nBAh4r8k6pve
+Rvo7Fcjuk7zqVLzdyEn5ZsjQA9AzJZWoF80jkUaF9tfNQ==
-----END RSA PRIVATE KEY-----
";
        private string _publicKey = @"-----BEGIN PUBLIC KEY-----
MIGfMA0GCSqGSIb3DQEBAQUAA4GNADCBiQKBgQCa/7jms4JVGTkOzlkZ++t8CCw8
DStNQOtywFLh4JWFx4a2LNjtjpgYo/o7njq3SH+J/uXyMkGzfkxh3hvNsLrzY/T+
F4V5fO9eunWsoa3lTyy+BAWmOdk4MNNAGQ2lnt2De/EfkhPow2EyNpAVL8WLgaka
w7Tq3Gb2stE0LljzIwIDAQAB
-----END PUBLIC KEY-----
";
        private string _private2048Key = @"-----BEGIN RSA PRIVATE KEY-----
MIIEpAIBAAKCAQEAzyWk2sQNuCS5xDDngag+oNGezsVYJuB5rP/4Ke0OCFfXP0iN
7yVUSoW0Wg+l1qyBnMBU58NckzvkWY2AXLJ/Az8FZ0qWAJZIzbqM/we3QpWpvVaU
lgkaZ0k/faG5CilrusFh744q0yfS65fa4UxdXSUNihreF6ckjKFvfEzBqk27ZUiu
O60/s+R1p5tBUFXQr6tQ6hqD/o9QSc3wh37Ho8xS0SZ+yUgZcofENiMFyzXnZrn+
KbRxRmczTEHB4yNofCO7jGnVeXD4ovx24unTGqpdUR0GY0F38YxbE65JaMfFlZay
vp/Wc6/TlLligqeNa3HYZe8MR9s5NCXTZ/wX8QIDAQABAoIBAQCz6FbYZYQg1Uy9
91dpxYy3MbfCj6TzBuzGcv2+tBMG7fuVC9exxvMBUkSEH6kB3Ispb+WN8J+7hD3c
BUhhuekUEa0Iu7+xvNR4UThZ2wKwAroMJmEgOcHDyNsqqsXB7J2S8pezbf8Fq0XF
tfq0yBUia6bptlIDqvfUZ/UiuKtO02lMc29rp5tkQxC178Pbl2QUlcUj96E0uYPG
h1STGX1pLJDbY/vSlkjLb9EjAGUEAzI4wiEXD4oN94j3IzPWAczXjglGL5OF/8oj
0MJGNyVpYMgG7z4FBe0BFoZCvEN/6/V+ejKo+3626MDIIF5jt3Apw1e9kIkV2wEJ
fZczIlvtAoGBAPU/kweJelArtb4Pm4a2Ms1fpKm6yOj32qC/Ljpg9TUjREOl1bmN
agIlgKr54UXLwC2koVMiIWoIvSR3UdjqaUEUzwwsel1n9ugLiVhv59o9pvXwJbRF
ZZuAUEg6CM2HWxgkRxhpQoljny2ylzk7iT4tjC/wE3s0QT8SUYGTYob7AoGBANg6
dVKBNxv4EnxYupHn8ciRvcr5PGYFKO3ObM1xGBl7TQTUUZslphkzPFIhjnvaRcjR
AR+sq4x9VHbLtRA1N5/rlXcM0MrElYf5Q616aK1L5ZCWVWlCm++yUmOmFC/v+zNh
OEg3EVsH0ZxZUv1UyG66jQRGc5eh4PanPCTjEhkDAoGAS+AjaQ2Lngon7Gl/wKnW
Bdw1YZ28Uvd72IfNkZo1wv7qO9Ouz/2Ecq2PpVYx8BodlwF8N/AzTk7t1b5kwCul
7NC3ThjksslbhmcrUwUsQkUYxrZJtABUc0u7it0JpCzgbhOrO041m7QKp2S9jKNy
zp3g5WPtYwzDsvPD68bhkMkCgYEAqCJMuCrhv03WkfosmOfSijNJcVr8LBg3CzNI
Rzd5ldbavLab/hf4YAAHF5YgRQ4k1VIvnYGWo1eRJg6gbEn1RtTZFAlTEVhrLaAV
j/9vBdHOX++F2qOAvZHbnsC1UdE5c2pVaVHonPAnfXu3nGUgtXk5zek6WN33H9RK
YCRxHiECgYABarGC6d1hTcthkxWVM1mOEpKxwZXzwVyqb2aCUgf1H9+yiRKP7ooE
q4B31UTEUZpswNxsArzdjxo2qlCp8moHx4M/r+H6rXjXoYNVatUtpE5R91ugIYr6
AI3PpvlrTGoILM2oJi73zb909ikiyrobqaCdaDXOPLcmMHcwYPSa6A==
-----END RSA PRIVATE KEY-----";

        private string _public2048Key = @"-----BEGIN PUBLIC KEY-----
MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAzyWk2sQNuCS5xDDngag+
oNGezsVYJuB5rP/4Ke0OCFfXP0iN7yVUSoW0Wg+l1qyBnMBU58NckzvkWY2AXLJ/
Az8FZ0qWAJZIzbqM/we3QpWpvVaUlgkaZ0k/faG5CilrusFh744q0yfS65fa4Uxd
XSUNihreF6ckjKFvfEzBqk27ZUiuO60/s+R1p5tBUFXQr6tQ6hqD/o9QSc3wh37H
o8xS0SZ+yUgZcofENiMFyzXnZrn+KbRxRmczTEHB4yNofCO7jGnVeXD4ovx24unT
GqpdUR0GY0F38YxbE65JaMfFlZayvp/Wc6/TlLligqeNa3HYZe8MR9s5NCXTZ/wX
8QIDAQAB
-----END PUBLIC KEY-----";

        [Fact(DisplayName ="OpenSSL: 1024 验签")]
        public void RSASignAndVerify()
        {
            string sign = OpenSSL.RSASign("ABCDeFGH", _privateKey);
            bool valid = OpenSSL.RSAVerify("ABCDeFGH", sign, _publicKey);
            Assert.True(valid);
        }

        [Fact(DisplayName = "OpenSSL: 2048 验签")]
        public void RSASignAndVerify2048()
        {
            string sign = OpenSSL.RSASign("ABCDeFGH", _private2048Key);
            bool valid = OpenSSL.RSAVerify("ABCDeFGH", sign, _public2048Key);
            Assert.True(valid);
        }

        [Fact(DisplayName = "OpenSSL: 1024 加密")]
        public void RSAEntry()
        {
            string mask = OpenSSL.RSAEncrypt("ABCDeFGH", _publicKey);
            string opened = OpenSSL.RSADecrypt(mask, _privateKey);
            Assert.Equal("ABCDeFGH", opened);
        }

        [Fact(DisplayName = "OpenSSL: 2048 加密")]
        public void RSAEntry2048()
        {
            string mask = OpenSSL.RSAEncrypt("ABCDeFGH", _public2048Key);
            string opened = OpenSSL.RSADecrypt(mask, _private2048Key);
            Assert.Equal("ABCDeFGH", opened);
        }
    }
}
