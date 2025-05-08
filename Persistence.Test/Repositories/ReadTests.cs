using Autodesk.Domain;
using Autodesk.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStruture.Constants.ColumnType.Database;
using Persistence.Repositories;

namespace Persistence.Test.Repositories
{
    public class ReadTests
    {
        private class TestReadRepository : Read<User>
        {
            public TestReadRepository(DbContext context) : base(context) { }
        }

        private static DbContextOptions<DbContext> CreateOptions() =>
            new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        private TestReadRepository CreateRepository(params User[] items)
        {
            var options = CreateOptions();
            var context = new DataContext(options, new SQLite());
            // Seed data
            context.Set<User>().AddRange(items);
            context.SaveChanges();
            return new TestReadRepository(context);
        }

        [Fact]
        public async Task ReadFilter_ReturnsOnlyMatchingItems()
        {
            // Arrange
            var a = new User("1") { Name = "Alice", Lastname="Godlord", Email = "alice@email.com" };
            var b = new User("2") { Name = "Bob", Lastname="Rogers", Email = "bob@email.com" };
            var repo = CreateRepository(a, b);

            // Act
            var result = await repo.ReadFilter(e => e.Name.StartsWith("A"));
            var list = result.ToList();

            // Assert
            Assert.Single(list);
            Assert.Equal("Alice", list[0].Name);
        }

        [Fact]
        public async Task ReadCountFilter_ReturnsCorrectCount()
        {
            // Arrange
            var items = new[]
            {
                new User("1") { Name = "XXX", Lastname = "ZZZ", Email="XXX@email.com" },
                new User("2") { Name = "YYY", Lastname = "ZZZ", Email="YYY@email.com" },
                new User("3") { Name = "XXX", Lastname = "ZZZ", Email="XXX@email.com" }
            };
            var repo = CreateRepository(items);

            // Act
            var countX = await repo.ReadCountFilter(e => e.Name == "XXX");
            var countY = await repo.ReadCountFilter(e => e.Name == "YYY");

            // Assert
            Assert.Equal(2, countX);
            Assert.Equal(1, countY);
        }

        [Fact]
        public async Task ReadPageByFilter_ReturnsCorrectPage()
        {
            // Arrange
            var items = Enumerable.Range(1, 10)
                                  .Select(i => new User(i.ToString()) { Name = $"Name{i}", Lastname=$"Lastname{i}", Email=$"email{i}@eamil.com" })
                                  .ToArray();
            var repo = CreateRepository(items);

            // Act: page size 3, get page 2 (zero-based → items 7,8,9)
            var page = await repo.ReadPageByFilter(e => int.Parse(e.Id) > 0, pageNumber: 2, pageSize: 3);
            var list = page.ToList();

            var listNotNull = list ?? [];
            // Assert
            Assert.Equal(3, actual: listNotNull.Count);
            Assert.Equal("7", listNotNull[0].Id);
            Assert.Equal("8", listNotNull[1].Id);
            Assert.Equal("9", listNotNull[2].Id);
        }
    }
}
