using Autodesk.Domain;
using Autodesk.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStruture.Constants.ColumnType.Database;
using Persistence.Repositories;

namespace Persistence.Test.Repositories
{
    public class RepositoryTests
    {

        // Concrete repo for testing
        private class TestRepository(DbContext context) : Read<User>(context)
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
    }
}
