using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Scripting;

namespace Internal
{
    public class SimpleEncryptionService : IEncryptionService
    {
        private readonly byte[] Key;
        private readonly byte[] IV;

        public SimpleEncryptionService()
        {
#if (UNITY_EDITOR || DEVELOPMENT_BUILD)
            // Константные ключи для Editor и Development Build ===
            //
            // 32 байта и 16 байт
            //
            Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
            IV = Encoding.UTF8.GetBytes("1234567890123456");

#else
            // Динамические ключи для релизных сборок ===
            (Key, IV) = EncryptionKeyManagement.GetOrCreateKeyAndIV();
#endif
        }

        public string Encrypt(string plainText)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = IV;

                    using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    {
                        var plainBytes = Encoding.UTF8.GetBytes(plainText);
                        var encryptedBytes = PerformCryptography(plainBytes, encryptor);
                        return Convert.ToBase64String(encryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Encryption failed: {ex.Message}");
                throw;
            }
        }

        public string Decrypt(string cipherText)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = Key;
                    aes.IV = IV;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    {
                        var cipherBytes = Convert.FromBase64String(cipherText);
                        var decryptedBytes = PerformCryptography(cipherBytes, decryptor);

                        return Encoding.UTF8.GetString(decryptedBytes);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Decryption failed: {ex.Message}");
                throw new InvalidDataException("Failed to decrypt the data. The data might be corrupted.");
            }
        }

        public bool IsEncrypted(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return false;

            try
            {
                // Попробуем проверить Base64-декодирование //
                // Так как я в редакторе часто его юзаю //
                var decoded = Convert.FromBase64String(data);

                // Минимальная длина данных для шифрования AES (блок 16 байт) //
                return decoded.Length >= 16;
            }
            catch
            {
                // Если ошибка, значит, это не зашифрованные данные //
                // Что мне для Editor мода нормуль. //
                return false;
            }
        }


        private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
        {
            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, offset: 0, count: data.Length);
                    cryptoStream.FlushFinalBlock();

                    return memoryStream.ToArray();
                }
            }
        }

        [Preserve]
        public static class EncryptionKeyManagement
        {
            private const string KeyPref = "EncryptionKey";
            private const string IvPref = "EncryptionIV";

            [Preserve]
            public static (byte[] Key, byte[] IV) GetOrCreateKeyAndIV()
            {
#if UNITY_EDITOR
                // Debug.LogWarning("Editor mode detected: Using constant keys.");

                // 32 байта и 16 байт, что бы не забыть.
                return (Encoding.UTF8.GetBytes("12345678901234567890123456789012"),
                    Encoding.UTF8.GetBytes("1234567890123456"));
#else
            if (!PlayerPrefs.HasKey(KeyPref) || !PlayerPrefs.HasKey(IvPref))
            {
                // Генерация случайных значений ===
                var key = GenerateRandomKey(32);
                var iv = GenerateRandomKey(16);

                // Сохранение в PlayerPrefs (да-да, для телефонов лучше использовать Secure Storage) ===
                PlayerPrefs.SetString(KeyPref, Convert.ToBase64String(key));
                PlayerPrefs.SetString(IvPref, Convert.ToBase64String(iv));
                PlayerPrefs.Save();

                Debug.Log("New encryption key and IV generated.");
            }

            // Загрузка ключа и IV ===
            var savedKey = Convert.FromBase64String(PlayerPrefs.GetString(KeyPref));
            var savedIv = Convert.FromBase64String(PlayerPrefs.GetString(IvPref));

            return (savedKey, savedIv);
#endif
            }

            [Preserve]
            private static byte[] GenerateRandomKey(int length)
            {
                using (var rng = new RNGCryptoServiceProvider())
                {
                    var key = new byte[length];
                    rng.GetBytes(key);
                    return key;
                }
            }
        }
    }
}