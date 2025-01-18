using System;
using System.Security.Cryptography;
using System.Text;

namespace Internal
{
    public static class FileUtility
    {
        /// <summary>
        /// Генерирует зашифрованное имя файла для файлов сохранений.
        /// </summary>
        /// <param name="originalFileName">Исходное имя файла.</param>
        /// <returns>Зашифрованное имя файла.</returns>
        public static string GetEncryptedFileName(string originalFileName)
        {
            return EncryptFileName(originalFileName);
        }

        /// <summary>
        /// Преобразует имя файла с использованием алгоритма SHA256.
        /// *NOTE: Обратно конвертнуть уже не выйдет!!!
        /// </summary>
        /// <param name="fileName">Имя файла для шифрования.</param>
        /// <returns>Зашифрованное имя файла.</returns>
        private static string EncryptFileName(string fileName)
        {
            using (var sha256 = SHA256.Create())
            {
                // Преобразуем имя файла в байты и шифруем его с использованием SHA256 //
                var fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                var hashBytes = sha256.ComputeHash(fileNameBytes);
                
                // Возвращаем как строку без дефисов //
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }
}