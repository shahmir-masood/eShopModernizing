using System;
using System.IO;
using eShopLegacy.Utilities;
using Xunit;

namespace eShopLegacyMVCTests.Utilities
{
    [Serializable]
    public class TestSerializableObject
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Value { get; set; }

        public TestSerializableObject()
        {
        }

        public TestSerializableObject(int id, string name, decimal value)
        {
            Id = id;
            Name = name;
            Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj is TestSerializableObject other)
            {
                return Id == other.Id && Name == other.Name && Value == other.Value;
            }
            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Id.GetHashCode();
                hash = hash * 23 + (Name?.GetHashCode() ?? 0);
                hash = hash * 23 + Value.GetHashCode();
                return hash;
            }
        }
    }

    public class SerializingTests
    {
        [Fact]
        public void SerializeBinary_ValidObject_ReturnsStream()
        {
            // Arrange
            var serializer = new Serializing();
            var testObject = new TestSerializableObject(1, "Test", 10.5m);

            // Act
            using (var result = serializer.SerializeBinary(testObject))
            {
                // Assert
                Assert.NotNull(result);
                Assert.True(result.Length > 0);
                Assert.True(result.CanRead);
                Assert.Equal(0, result.Position);
            }
        }

        [Fact]
        public void SerializeBinary_NullObject_ThrowsException()
        {
            // Arrange
            var serializer = new Serializing();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => serializer.SerializeBinary(null));
        }

        [Fact]
        public void DeserializeBinary_ValidStream_ReturnsObject()
        {
            // Arrange
            var serializer = new Serializing();
            var testObject = new TestSerializableObject(1, "Test", 10.5m);

            using (var stream = serializer.SerializeBinary(testObject))
            {
                // Act
                var result = serializer.DeserializeBinary(stream);

                // Assert
                Assert.NotNull(result);
                Assert.IsType<TestSerializableObject>(result);
                var deserialized = (TestSerializableObject)result;
                Assert.Equal(testObject.Id, deserialized.Id);
                Assert.Equal(testObject.Name, deserialized.Name);
                Assert.Equal(testObject.Value, deserialized.Value);
            }
        }

        [Fact]
        public void DeserializeBinary_NullStream_ThrowsException()
        {
            // Arrange
            var serializer = new Serializing();

            // Act & Assert
            Assert.Throws<NullReferenceException>(() => serializer.DeserializeBinary(null));
        }

        [Fact]
        public void SerializeDeserialize_RoundTrip_PreservesData()
        {
            // Arrange
            var serializer = new Serializing();
            var originalObject = new TestSerializableObject(42, "RoundTrip Test", 99.99m);

            // Act
            using (var stream = serializer.SerializeBinary(originalObject))
            {
                var deserializedObject = (TestSerializableObject)serializer.DeserializeBinary(stream);

                // Assert
                Assert.Equal(originalObject, deserializedObject);
            }
        }

        [Fact]
        public void SerializeBinary_ComplexObject_ReturnsStream()
        {
            // Arrange
            var serializer = new Serializing();
            var complexObject = new TestSerializableObject(100, "Complex Object with special characters: !@#$%^&*()", 12345.67m);

            // Act
            using (var result = serializer.SerializeBinary(complexObject))
            {
                // Assert
                Assert.NotNull(result);
                Assert.True(result.Length > 0);
            }
        }

        [Fact]
        public void SerializeBinary_EmptyString_ReturnsStream()
        {
            // Arrange
            var serializer = new Serializing();
            var testObject = new TestSerializableObject(1, "", 0m);

            // Act
            using (var result = serializer.SerializeBinary(testObject))
            {
                // Assert
                Assert.NotNull(result);
                Assert.True(result.Length > 0);
            }
        }

        [Fact]
        public void DeserializeBinary_EmptyStream_ThrowsException()
        {
            // Arrange
            var serializer = new Serializing();
            var emptyStream = new MemoryStream();

            // Act & Assert
            Assert.ThrowsAny<Exception>(() => serializer.DeserializeBinary(emptyStream));
        }

        [Fact]
        public void SerializeBinary_MultipleObjects_EachCreatesIndependentStream()
        {
            // Arrange
            var serializer = new Serializing();
            var object1 = new TestSerializableObject(1, "Object 1", 1.0m);
            var object2 = new TestSerializableObject(2, "Object 2", 2.0m);

            // Act
            using (var stream1 = serializer.SerializeBinary(object1))
            using (var stream2 = serializer.SerializeBinary(object2))
            {
                // Assert
                Assert.NotNull(stream1);
                Assert.NotNull(stream2);
                Assert.NotSame(stream1, stream2);
                var roundTrip1 = (TestSerializableObject)serializer.DeserializeBinary(stream1);
                var roundTrip2 = (TestSerializableObject)serializer.DeserializeBinary(stream2);
                Assert.Equal(object1, roundTrip1);
                Assert.Equal(object2, roundTrip2);
            }
        }
    }
}