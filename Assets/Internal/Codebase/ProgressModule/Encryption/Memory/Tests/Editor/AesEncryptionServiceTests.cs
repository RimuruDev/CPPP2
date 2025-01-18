using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Internal.Tests
{
    [TestFixture]
    public class AesEncryptionServiceTests
    {
        [SetUp]
        public void SetUp()
        {
            AesEncryptionService.GenerateSessionKey();
        }
        
        #region Примитивы
        
        [Test]
        public void EncryptDecryptInt_WithAes_ShouldReturnOriginalValue()
        {
            var originalValue = 123456;
            var encryptedValue = AesEncryptionService.EncryptIntWithAes(originalValue);
            var decryptedValue = AesEncryptionService.DecryptIntWithAes(encryptedValue);

            Assert.AreEqual(originalValue, decryptedValue);
        }

        [Test]
        public void EncryptDecryptFloat_WithAes_ShouldReturnOriginalValue()
        {
            var originalValue = 123.456f;
            var encryptedValue = AesEncryptionService.EncryptFloatWithAes(originalValue);
            var decryptedValue = AesEncryptionService.DecryptFloatWithAes(encryptedValue);

            Assert.AreEqual(originalValue, decryptedValue, 0.0001f);
        }

        [Test]
        [SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalse")]
        public void EncryptDecryptBool_WithAes_ShouldReturnOriginalValue()
        {
            var originalValue = true;
            var encryptedValue = AesEncryptionService.EncryptBoolWithAes(originalValue);
            var decryptedValue = AesEncryptionService.DecryptBoolWithAes(encryptedValue);

            Assert.AreEqual(originalValue, decryptedValue);
        }

        #endregion

        #region Коллекции

        [Test]
        public void EncryptDecryptList_WithAes_ShouldReturnOriginalList()
        {
            var originalList = new List<int> { 1, 2, 3, 4, 5 };
            var encryptedList = AesEncryptionService.EncryptListWithAes(originalList);
            var decryptedList = AesEncryptionService.DecryptListWithAes(encryptedList);

            Assert.AreEqual(originalList.Count, decryptedList.Count);
           
            for (var i = 0; i < originalList.Count; i++)
            {
                Assert.AreEqual(originalList[i], decryptedList[i]);
            }
        }

        #endregion

        #region Строки

        [Test]
        public void EncryptDecryptString_WithAes_ShouldReturnOriginalString()
        {
            var originalValue = "Test String";
            var encryptedValue = AesEncryptionService.EncryptStringWithAes(originalValue);
            var decryptedValue = AesEncryptionService.DecryptStringWithAes(encryptedValue);

            Assert.AreEqual(originalValue, decryptedValue);
        }

        #endregion

        #region Объекты

        [Serializable]
        public class Person
        {
            public int Id;
            public string Name;
        }

        [Test]
        public void EncryptDecryptObject_WithAes_ShouldReturnOriginalObject()
        {
            var originalObject = new Person { Id = 1, Name = "John Doe" };

            var encryptedObject = AesEncryptionService.EncryptObjectWithAes(originalObject);
            var decryptedObject = (Person)AesEncryptionService.DecryptObjectWithAes(encryptedObject);

            Assert.AreEqual(originalObject.Id, decryptedObject.Id);
            Assert.AreEqual(originalObject.Name, decryptedObject.Name);
        }

        #endregion
    }
}
