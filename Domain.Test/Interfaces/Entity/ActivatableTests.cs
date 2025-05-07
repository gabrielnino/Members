using Domain.Interfaces.Entity;

namespace Domain.Test.Interfaces.Entity
{
    public class ActivatableTests
    {
        private class TestActivatable : IActivatable
        {
            public bool Active { get; set; }
        }

        [Fact]
        public void GivenNewActivatable_WhenChecked_ThenIsInactiveByDefault()
        {
            // Given: a fresh IActivatable implementation
            var sut = new TestActivatable();

            // When: we read its Active property
            var isActive = sut.Active;

            // Then: it should default to false
            Assert.False(isActive);
        }

        [Fact]
        public void GivenActivatable_WhenSetActive_ThenActiveIsTrue()
        {
            // Given: a fresh IActivatable implementation
            var sut = new TestActivatable();

            // When: we set Active = true
            sut.Active = true;

            // Then: the getter should return true
            Assert.True(sut.Active);
        }

        [Fact]
        public void GivenActiveActivatable_WhenSetInactive_ThenActiveIsFalse()
        {
            // Given: an IActivatable that’s already active
            var sut = new TestActivatable { Active = true };

            // When: we set Active = false
            sut.Active = false;

            // Then: the getter should return false
            Assert.False(sut.Active);
        }
    }
}
