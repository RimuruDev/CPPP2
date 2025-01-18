using System;
using System.IO;
using UnityEngine;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Internal
{
    public static class FakeKeyUtility
    {
        private static string fakeKeysFolderPath;

        /// <summary>
        /// Генерирует фейковые ключи и сохраняет их в базе данных.
        /// </summary>
        /// <param name="fakeKeyCount">Количество фейковых ключей для генерации</param>
        /// <param name="folderPath">Путь для сохранения фейковых ключей</param>
        public static void GenerateAndSaveFakeKeys(int fakeKeyCount, string folderPath = null)
        {
            fakeKeysFolderPath = string.IsNullOrEmpty(folderPath)
                ? Application.persistentDataPath
                : folderPath;

            var fakeKeys = GenerateFakeKeys(fakeKeyCount);
            SaveFakeKeysToFile(fakeKeys);
        }

        /// <summary>
        /// Генерирует список фейковых ключей для усложнения поиска настоящего ключа.
        /// </summary>
        /// <returns>Список фейковых ключей.</returns>
        private static List<string> GenerateFakeKeys(int count)
        {
            if (count <= 0)
            {
                Debug.LogError("FakeKeys count must be greater than 0. Set default to 5.");
                count = 5;
            }

            var fakeKeys = new List<string>();

            var baseKey = EncryptionConstants.BaseKey;

            for (var i = 0; i < count; i++)
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
        private static string XORTransform(string baseKey, int index)
        {
            var transformed = new char[baseKey.Length];

            for (var i = 0; i < baseKey.Length; i++)
                transformed[i] = (char)(baseKey[i] ^ (index + 1));

            return new string(transformed);
        }

        /// <summary>
        /// Сохраняет фейковые ключи в файл.
        /// </summary>
        /// <param name="fakeKeys">Список фейковых ключей.</param>
        private static void SaveFakeKeysToFile(List<string> fakeKeys)
        {
            try
            {
                if (!Directory.Exists(fakeKeysFolderPath))
                    Directory.CreateDirectory(fakeKeysFolderPath);

                for (var i = 0; i < fakeKeys.Count; i++)
                {
                    // Генерация зашифрованного имени файла для каждого ключа //
                    var fileName = EncryptFileName($"FakeKey_{i + 1}");
                    var fakeKeyFilePath = Path.Combine(fakeKeysFolderPath, fileName);

                    File.WriteAllText(fakeKeyFilePath, fakeKeys[i]);
                }

                Debug.Log("<color=green>[FakeKeyUtility] Fake keys generated and saved successfully!</color>");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save fake keys: {ex.Message}");
            }
        }

        /// <summary>
        /// Зашифровывает имя файла для предотвращения угадывания структуры.
        /// </summary>
        /// <param name="fileName">Имя файла для шифрования.</param>
        /// <returns>Зашифрованное имя файла.</returns>
        private static string EncryptFileName(string fileName)
        {
            using (var sha256 = SHA256.Create())
            {
                // Преобразуем имя файла в байты и шифруем его с использованием SHA256
                var fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                var hashBytes = sha256.ComputeHash(fileNameBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Загружает фейковые ключи из файла.
        /// </summary>
        /// <returns>Список фейковых ключей.</returns>
        public static List<string> LoadFakeKeys()
        {
            try
            {
                // Проверяем, существует ли папка
                if (Directory.Exists(fakeKeysFolderPath))
                {
                    var fakeKeyFiles = Directory.GetFiles(fakeKeysFolderPath, "FakeKey_*.txt");
                    var fakeKeys = new List<string>();

                    foreach (var file in fakeKeyFiles)
                    {
                        var keyData = File.ReadAllText(file);
                        fakeKeys.Add(keyData);
                    }

                    return fakeKeys;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load fake keys: {ex.Message}");
            }

            return new List<string>();
        }
    }
}