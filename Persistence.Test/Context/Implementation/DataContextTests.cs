using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Persistence.Context.Implementation;
using Persistence.CreateStruture.Constants.ColumnType.Database;
using Persistence.Context.Interface;
using Autodesk.Domain;
using Xunit;

namespace Persistence.Context.Implementation.Tests
{
    public class DataContextTests
    {
        private static ModelBuilder CreateModelBuilder()
        {
            var conventions = new ConventionSet();
            return new ModelBuilder(conventions);
        }

        [Fact]
        public void OnModelCreating_ConfiguresAllEntitiesWithCorrectTableNames()
        {
            // Arrange
            var modelBuilder = CreateModelBuilder();
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase("TestDb")  // provider doesn't matter for model
                .Options;
            IDataContext ctx = new DataContext(options, new SQLite());

            // Act
            // Call protected OnModelCreating via reflection
            typeof(DataContext)
                .GetMethod("OnModelCreating", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(ctx, new object[] { modelBuilder });

            var model = modelBuilder.Model;

            // Assert: each entity type exists and has expected table name
            var userEntity = model.FindEntityType(typeof(User));
            var productEntity = model.FindEntityType(typeof(Product));
            var invoiceEntity = model.FindEntityType(typeof(Invoice));
            var errorLogEntity = model.FindEntityType(typeof(Domain.ErrorLog));

            Assert.NotNull(userEntity);
            Assert.Equal("Users", userEntity.GetTableName());

            Assert.NotNull(productEntity);
            Assert.Equal("Products", productEntity.GetTableName());

            Assert.NotNull(invoiceEntity);
            Assert.Equal("Invoices", invoiceEntity.GetTableName());

            Assert.NotNull(errorLogEntity);
            Assert.Equal("ErrorLogs", errorLogEntity.GetTableName());
        }

        [Fact]
        public void OnModelCreating_RegistersStringCompareOrdinalDbFunction()
        {
            // Arrange
            var modelBuilder = CreateModelBuilder();
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase("TestDb2")
                .Options;
            IDataContext ctx = new DataContext(options, new SQLite());

            // Act
            typeof(DataContext)
                .GetMethod("OnModelCreating", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!
                .Invoke(ctx, new object[] { modelBuilder });

            var model = modelBuilder.Model;
            // Look up the DbFunction by name
            var dbFunction = model
                .GetDbFunctions()
                .FirstOrDefault(f => f.Name.Equals("StringCompareOrdinal", StringComparison.Ordinal));

            // Assert
            Assert.NotNull(dbFunction);
           
        }

        [Fact]
        public void Initialize_WithInMemoryProvider_ReturnsFalse()
        {
            // Arrange: InMemory does not support migrations
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseInMemoryDatabase("TestDb3")
                .Options;
            var ctx = new DataContext(options, new SQLite());

            // Act
            var result = ctx.Initialize();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Initialize_WithSqliteInMemoryProvider_ReturnsTrue()
        {
            // Arrange: SQLite in-memory supports migrations
            var connection = new Microsoft.Data.Sqlite.SqliteConnection("Data Source=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(connection)
                .Options;
            var ctx = new DataContext(options, new SQLite());

            // Act
            var result = ctx.Initialize();

            // Assert
            Assert.True(result);

            connection.Close();
        }

        [Fact]
        public void StringCompareOrdinal_AlwaysThrowsNotSupportedException()
        {
            // Act & Assert
            Assert.Throws<NotSupportedException>(() => DataContext.StringCompareOrdinal("a", "b"));
        }
    }
}

