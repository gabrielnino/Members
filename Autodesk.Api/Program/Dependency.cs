using Application.Result;
using Application.UseCases.Repository.CRUD;
using Application.UseCases.Repository.CRUD.Query;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Domain;
using Autodesk.Infrastructure.Implementation.CRUD.User.Create;
using Autodesk.Infrastructure.Implementation.CRUD.User.Delete;
using Autodesk.Infrastructure.Implementation.CRUD.User.Query.ReadFilter;
using Autodesk.Infrastructure.Implementation.CRUD.User.Update;
using Infrastructure.Repositories.Abstract.CRUD.Util;
using Infrastructure.Result;
using System.Data;
using static System.Net.Mime.MediaTypeNames;

namespace Autodesk.Api.Program
{
    public class Dependency
    {
        protected static void Util(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUtilEntity<User>, UtilEntity<User>>();
        }

        protected static void User(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserReadFilterCursor, UserReadFilterCursor>();
            builder.Services.AddScoped<IUserCreate, UserCreate>();
            builder.Services.AddScoped<IUserUpdate, UserUpdate>();
            builder.Services.AddScoped<IUserDelete, UserDelete>();

        }

        protected static void DataSeeder(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IErrorStrategyHandler, ErrorStrategyHandler>();
        }
    }
}
