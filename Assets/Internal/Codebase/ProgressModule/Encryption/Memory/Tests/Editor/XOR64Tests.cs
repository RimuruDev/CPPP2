using NUnit.Framework;

namespace Internal.Tests
{
    [TestFixture]
    public class XOR64Tests
    {
        #region Генерация сессионного ключа

        [Test]
        public void GenerateNewKey_ShouldGenerateNewKey()
        {
            var initialKey = XOR64.Key;

            XOR64.GenerateNewKey();
            var newKey = XOR64.Key;

            Assert.AreNotEqual(initialKey, newKey);
        }

        #endregion

        #region Шифрование и дешифрование строк с сессионным ключом

        [Test]
        public void EncodeDecode_WithSessionKey_ShouldReturnOriginalString()
        {
            var originalValue = "TestString123!";
            
            var encryptedValue = XOR64.Encode(originalValue);
            var decryptedValue = XOR64.Decode(encryptedValue);

            Assert.AreEqual(originalValue, decryptedValue);
        }

        #endregion

        #region Шифрование и дешифрование с произвольным ключом

        [Test]
        public void EncryptDecrypt_WithCustomKey_ShouldReturnOriginalString()
        {
            var originalValue = "AnotherTest123!";
            var customKey = "CustomSecretKey";

            var encryptedValue = XOR64.Encrypt(originalValue, customKey);
            var decryptedValue = XOR64.Decrypt(encryptedValue, customKey);

            Assert.AreEqual(originalValue, decryptedValue);
        }

        #endregion

        #region Симметричность шифрования с использованием XOR

        [Test]
        public void EncryptDecrypt_WithSessionKey_ShouldReturnOriginalString()
        {
            var originalValue = "SymmetricTest";
            
            var encryptedValue = XOR64.Encode(originalValue);
            var decryptedValue = XOR64.Decode(encryptedValue);

            Assert.AreEqual(originalValue, decryptedValue);
        }

        #endregion

        #region Проверка на случайности с различными входами

        [Test]
        public void EncryptDecrypt_WithRandomStrings_ShouldReturnOriginalString()
        {
            var originalValue1 = "Test1";
            var originalValue2 = "Test2";

            var encryptedValue1 = XOR64.Encode(originalValue1);
            var encryptedValue2 = XOR64.Encode(originalValue2);

            var decryptedValue1 = XOR64.Decode(encryptedValue1);
            var decryptedValue2 = XOR64.Decode(encryptedValue2);

            Assert.AreNotEqual(encryptedValue1, encryptedValue2);

            Assert.AreEqual(originalValue1, decryptedValue1);
            Assert.AreEqual(originalValue2, decryptedValue2);
        }

        #endregion
    }
}