using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Autodesk.Application.UseCases.CRUD.Invoice;
using Autodesk.Application.UseCases.CRUD.Invoice.Query;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Infrastructure.Implementation.CRUD.Invoice.Create;
using Autodesk.Infrastructure.Implementation.CRUD.Invoice.Query.ReadFilter;
using Autodesk.Infrastructure.Implementation.CRUD.User.Create;
using Autodesk.Infrastructure.Implementation.CRUD.User.Delete;
using Autodesk.Infrastructure.Implementation.CRUD.User.Update;
using Configuration;
using Infrastructure.Repositories.CRUD;
using Infrastructure.Result;
using LiveNetwork.Application.Services;
using LiveNetwork.Application.UseCases.CRUD.Profile;
using LiveNetwork.Application.UseCases.CRUD.Profile.Query;
using LiveNetwork.Infrastructure.Implementation.CRUD.Profile.Create;
using LiveNetwork.Infrastructure.Implementation.CRUD.Profile.Delete;
using LiveNetwork.Infrastructure.Implementation.CRUD.Profile.Query.ReadFilter;
using LiveNetwork.Infrastructure.Implementation.CRUD.Profile.Update;
using LiveNetwork.Infrastructure.Services;
using Persistence.Context.Implementation;
using Persistence.Context.Interface;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants.ColumnType.Database;
using Services.Interfaces;

namespace Api.Startup
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

        protected static void Profile(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IProfileCreate, ProfileCreate>();
            builder.Services.AddScoped<IProfileRead, ProfileRead>();
            builder.Services.AddScoped<IProfileDelete, ProfileDelete>();
            builder.Services.AddScoped<IProfileUpdate, ProfileUpdate>();
        }

        protected static void Invoice(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IInvoiceCreate, InvoiceCreate>();
            builder.Services.AddScoped<IInvoiceRead, InvoiceRead>();
        }

        protected static void ErrorLog(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IErrorLogCreate, ErrorLogCreate>();
        }

        //
        protected static void DataSeeder(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IErrorHandler, ErrorHandler>();
        }

        protected static void Composition(WebApplicationBuilder builder)
        {
            AppConfig appConfig = new();
            ExecutionTracker executionOptions = new(Environment.CurrentDirectory);
            builder.Services.AddSingleton(executionOptions);
            builder.Services.AddSingleton(appConfig);
            builder.Services.AddSingleton<ISecurityCheck, SecurityCheck>();
            builder.Services.AddTransient<IPromptGenerator, PromptGenerator>();
            builder.Services.AddTransient<ILoginService, LoginService>();
            builder.Services.AddTransient<ICaptureSnapshot, CaptureSnapshot>();
            builder.Services.AddSingleton<IWebDriverFactory, ChromeDriverFactory>();
            builder.Services.AddTransient<ISearch, Search>();
            builder.Services.AddTransient<IProcessor, Processor>();
            builder.Services.AddSingleton<IDirectoryCheck, DirectoryCheck>();
            builder.Services.AddSingleton<IOpenAIClient, OpenAIClient>();
            builder.Services.AddSingleton<IUtil, Util>();
            builder.Services.AddSingleton<ITrackingService, TrackingService>();
            builder.Services.AddSingleton<ISearchCoordinator, SearchCoordinator>();
            builder.Services.AddSingleton<IResumeDetailService, ResumeDetailService>();
            builder.Services.AddSingleton<IInviteConnections, InviteConnections>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddSingleton<IColumnTypes, SQLite>();
            builder.Services.AddSingleton<IErrorHandler, ErrorHandler>();
            builder.Services.AddScoped<IConnectionInfoCollector, ConnectionInfoCollector>();
        }


    }
}
