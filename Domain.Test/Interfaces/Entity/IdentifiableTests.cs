using Domain.Interfaces.Entity;

namespace Domain.Test.Interfaces.Entity
{
    public class IdentifiableTests
    {
        private class TestIdentifiable(string id) : IIdentifiable
        {
            public string Id { get; } = id;
        }

        [Fact]
        public void GivenIdentifiableWithSpecificId_WhenAccessingId_ThenReturnsThatId()
        {
            // Given: an IIdentifiable implementation with a specific ID
            var expectedId = Guid.NewGuid().ToString();
            var sut = new TestIdentifiable(expectedId);

            // When: we read the ID
            var actualId = sut.Id;

            // Then: it should return the same ID
            Assert.Equal(expectedId, actualId);
        }

        [Fact]
        public void GivenIdentifiableWithNullId_WhenAccessingId_ThenReturnsNull()
        {
            // Given: an IIdentifiable implementation with a null ID
            string expectedId = null;
            var sut = new TestIdentifiable(expectedId);

            // When: we read the ID
            var actualId = sut.Id;

            // Then: it should return null
            Assert.Null(actualId);
        }

        [Fact]
        public void GivenIdentifiableWithEmptyId_WhenAccessingId_ThenReturnsEmptyString()
        {
            // Given: an IIdentifiable implementation with an empty ID
            string expectedId = string.Empty;
            var sut = new TestIdentifiable(expectedId);

            // When: we read the ID
            var actualId = sut.Id;

            // Then: it should return an empty string
            Assert.Equal(string.Empty, actualId);
        }
    }
}
