using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.CreateStruture.Constants.ColumnType;

namespace Persistence.Test.Context.Interface
{
    public class DataContextTests
    {
        // Minimal stub for IColumnTypes
        private class FakeColumnTypes : IColumnTypes
        {
            public string Integer => "INTEGER";
            public string Long => "INTEGER";
            public string TypeBool => "INTEGER";
            public string TypeTime => "TEXT";
            public string TypeVar => "TEXT";
            public string TypeVar50 => "TEXT";
            public string TypeVar150 => "TEXT";
            public string TypeVar64 => "TEXT";
            public string TypeBlob => "BLOB";
            public string Strategy => "Sqlite:Autoincrement";
            public object? SqlStrategy => true;
            public string Name => string.Empty;
            public object? Value => null;
        }

        // Helper to build In-Memory options
        private static DbContextOptions<DataContext> CreateOptions() =>
            new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        [Fact]
        public void GivenNewContext_WhenInitialize_ThenReturnsTrue()
        {
            // Arrange
            var options = CreateOptions();
            var ctx = new DataContext(options, new FakeColumnTypes());

            // Act
            var result = ctx.Initialize();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void GivenInitializedContext_WhenQueryUsers_ThenUsersSetIsEmpty()
        {
            // Arrange
            var options = CreateOptions();
            var ctx = new DataContext(options, new FakeColumnTypes());
            ctx.Initialize();

            // Act
            var count = ctx.Users.CountAsync().Result;

            // Assert
            Assert.Equal(0, count);
        }
    }
}
