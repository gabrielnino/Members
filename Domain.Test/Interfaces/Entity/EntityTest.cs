using Domain.Interfaces.Entity;

namespace Domain.Test.Interfaces.Entity
{
    public class EntityTest
    {
        private class TestEntity : IEntity
        {
            public string Id { get; }
            public bool Active { get; set; }
            public TestEntity(string id)
            {
                ArgumentNullException.ThrowIfNull(id);

                if (string.IsNullOrWhiteSpace(id))
                {
                    throw new ArgumentException("Id cannot be empty or whitespace.", nameof(id));
                }

                Id = id;
            }
        }

        [Fact]
        public void GivenNewActivatable_WhenChecked_ThenIsInactiveByDefault()
        {
            // Given: a fresh IActivatable implementation
            var sut = new TestEntity(Guid.NewGuid().ToString());

            // When: we read its Active property
            var isActive = sut.Active;

            // Then: it should default to false
            Assert.False(isActive);
        }

        [Fact]
        public void GivenActivatable_WhenSetActive_ThenActiveIsTrue()
        {
            // Given: a fresh IActivatable implementation
            var sut = new TestEntity(Guid.NewGuid().ToString())
            {
                // When: we set Active = true
                Active = true
            };

            // Then: the getter should return true
            Assert.True(sut.Active);
        }

        [Fact]
        public void GivenActiveActivatable_WhenSetInactive_ThenActiveIsFalse()
        {
            // Given: an IActivatable that’s already active
            var sut = new TestEntity(Guid.NewGuid().ToString()) { Active = true };

            // When: we set Active = false
            sut.Active = false;

            // Then: the getter should return false
            Assert.False(sut.Active);
        }

        [Fact]
        public void GivenIdentifiableWithSpecificId_WhenAccessingId_ThenReturnsThatId()
        {
            // Given: an IIdentifiable implementation with a specific ID
            var expectedId = Guid.NewGuid().ToString();
            var sut = new TestEntity(expectedId);

            // When: we read the ID
            var actualId = sut.Id;

            // Then: it should return the same ID
            Assert.Equal(expectedId, actualId);
        }

        [Fact]
        public void GivenIdentifiableWithNullId_WhenAccessingId_ThenArgumentException()
        {
            // Given: an IIdentifiable implementation with a null ID


            // When: we read the ID
            string expectedId = null;


            // Then: it should return ArgumentException
            Assert.Throws<ArgumentNullException>(() => new TestEntity(expectedId));
        }

        [Fact]
        public void GivenIdentifiableWithEmptyId_WhenAccessingId_ThenReturnsException()
        {
            // Given: an IIdentifiable implementation with an empty ID



            // When: we read the ID
            string expectedId = string.Empty;

            // Then: it should return an empty string
            Assert.Throws<ArgumentException>(() => new TestEntity(expectedId));
        }
    }
}
