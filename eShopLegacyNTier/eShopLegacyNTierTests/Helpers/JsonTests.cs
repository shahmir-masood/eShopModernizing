using System;
using System.Threading.Tasks;
using eShop.UWP.Helpers;
using Xunit;

namespace eShopLegacyNTierTests.Helpers
{
    public class JsonTests
    {
        [Fact]
        public async Task ToObjectAsync_ValidJson_ReturnsObject()
        {
            // Arrange
            var jsonString = "{\"Id\":1,\"Name\":\"Test\"}";

            // Act
            var result = await Json.ToObjectAsync<TestObject>(jsonString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
            Assert.Equal("Test", result.Name);
        }

        [Fact]
        public async Task ToObjectAsync_InvalidJson_ThrowsException()
        {
            // Arrange
            var jsonString = "{invalid json}";

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => Json.ToObjectAsync<TestObject>(jsonString));
        }

        [Fact]
        public async Task ToObjectAsync_NullString_ThrowsException()
        {
            // Arrange
            string jsonString = null;

            // Act & Assert
            await Assert.ThrowsAnyAsync<Exception>(() => Json.ToObjectAsync<TestObject>(jsonString));
        }

        [Fact]
        public async Task ToObjectAsync_EmptyString_ReturnsNull()
        {
            // Arrange
            var jsonString = "";

            // Act
            var result = await Json.ToObjectAsync<TestObject>(jsonString);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task StringifyAsync_ValidObject_ReturnsJson()
        {
            // Arrange
            var testObject = new TestObject { Id = 1, Name = "Test" };

            // Act
            var result = await Json.StringifyAsync(testObject);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\"Id\":1", result);
            Assert.Contains("\"Name\":\"Test\"", result);
        }

        [Fact]
        public async Task StringifyAsync_NullObject_ReturnsNullJson()
        {
            // Arrange
            TestObject testObject = null;

            // Act
            var result = await Json.StringifyAsync(testObject);

            // Assert
            Assert.Equal("null", result);
        }

        [Fact]
        public async Task StringifyAsync_ComplexObject_ReturnsJson()
        {
            // Arrange
            var testObject = new TestObject
            {
                Id = 42,
                Name = "Complex Object with special characters: !@#$%^&*()",
                Value = 123.45m
            };

            // Act
            var result = await Json.StringifyAsync(testObject);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("\"Id\":42", result);
            Assert.Contains("\"Name\"", result);
            Assert.Contains("\"Value\":123.45", result);
        }

        [Fact]
        public async Task StringifyToObject_RoundTrip_PreservesData()
        {
            // Arrange
            var originalObject = new TestObject
            {
                Id = 99,
                Name = "RoundTrip Test",
                Value = 999.99m
            };

            // Act
            var jsonString = await Json.StringifyAsync(originalObject);
            var deserializedObject = await Json.ToObjectAsync<TestObject>(jsonString);

            // Assert
            Assert.Equal(originalObject.Id, deserializedObject.Id);
            Assert.Equal(originalObject.Name, deserializedObject.Name);
            Assert.Equal(originalObject.Value, deserializedObject.Value);
        }

        [Fact]
        public async Task ToObjectAsync_ArrayOfObjects_ReturnsArray()
        {
            // Arrange
            var jsonString = "[{\"Id\":1,\"Name\":\"Test1\"},{\"Id\":2,\"Name\":\"Test2\"}]";

            // Act
            var result = await Json.ToObjectAsync<TestObject[]>(jsonString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Length);
            Assert.Equal(1, result[0].Id);
            Assert.Equal(2, result[1].Id);
        }

        [Fact]
        public async Task StringifyAsync_ArrayOfObjects_ReturnsJsonArray()
        {
            // Arrange
            var testObjects = new[]
            {
                new TestObject { Id = 1, Name = "Test1" },
                new TestObject { Id = 2, Name = "Test2" }
            };

            // Act
            var result = await Json.StringifyAsync(testObjects);

            // Assert
            Assert.NotNull(result);
            Assert.StartsWith("[", result);
            Assert.EndsWith("]", result);
            Assert.Contains("\"Id\":1", result);
            Assert.Contains("\"Id\":2", result);
        }

        // Helper class for testing
        private class TestObject
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Value { get; set; }
        }
    }
}