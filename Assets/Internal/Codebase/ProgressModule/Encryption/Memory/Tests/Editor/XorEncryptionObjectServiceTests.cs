using NUnit.Framework;
using System;

namespace Internal.Tests
{
    [TestFixture]
    public class XorEncryptionObjectServiceTests
    {
        [Serializable]
        public class MyClass
        {
            public int Id;
            public string Name;
            public float Value;
        }

        #region Примитивы

        [Test]
        public void SerializeObject_ShouldReturnCorrectByteArray()
        {
            var originalObject = new MyClass { Id = 1, Name = "Test", Value = 123.45f };
            var serializedObject = XorEncryptionObjectService.SerializeObject(originalObject);

            Assert.IsNotNull(serializedObject);
            Assert.AreNotEqual(0, serializedObject.Length);
        }

        [Test]
        public void DeserializeObject_ShouldReturnCorrectObject()
        {
            var originalObject = new MyClass { Id = 1, Name = "Test", Value = 123.45f };
            var serializedObject = XorEncryptionObjectService.SerializeObject(originalObject);

            var deserializedObject = (MyClass)XorEncryptionObjectService.DeserializeObject(serializedObject);

            Assert.AreEqual(originalObject.Id, deserializedObject.Id);
            Assert.AreEqual(originalObject.Name, deserializedObject.Name);
            Assert.AreEqual(originalObject.Value, deserializedObject.Value);
        }

        #endregion

        #region Шифрование и дешифрование объектов с использованием XOR

        [Test]
        public void EncryptDecryptObject_WithXor_ShouldReturnOriginalObject()
        {
            var originalObject = new MyClass { Id = 1, Name = "Test", Value = 123.45f };

            // Шифруем объект //
            var encryptedObject = XorEncryptionObjectService.EncryptObjectWithXor(originalObject);

            // Дешифруем объект //
            var decryptedObject = (MyClass)XorEncryptionObjectService.DecryptObjectWithXor(encryptedObject);

            // Проверяем, что оригинальный объект и дешифрованный совпадают //
            Assert.AreEqual(originalObject.Id, decryptedObject.Id);
            Assert.AreEqual(originalObject.Name, decryptedObject.Name);
            Assert.AreEqual(originalObject.Value, decryptedObject.Value);
        }

        #endregion

        #region Шифрование и дешифрование байтовых данных с использованием XOR

        [Test]
        public void EncryptDecryptByteArray_WithXor_ShouldReturnOriginalArray()
        {
            var originalData = new byte[] { 1, 2, 3, 4, 5 };

            // Шифруем данные
            var encryptedData = XorEncryptionObjectService.EncryptWithXor(originalData);

            // Дешифруем данные
            var decryptedData = XorEncryptionObjectService.DecryptWithXor(encryptedData);

            // Проверяем, что оригинальные данные и дешифрованные совпадают //
            Assert.AreEqual(originalData.Length, decryptedData.Length);

            for (var i = 0; i < originalData.Length; i++)
            {
                Assert.AreEqual(originalData[i], decryptedData[i]);
            }
        }

        #endregion

        #region Тесты симметричности XOR

        [Test]
        public void EncryptDecryptString_WithXor_ShouldReturnOriginalString()
        {
            var originalValue = "Test String";

            // Шифруем строку
            var encryptedValue = XorEncryptionService.EncryptWithXor(originalValue);

            // Дешифруем строку
            var decryptedValue = XorEncryptionService.DecryptWithXor(encryptedValue);

            // Проверяем, что исходная и расшифрованная строки совпадают
            Assert.AreEqual(originalValue, decryptedValue);
        }

        #endregion
    }
}