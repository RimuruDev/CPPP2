using System;
using System.Text;

namespace Internal
{
    public class XOR64
    {
        /// <summary>
        /// Генерация сессионного ключа (сессионный ключ создается один раз для игры).
        /// </summary>
        public static byte[] Key { get; private set; }

        static XOR64()
        {
            GenerateNewKey();
        }

        /// <summary>
        /// Генерация нового сессионного ключа.
        /// </summary>
        public static void GenerateNewKey()
        {
            Key = Guid.NewGuid().ToByteArray();
        }

        /// <summary>
        /// Шифрование строки с помощью XOR и Base64.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Encode(string value)
        {
            return Convert.ToBase64String(Encode(Encoding.UTF8.GetBytes(value), Key));
        }

        /// <summary>
        /// Дешифровка строки с помощью XOR и Base64.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string Decode(string value)
        {
            return Encoding.UTF8.GetString(Encode(Convert.FromBase64String(value), Key));
        }

        /// <summary>
        /// Шифрование с заданным ключом.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Encrypt(string value, string key)
        {
            return Convert.ToBase64String(Encode(Encoding.UTF8.GetBytes(value), Encoding.UTF8.GetBytes(key)));
        }

        /// <summary>
        /// Дешифровка с заданным ключом.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string Decrypt(string value, string key)
        {
            return Encoding.UTF8.GetString(Encode(Convert.FromBase64String(value), Encoding.UTF8.GetBytes(key)));
        }

        /// <summary>
        /// Основная функция XOR шифрования.
        /// </summary>
        /// <param name="bytes"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        private static byte[] Encode(byte[] bytes, byte[] key)
        {
            var j = 0;

            // XOR каждой байт с ключом //
            // Что бы неповадно было !!! //
            for (var i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= key[j];

                // Переход по циклу ключа //
                if (++j == key.Length)
                {
                    j = 0;
                }
            }

            return bytes;
        }
    }
}