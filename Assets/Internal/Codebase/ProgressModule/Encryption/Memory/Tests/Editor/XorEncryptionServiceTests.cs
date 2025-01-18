using NUnit.Framework;
using System.Collections.Generic;

namespace Internal.Tests
{
    [TestFixture]
    public class XorEncryptionServiceTests
    {
        #region Примитивы

        [Test]
        public void EncryptDecryptInt_WithXor_ShouldReturnOriginalValue()
        {
            var originalValue = 123456;
            var encryptedValue = XorEncryptionService.EncryptIntWithXor(originalValue);
            var decryptedValue = XorEncryptionService.DecryptIntWithXor(encryptedValue);

            Assert.AreEqual(originalValue, decryptedValue);
        }

        [Test]
        public void EncryptDecryptFloat_WithXor_ShouldReturnOriginalValue()
        {
            var originalValue = 123.456f;
            var encryptedValue = XorEncryptionService.EncryptFloatWithXor(originalValue);
            var decryptedValue = XorEncryptionService.DecryptFloatWithXor(encryptedValue);

            // Погрешность для float ===
            Assert.AreEqual(originalValue, decryptedValue, 0.0001f); 
        }

        [Test]
        public void EncryptDecryptBool_WithXor_ShouldReturnOriginalValue()
        {
            var originalValue = true;
            var encryptedValue = XorEncryptionService.EncryptBoolWithXor(originalValue);
            var decryptedValue = XorEncryptionService.DecryptBoolWithXor(encryptedValue);

            Assert.AreEqual(originalValue, decryptedValue);
        }

        #endregion

        #region Коллекции

        [Test]
        public void EncryptDecryptList_WithXor_ShouldReturnOriginalList()
        {
            var originalList = new List<int> { 1, 2, 3, 4, 5 };
            var encryptedList = XorEncryptionService.EncryptListWithXor(originalList);
            var decryptedList = XorEncryptionService.DecryptListWithXor(encryptedList);

            Assert.AreEqual(originalList.Count, decryptedList.Count);
            for (int i = 0; i < originalList.Count; i++)
            {
                Assert.AreEqual(originalList[i], decryptedList[i]);
            }
        }

        #endregion

        #region Основные методы шифрования и дешифрования с использованием XOR

        [Test]
        public void EncryptDecryptByteArray_WithXor_ShouldReturnOriginalArray()
        {
            var originalValue = new byte[] { 1, 2, 3, 4, 5 };
            var encryptedValue = XorEncryptionService.EncryptWithXor(originalValue);
            var decryptedValue = XorEncryptionService.DecryptWithXor(encryptedValue);

            Assert.AreEqual(originalValue.Length, decryptedValue.Length);
            for (int i = 0; i < originalValue.Length; i++)
            {
                Assert.AreEqual(originalValue[i], decryptedValue[i]);
            }
        }

        [Test]
        public void EncryptDecryptString_WithXor_ShouldReturnOriginalString()
        {
            var originalValue = "Test String";
            var encryptedValue = XorEncryptionService.EncryptWithXor(originalValue);
            var decryptedValue = XorEncryptionService.DecryptWithXor(encryptedValue);

            Assert.AreEqual(originalValue, decryptedValue);
        }

        #endregion
    }
}