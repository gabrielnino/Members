using Xunit;
using Autodesk.Shared.Pagination;

namespace Autodesk.Shared.Pagination.Tests
{
    public class PaginationDefaultsTests
    {
        [Fact]
        public void MaxPageSize_Is100()
        {
            Assert.Equal(100, PaginationDefaults.MaxPageSize);
        }

        [Fact]
        public void DefaultPageSize_Is8()
        {
            Assert.Equal(8, PaginationDefaults.DefaultPageSize);
        }
    }
}
