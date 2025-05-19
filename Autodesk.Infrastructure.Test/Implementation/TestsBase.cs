using Autodesk.Domain;
using Autodesk.Infrastructure.Implementation.CRUD.User.Create;
using Autodesk.Infrastructure.Implementation.CRUD.User.Delete;
using Autodesk.Infrastructure.Implementation.CRUD.User.Update;
using Infrastructure.Repositories.Abstract.CRUD.Util;
using Infrastructure.Result;
using Microsoft.EntityFrameworkCore;
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
        protected class UserUtilEntity : UtilEntity<User> { }

        private static DbContextOptions<DataContext> CreateOption() =>
            new DbContextOptionsBuilder<DataContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        protected static DbContextOptions<DataContext> Opts => CreateOption();

        protected static UserUtilEntity Util => new();
        protected static ErrorHandler Errors => new();
        protected DataContext Ctx;

        protected UserCreate RepoCreate;
        protected UserDelete RepoDelete;
        protected UserUpdate RepoUpdate;
        protected UnitOfWork UnitOfWork;
        public TestsBase()
        {
            Errors.LoadErrorMappings(JsonFile);
            Ctx = new DataContext(Opts, new SQLite());
            UnitOfWork = new UnitOfWork(Ctx);  
            RepoCreate = new UserCreate(UnitOfWork);
            RepoDelete = new UserDelete(UnitOfWork, Errors);
            RepoUpdate = new UserUpdate(UnitOfWork, Errors,Util);
        }
    }
}