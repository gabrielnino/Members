using Autodesk.Domain;
using Autodesk.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;
using Persistence.CreateStruture.Constants.ColumnType.Database;
using Persistence.Repositories;

namespace Persistence.Test.Repositories
{
    public class RepositoryTests
    {

        // Concrete repo for testing
        private class TestRepository : Read<User>
        {
            public TestRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
            {
            }
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
            var unitOfWork = new UnitOfWork(context);
            return new TestRepository(unitOfWork);
        }
    }
}
