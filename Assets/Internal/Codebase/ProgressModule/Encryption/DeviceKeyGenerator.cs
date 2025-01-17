#if UNITY_ANDROID
using System;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;

namespace Internal
{
    public static class DeviceKeyGenerator
    {
        /// <summary>
        /// Получение уникального Android ID
        /// </summary>
        public static string GetDeviceUniqueId()
        {
            // Получаем уникальный идентификатор устройства
            string androidId = SystemInfo.deviceUniqueIdentifier;

            // Преобразуем в байты для использования в шифровании
            byte[] deviceIdBytes = Encoding.UTF8.GetBytes(androidId);

            // Генерация ключа с использованием устройства
            using (SHA256 sha256 = SHA256.Create())
            {
                return Convert.ToBase64String(sha256.ComputeHash(deviceIdBytes));
            }
        }
    }
}
#endif