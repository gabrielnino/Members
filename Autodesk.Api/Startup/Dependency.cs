using Application.Result;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Infrastructure.Implementation.CRUD.User.Create;
using Autodesk.Infrastructure.Implementation.CRUD.User.Delete;
using Autodesk.Infrastructure.Implementation.CRUD.User.Update;
using Infrastructure.Result;
using Persistence.Context.Implementation;
using Persistence.Context.Interface;
using Persistence.Repositories;

namespace Autodesk.Api.Startup
{
    public class Dependency
    {
        protected static void Cache(WebApplicationBuilder builder)
        {
            builder.Services.AddMemoryCache();
        }

       protected static void User(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IUserRead, UserRead>();
            builder.Services.AddScoped<IUserCreate, UserCreate>();
            builder.Services.AddScoped<IUserUpdate, UserUpdate>();
            builder.Services.AddScoped<IUserDelete, UserDelete>();

        }

        protected static void DataSeeder(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IErrorHandler, ErrorHandler>();
        }
    }
}
