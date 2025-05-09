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
    }
}
