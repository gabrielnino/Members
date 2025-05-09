namespace Autodesk.Api.Program
{
    internal class Services : Database
    {
        protected static void ConfigureServices(WebApplicationBuilder builder)
        {
            var provider = builder.Configuration.GetValue<string>("DatabaseProvider");
            if (provider == null)
            {
                return;
            }

            string connectionString = GetConnectionString(builder);
            Database.SetDatabase(builder);
            AddScoped(builder);
            builder.Services.AddLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddConsole();
                loggingBuilder.AddDebug();
            });
            builder.Services.AddHttpClient();
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDistributedMemoryCache();
            AddConfigureSettings(builder);
            AddJwtBearer(builder);
        }
    }
}
