using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;

namespace Internal
{
    /// <summary>
    /// Сервис для шифрования данных с использованием AES и нескольких фейковых ключей для защиты настоящего ключа.
    /// </summary>
    public class SimpleEncryptionService : IEncryptionService
    {
        private byte[] Key;
        private byte[] IV;

        /// <summary>
        /// Конструктор, который либо загружает существующий зашифрованный ключ из PlayerPrefs, 
        /// либо генерирует и сохраняет новый.
        /// </summary>
        public SimpleEncryptionService()
        {
            if (PlayerPrefs.HasKey(EncryptionConstants.EncryptedKeyPref))
                LoadKey();
            else
                GenerateAndSaveKey(); // TODO: Генерировать фейки можно асинхронно, что бы не замедлять на 1-2 секунды запуск игры.
        }

        /// <summary>
        /// Шифрует текст с использованием AES.
        /// </summary>
        /// <param name="plainText">Текст для шифрования.</param>
        /// <returns>Зашифрованный текст в формате Base64.</returns>
        public string Encrypt(string plainText)
        {
            try
            {
                using (var aes = Aes.Create())
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

        /// <summary>
        /// Дешифрует текст с использованием AES.
        /// </summary>
        /// <param name="cipherText">Зашифрованный текст в формате Base64.</param>
        /// <returns>Дешифрованный текст.</returns>
        public string Decrypt(string cipherText)
        {
            if (!IsEncrypted(cipherText))
            {
                Debug.LogError("Attempting to decrypt data that is not encrypted.");
                throw new InvalidDataException("Data is not encrypted or is invalid.");
            }

            try
            {
                using (var aes = Aes.Create())
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

        /// <summary>
        /// Проверяет, является ли строка зашифрованной.
        /// </summary>
        /// <param name="data">Строка для проверки.</param>
        /// <returns>True, если строка зашифрована, иначе False.</returns>
        public bool IsEncrypted(string data)
        {
            if (string.IsNullOrWhiteSpace(data)) return false;

            try
            {
                var decoded = Convert.FromBase64String(data);
              
                return decoded.Length >= EncryptionConstants.MinEncryptedDataLength;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        /// <summary>
        /// Выполняет криптографическую операцию (шифрование или дешифрование) с использованием указанного крипто-потока.
        /// </summary>
        /// <param name="data">Данные для шифрования или дешифрования.</param>
        /// <param name="cryptoTransform">Трансформер криптографической операции.</param>
        /// <returns>Результат криптографической операции.</returns>
        private byte[] PerformCryptography(byte[] data, ICryptoTransform cryptoTransform)
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

        /// <summary>
        /// Генерирует и сохраняет новый зашифрованный ключ и IV.
        /// Также создает фейковые ключи для дополнительной защиты.
        /// </summary>
        private void GenerateAndSaveKey()
        {
            using (var aes = Aes.Create())
            {
                aes.GenerateKey();
                aes.GenerateIV();
                Key = aes.Key;
                IV = aes.IV;
            }

            // Переворачиваем ключ для дополнительной защиты //
            Key = ReverseByteArray(Key);

            // Преобразуем настоящий ключ в несколько фейковых //
            var fakeKeys = GenerateFakeKeys();
            var encryptedFakeKeys = EncryptFakeKeys(fakeKeys);

            // Шифруем настоящий ключ с использованием мастер-ключа //
            var encryptedKey = EncryptWithMasterKey(Key);
            var encryptedIV = EncryptWithMasterKey(IV);

            // Сохраняем зашифрованные ключи и фейковые в PlayerPrefs //
            PlayerPrefs.SetString(EncryptionConstants.EncryptedKeyPref, Convert.ToBase64String(encryptedKey));
            PlayerPrefs.SetString(EncryptionConstants.EncryptedIVPref, Convert.ToBase64String(encryptedIV));
        
            // Фейковые ключи //
            PlayerPrefs.SetString(EncryptionConstants.FakeKeysPref, string.Join(",", encryptedFakeKeys));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Загружает зашифрованный ключ и IV из PlayerPrefs.
        /// </summary>
        private void LoadKey()
        {
            var encryptedKey = Convert.FromBase64String(PlayerPrefs.GetString(EncryptionConstants.EncryptedKeyPref));
            var encryptedIV = Convert.FromBase64String(PlayerPrefs.GetString(EncryptionConstants.EncryptedIVPref));
            var fakeKeys = PlayerPrefs.GetString(EncryptionConstants.FakeKeysPref).Split(',');

            // Расшифровываем ключ и IV //
            Key = DecryptWithMasterKey(encryptedKey);
            IV = DecryptWithMasterKey(encryptedIV);

            // Дополнительно можно распечатать фейковые ключи для анализа //
            Debug.Log($"Fake Keys: {string.Join(", ", fakeKeys)}");
        }

        /// <summary>
        /// Шифрует данные с использованием мастер-ключа.
        /// </summary>
        /// <param name="data">Данные для шифрования.</param>
        /// <returns>Зашифрованные данные.</returns>
        private byte[] EncryptWithMasterKey(byte[] data)
        {
            using (var aes = Aes.Create())
            {
                // Убедись, что длина ключа соответствует допустимым размерам:
                // 32 -> 256 бит | Это исправление ошибки:
                //  - Error: CryptographicException: Specified key is not a valid size for this algorithm.
                aes.Key = new byte[32]; 
                var keyBytes = Encoding.UTF8.GetBytes(EncryptionConstants.MasterKey);
              
                // Обрезаем если нужно ===
                Array.Copy(keyBytes, aes.Key, Math.Min(keyBytes.Length, aes.Key.Length));

                // Используем первые 16 байтов как IV //
                aes.IV = aes.Key.Take(EncryptionConstants.IVLength).ToArray();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                    return PerformCryptography(data, encryptor);
            }
        }


        /// <summary>
        /// Дешифрует данные с использованием мастер-ключа.
        /// </summary>
        /// <param name="encryptedData">Зашифрованные данные.</param>
        /// <returns>Дешифрованные данные.</returns>
        private byte[] DecryptWithMasterKey(byte[] encryptedData)
        {
            using (var aes = Aes.Create())
            {
                // Загружаем и проверяем размер ключа //
                var keyBytes = Encoding.UTF8.GetBytes(EncryptionConstants.MasterKey);
              
                // Убедись, что длина ключа соответствует допустимым размерам:
                // 32 -> 256 бит | Это исправление ошибки:
                //  - Error: CryptographicException: Specified key is not a valid size for this algorithm.
                aes.Key = new byte[32];
                
                // Обрезаем! ===
                Array.Copy(keyBytes, aes.Key, Math.Min(keyBytes.Length, aes.Key.Length)); 

                // Используем первые 16 байтов мастер-ключа как IV //
                aes.IV = aes.Key.Take(EncryptionConstants.IVLength).ToArray();

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    return PerformCryptography(encryptedData, decryptor);
            }
        }


        /// <summary>
        /// Генерирует список фейковых ключей для усложнения поиска настоящего ключа.
        /// </summary>
        /// <returns>Список фейковых ключей.</returns>
        private List<string> GenerateFakeKeys()
        {
            var fakeKeys = new List<string>();
            
            // Базовый ключ для создания фейков //
            // Главное на релизе в конфиге не забыть поменять:D
            var baseKey = EncryptionConstants.BaseKey;

            //////////////////////////////////////////////////////////////////
            // Генерация фейковых ключей через XOR или другие трансформации //
            //////////////////////////////////////////////////////////////////
            for (var i = 0; i < 5; i++)
            {
                var fakeKey = XORTransform(baseKey, i);
                
                fakeKeys.Add(fakeKey);
            }

            return fakeKeys;
        }

        /// <summary>
        /// Преобразует ключ с использованием операции XOR.
        /// </summary>
        /// <param name="baseKey">Базовый ключ.</param>
        /// <param name="index">Индекс для изменения ключа.</param>
        /// <returns>Измененный ключ.</returns>
        private string XORTransform(string baseKey, int index)
        {
            var transformed = new char[baseKey.Length];
           
            for (var i = 0; i < baseKey.Length; i++)
                transformed[i] = (char)(baseKey[i] ^ (index + 1));

            return new string(transformed);
        }

        /// <summary>
        /// Шифрует фейковые ключи с использованием мастер-ключа.
        /// </summary>
        /// <param name="fakeKeys">Список фейковых ключей.</param>
        /// <returns>Зашифрованные фейковые ключи.</returns>
        private List<string> EncryptFakeKeys(List<string> fakeKeys)
        {
            var encryptedKeys = new List<string>();

            foreach (var fakeKey in fakeKeys)
            {
                var encryptedFakeKey = EncryptWithMasterKey(Encoding.UTF8.GetBytes(fakeKey));
                
                encryptedKeys.Add(Convert.ToBase64String(encryptedFakeKey));
            }

            return encryptedKeys;
        }

        /// <summary>
        /// Переворачивает массив байтов.
        /// </summary>
        /// <param name="data">Массив байтов для переворота.</param>
        /// <returns>Перевернутый массив байтов.</returns>
        private byte[] ReverseByteArray(byte[] data)
        {
            Array.Reverse(data);
            
            return data;
        }
    }
}