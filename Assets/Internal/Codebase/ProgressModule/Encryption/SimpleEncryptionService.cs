using System;
using System.IO;
using System.Linq;
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
#if UNITY_ANDROID && !UNITY_EDITOR
            // Уникальный идентификатор устройства для Android //
            // Так как я зае&*ся с безопасным хранилищем ведроида...
            var deviceId = DeviceKeyGenerator.GetDeviceUniqueId();
            Key = Encoding.UTF8.GetBytes(deviceId.PadRight(32, '0'));  // Преобразуем в 32 байта //
            IV = Encoding.UTF8.GetBytes(deviceId.PadRight(16, '0'));   // Преобразуем в 16 байт //
#elif (UNITY_EDITOR || DEVELOPMENT_BUILD)
            // Константные ключи для Editor и Development Build
            Key = Encoding.UTF8.GetBytes("12345678901234567890123456789012");
            IV = Encoding.UTF8.GetBytes("1234567890123456");
#else
            // Для других платформ, например, Standalone, WebGL, fallback на хардкод ключ //
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
            if (!IsEncrypted(cipherText))
            {
                Debug.LogError("Attempting to decrypt data that is not encrypted.");
                throw new InvalidDataException("Data is not encrypted or is invalid.");
            }

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
                // Проверяем, является ли строка допустимой Base64 //
                var decoded = Convert.FromBase64String(data);

                // Минимальная длина данных для шифрования AES //
                return decoded.Length >= 16;
            }
            catch (FormatException)
            {
                return false;
            }
        }

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
    }

    [Preserve]
    public static class EncryptionKeyManagement
    {
        private const string KeyPref = "EncryptedEncryptionKey";
        private const string IvPref = "EncryptedEncryptionIV";

        // Пример ключа для зашифровки/расшифровки. Хотя при декомпайле пофигу что тут будет...
        private static readonly byte[] MasterKey = Encoding.UTF8.GetBytes("MasterSecretKey1234567890123456"); 

        public static (byte[] Key, byte[] IV) GetOrCreateKeyAndIV()
        {
            if (!PlayerPrefs.HasKey(KeyPref) || !PlayerPrefs.HasKey(IvPref))
            {
                var key = GenerateRandomKey(32);
                var iv = GenerateRandomKey(16);

                // Шифруем ключ и IV перед сохранением //
                var encryptedKey = EncryptWithMasterKey(key, MasterKey);
                var encryptedIv = EncryptWithMasterKey(iv, MasterKey);

                PlayerPrefs.SetString(KeyPref, Convert.ToBase64String(encryptedKey));
                PlayerPrefs.SetString(IvPref, Convert.ToBase64String(encryptedIv));
                PlayerPrefs.Save();
            }

            var encryptedSavedKey = Convert.FromBase64String(PlayerPrefs.GetString(KeyPref));
            var encryptedSavedIv = Convert.FromBase64String(PlayerPrefs.GetString(IvPref));

            // Расшифровываем ключ и IV //
            var key_ = DecryptWithMasterKey(encryptedSavedKey, MasterKey);
            var iv_ = DecryptWithMasterKey(encryptedSavedIv, MasterKey);

            return (key_, iv_);
        }

        private static byte[] EncryptWithMasterKey(byte[] data, byte[] masterKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = masterKey;
                aes.IV = masterKey.Take(16).ToArray();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(data, encryptor);
                }
            }
        }

        private static byte[] DecryptWithMasterKey(byte[] encryptedData, byte[] masterKey)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = masterKey;
                aes.IV = masterKey.Take(16).ToArray();

                using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                {
                    return PerformCryptography(encryptedData, decryptor);
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