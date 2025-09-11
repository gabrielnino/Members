using Autodesk.Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Implementation;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Persistence.Repositories;

namespace Persistence.Test.Repositories
{
    public class ReadTests
    {
        private class TestReadRepository : Repository<User>
        {
            public TestReadRepository(IUnitOfWork unitOfWork) : base(unitOfWork)
            {
            }

        }

        private static DbContextOptions<DbContext> CreateOptions() =>
            new DbContextOptionsBuilder<DbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

        private TestReadRepository CreateRepository(params User[] items)
        {
            var options = CreateOptions();
            var context = new DataContext(options, new SQLite());
            var unitOfWork = new UnitOfWork(context);
            return new TestReadRepository(unitOfWork);
        }
    }
}
