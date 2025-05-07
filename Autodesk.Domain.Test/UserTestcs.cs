namespace Autodesk.Domain.Test
{
    /// <summary>
    /// Unit tests for the <see cref="User"/> entity.
    /// </summary>
    public class UserTests
    {
        [Fact]
        public void GivenNewUser_WhenInstantiated_ThenIsInactiveByDefault()
        {
            // Given: a new User with a valid ID
            var sut = new User(Guid.NewGuid().ToString());

            // When: we inspect its Active property
            var isActive = sut.Active;

            // Then: it should be false by default
            Assert.False(isActive);
        }

        [Fact]
        public void GivenUser_WhenActivated_ThenActiveIsTrue()
        {
            // Given: a new User
            var sut = new User(Guid.NewGuid().ToString());

            // When: we set Active = true
            sut.Active = true;

            // Then: Active should return true
            Assert.True(sut.Active);
        }

        [Fact]
        public void GivenActiveUser_WhenDeactivated_ThenActiveIsFalse()
        {
            // Given: a User that is currently active
            var sut = new User(Guid.NewGuid().ToString()) { Active = true };

            // When: we set Active = false
            sut.Active = false;

            // Then: Active should return false
            Assert.False(sut.Active);
        }

        [Fact]
        public void GivenValidId_WhenConstructingUser_ThenIdIsSetCorrectly()
        {
            // Given: a specific, non-null, non-empty ID
            var expectedId = Guid.NewGuid().ToString();

            // When: we create a User with that ID
            var sut = new User(expectedId);

            // Then: its Id property should match the provided ID
            Assert.Equal(expectedId, sut.Id);
        }

        [Fact]
        public void GivenNullId_WhenConstructingUser_ThenThrowsArgumentNullException()
        {
            // Given: a null ID
            string? invalidId = null;

            // When
            
            //Then: constructing a User should throw ArgumentNullException
            Assert.Throws<ArgumentNullException>(() => new User(invalidId!));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void GivenEmptyOrWhitespaceId_WhenConstructingUser_ThenThrowsArgumentException(string invalidId)
        {
            // Given: an empty or whitespace-only ID

            // When
            
            //Then: constructing a User should throw ArgumentException
            Assert.Throws<ArgumentException>(() => new User(invalidId));
        }
    }
}
