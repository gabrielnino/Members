using Application.UseCases.Repository.CRUD;
using Application.UseCases.Repository.CRUD.Query;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Domain;
using Autodesk.Infrastructure.Implementation.CRUD.User.Create;
using Autodesk.Infrastructure.Implementation.CRUD.User.Query.ReadFilter;
using Autodesk.Infrastructure.Implementation.CRUD.User.Query.ReadFilterCount;
using Autodesk.Infrastructure.Implementation.CRUD.User.Query.ReadFilterPage;
using Autodesk.Infrastructure.Implementation.CRUD.User.Query.ReadId;
using Autodesk.Infrastructure.Implementation.CRUD.User.Update;
using Infrastructure.Repositories.Abstract.CRUD.Util;
using static System.Net.Mime.MediaTypeNames;

namespace Autodesk.Api.Program
{
    public class Dependency
    {
        protected static void Util(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IReadFilter<User>, UserReadFilter>();
            builder.Services.AddScoped<IUtilEntity<User>, UtilEntity<User>>();
        }

        protected static void User(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserReadById, UserReadById>();
            builder.Services.AddScoped<IUserReadFilter, UserReadFilter>();
            builder.Services.AddScoped<IUserReadFilterCount, UserReadFilterCount>();
            builder.Services.AddScoped<IUserReadFilterPage, UserReadFilterPage>();
            builder.Services.AddScoped<IUserCreate, UserCreate>();
            builder.Services.AddScoped<IUserUpdate, UserUpdate>();

        }
    }
}
