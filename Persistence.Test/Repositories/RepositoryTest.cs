using Autodesk.Domain;
using Autodesk.Persistence.Context;
using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.CreateStruture.Constants.ColumnType.Database;
using Persistence.Repositories;
using System.Xml.Linq;

namespace Persistence.Test.Repositories
{
    public class RepositoryTests
    {

        // Concrete repo for testing
        private class TestRepository(DbContext context) : Repository<User>(context)
        {
        }

        // Create new in-memory options
        private static DbContextOptions<DbContext> CreateOptions() =>
            new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        private static TestRepository CreateRepository(params User[] seed)
        {
            var options = CreateOptions();
            var context = new DataContext(options, new SQLite());
            if (seed?.Any() == true)
            {
                context.Set<User>().AddRange(seed);
                context.SaveChanges();
            }
            return new TestRepository(context);
        }

        [Fact]
        public async Task Create_ShouldAddEntity()
        {
            // Arrange
            var repo = CreateRepository();
            var entity = new User(Guid.NewGuid().ToString()) { Name="Bob", Lastname="Roberts", Email="bob@email.com" , Active = false };

            // Act
            await repo.Create(entity);

            // Assert
            var all = repo.ReadFilter(e => true).Result.ToList();
            Assert.Single(all);
            Assert.Equal(entity.Id, all[0].Id);
        }

        [Fact]
        public async Task Update_ShouldModifyEntity()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var original = new User(id) { Name="Bob", Lastname="Roberts", Email="bob@email.com", Active = false };
            var repo = CreateRepository(original);

            // Act
            original.Active = true;
            await repo.Update(original);

            // Assert
            var updated = repo.ReadFilter(e => e.Id == id).Result.Single();
            Assert.True(updated.Active);
        }

        [Fact]
        public async Task Delete_ShouldRemoveEntity()
        {
            // Arrange
            var id = Guid.NewGuid().ToString();
            var entity = new User (id){ Name="Bob", Lastname="Roberts", Email="bob@email.com", Active = true };
            var repo = CreateRepository(entity);

            // Act
            await repo.Delete(entity);

            // Assert
            var count = await repo.ReadCountFilter(e => e.Id == id);
            Assert.Equal(0, count);
        }
    }
}
