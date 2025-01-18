namespace Internal
{
    /// <summary>
    /// Класс, содержащий константы для шифрования.
    /// </summary>
    public static class EncryptionConstants
    {
        /// <summary>
        /// Ключ для хранения зашифрованного ключа
        /// </summary>
        public const string EncryptedKeyPref = "EncryptedKey";

        /// <summary>
        /// Ключ для хранения зашифрованного IV
        /// </summary>
        public const string EncryptedIVPref = "EncryptedIV";

        /// <summary>
        /// Ключ для хранения фейковых ключей
        /// </summary>
        public const string FakeKeysPref = "FakeKeys";

        /// <summary>
        /// Мастер-ключ для шифрования
        /// </summary>
        public const string MasterKey = "MasterSecretKey1234567890123456";

        /// <summary>
        /// Базовый ключ для создания фейковых
        /// </summary>
        public const string BaseKey = "RealKeyForEncryption";

        /// <summary>
        ///  // Длина IV (в байтах)
        /// </summary>
        public const int IVLength = 16;

        /// <summary>
        /// Минимальная длина зашифрованных данных
        /// </summary>
        public const int MinEncryptedDataLength = 16;
    }
}