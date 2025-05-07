using Persistence.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Test.Repositories
{
    public class RepositoryHelperTests
    {
        [Fact]
        public void ValidateArgument_WithNonNullReference_ReturnsSameInstance()
        {
            // Arrange
            var input = new object();

            // Act
            var result = RepositoryHelper.ValidateArgument(input);

            // Assert
            Assert.Same(input, result);
        }

        [Fact]
        public void ValidateArgument_WithNullReference_ThrowsArgumentNullException()
        {
            // Arrange
            object? input = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RepositoryHelper.ValidateArgument(input!));
        }

        [Fact]
        public void ValidateArgument_WithNonNullValueType_ReturnsSameValue()
        {
            // Arrange
            int input = 42;

            // Act
            var result = RepositoryHelper.ValidateArgument(input);

            // Assert
            Assert.Equal(input, result);
        }

        [Fact]
        public void ValidateArgument_WithNullableValueTypeNull_ThrowsArgumentNullException()
        {
            // Arrange
            int? input = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => RepositoryHelper.ValidateArgument(input!));
        }
    }
}
