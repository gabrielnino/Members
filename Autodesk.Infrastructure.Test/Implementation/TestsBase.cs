using Application.UseCases.Repository.UseCases.CRUD;
using Autodesk.Domain;
using Autodesk.Infrastructure.Implementation.CRUD.User.Create;
using Autodesk.Infrastructure.Implementation.CRUD.User.Delete;
using Autodesk.Infrastructure.Implementation.CRUD.User.Update;
using Infrastructure.Repositories.CRUD;
using Infrastructure.Result;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Persistence.Context.Implementation;
using Persistence.CreateStruture.Constants.ColumnType.Database;

namespace Autodesk.Infrastructure.Test.Implementation
{
    public class TestsBase
    {
        private const string Folder = "Implementation";
        private const string File = "ErrorMappings.json";
        private static string BaseDirectory => AppContext.BaseDirectory;
        protected static string JsonFile => Path.Combine(BaseDirectory, Folder, File);

        private static DbContextOptions<DataContext> CreateOption() =>
            new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        protected static DbContextOptions<DataContext> Opts => CreateOption();

        protected static ErrorHandler Errors => new();
        protected DataContext Ctx;

        protected UserCreate RepoCreate;
        protected UserDelete RepoDelete;
        protected UserUpdate RepoUpdate;
        protected UnitOfWork UnitOfWork;
        protected ErrorLogCreate ErrorLogCreate;
        protected Mock<IMemoryCache> Cache;
        public TestsBase()
        {
            Errors.LoadErrorMappings(JsonFile);
            Ctx = new DataContext(Opts, new SQLite());
            UnitOfWork = new UnitOfWork(Ctx);
            ErrorLogCreate = new ErrorLogCreate(UnitOfWork);
            Cache = new Mock<IMemoryCache>();
            RepoCreate = new UserCreate(UnitOfWork, Errors, ErrorLogCreate, Cache.Object);
            RepoDelete = new UserDelete(UnitOfWork, Errors, ErrorLogCreate, Cache.Object);
            RepoUpdate = new UserUpdate(UnitOfWork, Errors, ErrorLogCreate, Cache.Object);
        }
    }
}