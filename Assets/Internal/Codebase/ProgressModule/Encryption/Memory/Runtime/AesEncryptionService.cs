using System.Security.Cryptography;
using UnityEngine;

namespace Internal
{
    public static class AesEncryptionService
    {
        private static byte[] sessionKey;
        private static byte[] sessionIV;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeSessionKey()
        {
            GenerateSessionKey();
        }

        private static void GenerateSessionKey()
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();

                sessionKey = aes.Key;
                sessionIV = aes.IV;
            }
        }

        public static byte[] EncryptData(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = sessionKey;
                aes.IV = sessionIV;
                aes.Padding = PaddingMode.PKCS7;

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, encryptor);
                }
            }
        }

        public static byte[] DecryptData(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                aes.Key = sessionKey;
                aes.IV = sessionIV;
                aes.Padding = PaddingMode.PKCS7;

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, decryptor);
                }
            }
        }

        private static byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var memoryStream = new System.IO.MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();

                    return memoryStream.ToArray();
                }
            }
        }
    }
}