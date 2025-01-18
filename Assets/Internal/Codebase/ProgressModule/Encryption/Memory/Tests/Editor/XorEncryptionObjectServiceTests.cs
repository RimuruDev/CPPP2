using System;
using NUnit.Framework;

namespace Internal.Tests
{
    [TestFixture]
    public class XorEncryptionObjectServiceTests
    {
        [Serializable]
        public class Address
        {
            public string Street;
            public string City;
        }

        [Serializable]
        public class Person
        {
            public int Id;
            public string Name;
            public Address Address;
        }

        [Serializable]
        public struct Point
        {
            public int X;
            public int Y;
        }

        #region Примитивы

        [Test]
        public void SerializeObject_ShouldReturnCorrectByteArray()
        {
            var originalObject = new Person
            {
                Id = 1,
                Name = "John",
                Address = new Address
                {
                    Street = "Main St",
                    City = "Metropolis"
                }
            };

            var serializedObject = XorEncryptionService.SerializeObject(originalObject);

            Assert.IsNotNull(serializedObject);
            Assert.AreNotEqual(0, serializedObject.Length);
        }

        [Test]
        public void DeserializeObject_ShouldReturnCorrectObject()
        {
            var originalObject = new Person
            {
                Id = 1,
                Name = "John",
                Address = new Address
                {
                    Street = "Main St",
                    City = "Metropolis"
                }
            };

            var serializedObject = XorEncryptionService.SerializeObject(originalObject);
            var deserializedObject = (Person)XorEncryptionService.DeserializeObject(serializedObject);

            Assert.AreEqual(originalObject.Id, deserializedObject.Id);
            Assert.AreEqual(originalObject.Name, deserializedObject.Name);
            Assert.AreEqual(originalObject.Address.Street, deserializedObject.Address.Street);
            Assert.AreEqual(originalObject.Address.City, deserializedObject.Address.City);
        }

        [Test]
        public void EncryptDecryptObject_WithXor_ShouldReturnOriginalObject()
        {
            var originalObject = new Person
            {
                Id = 1,
                Name = "John",
                Address = new Address
                {
                    Street = "Main St",
                    City = "Metropolis"
                }
            };

            var encryptedObject = XorEncryptionService.EncryptObjectWithXor(originalObject);
            var decryptedObject = (Person)XorEncryptionService.DecryptObjectWithXor(encryptedObject);

            Assert.AreEqual(originalObject.Id, decryptedObject.Id);
            Assert.AreEqual(originalObject.Name, decryptedObject.Name);
            Assert.AreEqual(originalObject.Address.Street, decryptedObject.Address.Street);
            Assert.AreEqual(originalObject.Address.City, decryptedObject.Address.City);
        }

        #endregion

        #region Структуры

        [Test]
        public void SerializeAndDeserializeStruct_ShouldWorkCorrectly()
        {
            var originalStruct = new Point { X = 10, Y = 20 };
            var serializedStruct = XorEncryptionService.SerializeObject(originalStruct);
            var deserializedStruct = (Point)XorEncryptionService.DeserializeObject(serializedStruct);

            Assert.AreEqual(originalStruct.X, deserializedStruct.X);
            Assert.AreEqual(originalStruct.Y, deserializedStruct.Y);
        }

        [Test]
        public void EncryptDecryptStruct_WithXor_ShouldReturnOriginalStruct()
        {
            var originalStruct = new Point { X = 10, Y = 20 };

            var encryptedStruct = XorEncryptionService.EncryptObjectWithXor(originalStruct);
            var decryptedStruct = (Point)XorEncryptionService.DecryptObjectWithXor(encryptedStruct);

            Assert.AreEqual(originalStruct.X, decryptedStruct.X);
            Assert.AreEqual(originalStruct.Y, decryptedStruct.Y);
        }

        #endregion

        #region Вложенные объекты

        [Test]
        public void SerializeAndDeserializeNestedObject_ShouldWorkCorrectly()
        {
            var originalObject = new Person
            {
                Id = 1,
                Name = "John",
                Address = new Address
                {
                    Street = "Main St",
                    City = "Metropolis"
                }
            };

            var serializedObject = XorEncryptionService.SerializeObject(originalObject);
            var deserializedObject = (Person)XorEncryptionService.DeserializeObject(serializedObject);

            Assert.AreEqual(originalObject.Id, deserializedObject.Id);
            Assert.AreEqual(originalObject.Name, deserializedObject.Name);
            Assert.AreEqual(originalObject.Address.Street, deserializedObject.Address.Street);
            Assert.AreEqual(originalObject.Address.City, deserializedObject.Address.City);
        }
        
        [Test]
        public void SerializeAndDeserializeNestedObject_ShouldWorkCorrectly_GenericType()
        {
            var originalObject = new Person
            {
                Id = 1,
                Name = "John",
                Address = new Address
                {
                    Street = "Main St",
                    City = "Metropolis"
                }
            };

            var serializedObject = XorEncryptionService.SerializeObject(originalObject);
            var deserializedObject = XorEncryptionService.DeserializeObject<Person>(serializedObject);

            Assert.AreEqual(originalObject.Id, deserializedObject.Id);
            Assert.AreEqual(originalObject.Name, deserializedObject.Name);
            Assert.AreEqual(originalObject.Address.Street, deserializedObject.Address.Street);
            Assert.AreEqual(originalObject.Address.City, deserializedObject.Address.City);
        }

        #endregion
    }
}