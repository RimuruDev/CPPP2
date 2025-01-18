using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace Internal
{
    public static class XorEncryptionObjectService
    {
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
            byte[] decryptedData = DecryptWithXor(encryptedData);
            return DeserializeObject(decryptedData);
        }

        /// <summary>
        /// Шифрование с использованием XOR
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] EncryptWithXor(byte[] data)
        {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                // Используем сессионный ключ //
                result[i] = (byte)(data[i] ^ XOR64.Key[i % XOR64.Key.Length]); 
            }
            return result;
        }

        /// <summary>
        /// Дешифровка с использованием XOR
        /// </summary>
        /// <param name="encryptedData"></param>
        /// <returns></returns>
        public static byte[] DecryptWithXor(byte[] encryptedData)
        {
            // XOR-шифрование симметричное //
            return EncryptWithXor(encryptedData);
        }
    }
}
