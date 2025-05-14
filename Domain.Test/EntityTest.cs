namespace Domain.Test
{
    /// <summary>
    /// Unit tests for the <see cref="Entity"/> entity.
    /// </summary>
    public class EntityTests
    {
        [Fact]
        public void GivenNewEntity_WhenInstantiated_ThenIsInactiveByDefault()
        {
            // Given: a new Entity with a valid ID
            var sut = new Entity(Guid.NewGuid().ToString());

            // When: we inspect its Active property
            var isActive = sut.Active;

            // Then: it should be false by default
            Assert.False(isActive);
        }

        [Fact]
        public void GivenEntity_WhenActivated_ThenActiveIsTrue()
        {
            // Given: a new Entity
            var sut = new Entity(Guid.NewGuid().ToString());

            // When: we set Active = true
            sut.Active = true;

            // Then: Active should return true
            Assert.True(sut.Active);
        }

        [Fact]
        public void GivenActiveEntity_WhenDeactivated_ThenActiveIsFalse()
        {
            // Given: a Entity that is currently active
            var sut = new Entity(Guid.NewGuid().ToString()) { Active = true };

            // When: we set Active = false
            sut.Active = false;

            // Then: Active should return false
            Assert.False(sut.Active);
        }

        [Fact]
        public void GivenValidId_WhenConstructingEntity_ThenIdIsSetCorrectly()
        {
            // Given: a specific, non-null, non-empty ID
            var expectedId = Guid.NewGuid().ToString();

            // When: we create a Entity with that ID
            var sut = new Entity(expectedId);

            // Then: its Id property should match the provided ID
            Assert.Equal(expectedId, sut.Id);
        }

        [Fact]
        public void GivenNullId_WhenConstructingEntity_ThenThrowsArgumentNullException()
        {
            // Given: a null ID
            string? invalidId = null;

            // When

            //Then: constructing a Entity should throw ArgumentNullException
            Assert.Throws<ArgumentNullException>(() => new Entity(invalidId!));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GivenEmptyOrWhitespaceId_WhenConstructingEntity_ThenThrowsArgumentException(string invalidId)
        {
            // Given: an empty or whitespace-only ID

            // When

            //Then: constructing a Entity should throw ArgumentException
            Assert.Throws<ArgumentException>(() => new Entity(invalidId));
        }
    }
}
