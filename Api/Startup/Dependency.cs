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
using Commands;
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
        protected static void Cache(IHostApplicationBuilder builder)
        {
            builder.Services.AddMemoryCache();
        }

        protected static void User(IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUserRead, UserRead>();
            builder.Services.AddScoped<IUserCreate, UserCreate>();
            builder.Services.AddScoped<IUserUpdate, UserUpdate>();
            builder.Services.AddScoped<IUserDelete, UserDelete>();
        }

        protected static void DataBase(IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddSingleton<IColumnTypes, SQLite>();
        }

        protected static void Profile(IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<IProfileCreate, ProfileCreate>();
            builder.Services.AddScoped<IProfileRead, ProfileRead>();
            builder.Services.AddScoped<IProfileDelete, ProfileDelete>();
            builder.Services.AddScoped<IProfileUpdate, ProfileUpdate>();
        }

        protected static void Invoice(IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<IInvoiceCreate, InvoiceCreate>();
            builder.Services.AddScoped<IInvoiceRead, InvoiceRead>();
        }

        protected static void ErrorLog(IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<IErrorLogCreate, ErrorLogCreate>();
        }

        //
        protected static void DataSeeder(IHostApplicationBuilder builder)
        {
            builder.Services.AddScoped<IErrorHandler, ErrorHandler>();
        }

        protected static void Composition(IHostApplicationBuilder builder)
        {
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
            builder.Services.AddScoped<IConnectionInfoCollector, ConnectionInfoCollector>();
            builder.Services.AddScoped<ILinkedInChat, LinkedInChat>();
            
        }

        protected static void Commands(IHostApplicationBuilder builder, string[] args)
        {
            builder.Services.AddSingleton(new CommandArgs(args));
            builder.Services.AddSingleton<CommandFactory>();
            builder.Services.AddTransient<HelpCommand>();
            builder.Services.AddTransient<SearchCommand>();
            builder.Services.AddTransient<PromtCommand>();
            builder.Services.AddTransient<InviteCommand>();
            builder.Services.AddTransient<CollectorCommand>();
            builder.Services.AddTransient<ChatCommand>();
        }

        protected static void Configuration(IHostApplicationBuilder builder)
        {

            var appConfig = new AppConfig();
            builder.Configuration.Bind(appConfig);
            builder.Services.AddSingleton<AppConfig>(appConfig);
            ExecutionTracker executionOptions = new(Environment.CurrentDirectory);
            builder.Services.AddSingleton(executionOptions);

        }
    }
}
