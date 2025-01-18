using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
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

        public static void GenerateSessionKey()
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
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();

                    return memoryStream.ToArray();
                }
            }
        }

        #region Примитивы

        public static byte[] EncryptIntWithAes(int value)
        {
            return EncryptData(BitConverter.GetBytes(value));
        }

        public static int DecryptIntWithAes(byte[] encryptedValue)
        {
            var decryptedBytes = DecryptData(encryptedValue);
            return BitConverter.ToInt32(decryptedBytes, 0);
        }

        public static byte[] EncryptFloatWithAes(float value)
        {
            return EncryptData(BitConverter.GetBytes(value));
        }

        public static float DecryptFloatWithAes(byte[] encryptedValue)
        {
            var decryptedBytes = DecryptData(encryptedValue);
            return BitConverter.ToSingle(decryptedBytes, 0);
        }

        public static byte[] EncryptBoolWithAes(bool value)
        {
            return EncryptData(BitConverter.GetBytes(value));
        }

        public static bool DecryptBoolWithAes(byte[] encryptedValue)
        {
            var decryptedBytes = DecryptData(encryptedValue);
            return BitConverter.ToBoolean(decryptedBytes, 0);
        }

        #endregion

        #region Коллекции

        public static List<byte[]> EncryptListWithAes(List<int> list)
        {
            var encryptedList = new List<byte[]>();
            foreach (var item in list)
            {
                encryptedList.Add(EncryptIntWithAes(item));
            }

            return encryptedList;
        }

        public static List<int> DecryptListWithAes(List<byte[]> encryptedList)
        {
            var decryptedList = new List<int>();
            foreach (var item in encryptedList)
            {
                decryptedList.Add(DecryptIntWithAes(item));
            }

            return decryptedList;
        }

        #endregion

        #region Строки

        public static string EncryptStringWithAes(string value)
        {
            byte[] data = Encoding.UTF8.GetBytes(value);
            byte[] encryptedData = EncryptData(data);
            return Convert.ToBase64String(encryptedData);
        }

        public static string DecryptStringWithAes(string encryptedValue)
        {
            byte[] encryptedData = Convert.FromBase64String(encryptedValue);
            byte[] decryptedData = DecryptData(encryptedData);
            return Encoding.UTF8.GetString(decryptedData);
        }

        #endregion

        #region Объекты

        public static byte[] SerializeObject(object obj)
        {
            IFormatter formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);
                return stream.ToArray();
            }
        }

        public static object DeserializeObject(byte[] data)
        {
            IFormatter formatter = new BinaryFormatter();

            using (var stream = new MemoryStream(data))
            {
                return formatter.Deserialize(stream);
            }
        }

        public static T DeserializeObject<T>(byte[] data)
        {
            IFormatter formatter = new BinaryFormatter();

            using (var stream = new MemoryStream(data))
            {
                return (T)formatter.Deserialize(stream);
            }
        }

        public static byte[] EncryptObjectWithAes(object obj)
        {
            var objectData = SerializeObject(obj);
            return EncryptData(objectData);
        }

        public static object DecryptObjectWithAes(byte[] encryptedData)
        {
            var decryptedData = DecryptData(encryptedData);
            return DeserializeObject(decryptedData);
        }

        #endregion
    }
}