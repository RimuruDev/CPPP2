using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Internal
{
    public static class MemoryWithXor
    {
        public static byte[] EncryptInt(int value) =>
            XorEncryptionService.EncryptIntWithXor(value);

        public static int DecryptInt(byte[] encryptedValue) =>
            XorEncryptionService.DecryptIntWithXor(encryptedValue);


        public static byte[] EncryptFloat(float value) =>
            XorEncryptionService.EncryptFloatWithXor(value);

        public static float DecryptFloat(byte[] encryptedValue) =>
            XorEncryptionService.DecryptFloatWithXor(encryptedValue);

        public static byte[] EncryptBool(bool value) =>
            XorEncryptionService.EncryptBoolWithXor(value);

        public static bool DecryptBool(byte[] encryptedValue) =>
            XorEncryptionService.DecryptBoolWithXor(encryptedValue);

        public static List<byte[]> EncryptList(List<int> list) =>
            XorEncryptionService.EncryptListWithXor(list);

        public static List<int> DecryptList(List<byte[]> encryptedList) =>
            XorEncryptionService.DecryptListWithXor(encryptedList);


        public static byte[] Encrypt(byte[] data) =>
            XorEncryptionService.EncryptWithXor(data);

        public static byte[] Decrypt(byte[] encryptedData) =>
            XorEncryptionService.DecryptWithXor(encryptedData);


        public static string Encrypt(string value) =>
            XorEncryptionService.EncryptWithXor(value);

        public static string Decrypt(string encryptedValue) =>
            XorEncryptionService.DecryptWithXor(encryptedValue);
    }

    public static class XorEncryptionService
    {
        #region Примитивы

        public static byte[] EncryptIntWithXor(int value)
        {
            return EncryptWithXor(BitConverter.GetBytes(value));
        }

        public static int DecryptIntWithXor(byte[] encryptedValue)
        {
            var decryptedBytes = DecryptWithXor(encryptedValue);
            return BitConverter.ToInt32(decryptedBytes, 0);
        }

        public static byte[] EncryptFloatWithXor(float value)
        {
            return EncryptWithXor(BitConverter.GetBytes(value));
        }

        public static float DecryptFloatWithXor(byte[] encryptedValue)
        {
            var decryptedBytes = DecryptWithXor(encryptedValue);
            return BitConverter.ToSingle(decryptedBytes, 0);
        }

        public static byte[] EncryptBoolWithXor(bool value)
        {
            return EncryptWithXor(BitConverter.GetBytes(value));
        }

        public static bool DecryptBoolWithXor(byte[] encryptedValue)
        {
            var decryptedBytes = DecryptWithXor(encryptedValue);
            return BitConverter.ToBoolean(decryptedBytes, 0);
        }

        #endregion

        #region Коллекции

        public static List<byte[]> EncryptListWithXor(List<int> list)
        {
            var encryptedList = new List<byte[]>();
            foreach (var item in list)
            {
                encryptedList.Add(EncryptIntWithXor(item));
            }

            return encryptedList;
        }

        public static List<int> DecryptListWithXor(List<byte[]> encryptedList)
        {
            var decryptedList = new List<int>();
            foreach (var item in encryptedList)
            {
                decryptedList.Add(DecryptIntWithXor(item));
            }

            return decryptedList;
        }

        #endregion

        #region Основные методы шифрования и дешифрования с использованием XOR

        public static byte[] EncryptWithXor(byte[] data)
        {
            var result = new byte[data.Length];

            for (var i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ XOR64.Key[i % XOR64.Key.Length]);
            }

            return result;
        }

        public static byte[] DecryptWithXor(byte[] encryptedData)
        {
            return EncryptWithXor(encryptedData);
        }

        public static string EncryptWithXor(string value)
        {
            return XOR64.Encode(value);
        }

        public static string DecryptWithXor(string encryptedValue)
        {
            return XOR64.Decode(encryptedValue);
        }

        #endregion

        #region Object

        /// <summary>
        /// Преобразуем объект в байты
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] SerializeObject(object obj)
        {
            IFormatter formatter = new BinaryFormatter();

            using (var stream = new MemoryStream())
            {
                formatter.Serialize(stream, obj);

                return stream.ToArray();
            }
        }

        /// <summary>
        /// Восстановление объекта из байтового массива
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Шифрования объекта (класса или структуры)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] EncryptObjectWithXor(object obj)
        {
            var objectData = SerializeObject(obj);

            return EncryptWithXor(objectData);
        }

        /// <summary>
        /// Дешифрования объекта (класса или структуры)
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <returns></returns>
        public static object DecryptObjectWithXor(byte[] encryptedData)
        {
            var decryptedData = DecryptWithXor(encryptedData);

            return DeserializeObject(decryptedData);
        }

        #endregion
    }
}