using System;
using System.Threading.Tasks;
using Autodesk.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Context.Implementation;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Persistence.Context.Interface;
using Xunit;

namespace Persistence.Context.Implementation.Tests
{
    public class UnitOfWorkTests : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly DataContext _context;
        private readonly UnitOfWork _uow;

        public UnitOfWorkTests()
        {
            // Use a real SQLite in-memory database to support transactions
            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<DataContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new DataContext(options, new SQLite());
            // Ensure the schema is created
            _context.Database.EnsureCreated();

            _uow = new UnitOfWork(_context);
        }

        public void Dispose()
        {
            _uow.Dispose();
            _connection.Close();
        }

        [Fact]
        public async Task CommitAsync_SavesChanges()
        {
            // Arrange
            var user = new User(Guid.NewGuid().ToString())
            {
                Name     = "Test",
                Lastname = "User",
                Email    = "test.user@example.com"
            };
            _context.Set<User>().Add(user);

            // Act
            var savedCount = await _uow.CommitAsync();

            // Assert: one row affected, and the user exists
            Assert.Equal(1, savedCount);
            var fetched = await _context.Set<User>().FindAsync(user.Id);
            Assert.NotNull(fetched);
            Assert.Equal("Test", fetched.Name);
        }

        [Fact]
        public async Task BeginTransaction_CommitTransaction_PersistsWithinTransaction()
        {
            // Arrange
            using var tx = await _uow.BeginTransactionAsync();

            var user = new User(Guid.NewGuid().ToString())
            {
                Name     = "Trans",
                Lastname = "Commit",
                Email    = "trans.commit@example.com"
            };
            _context.Set<User>().Add(user);

            // Act
            await _uow.CommitTransactionAsync(tx);

            // Assert: after commit, the user is in the database
            var fetched = await _context.Set<User>().FindAsync(user.Id);
            Assert.NotNull(fetched);
            Assert.Equal("Trans", fetched.Name);
        }



        [Fact]
        public void Dispose_DisposesContext()
        {
            // Arrange
            _uow.Dispose();

            // Act & Assert: any operation on context should throw
            Assert.Throws<ObjectDisposedException>(() => _ = _context.Set<User>());
        }
    }
}
