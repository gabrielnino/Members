namespace Api.Startup
{
    public class Builder : Database
    {
        protected static void ConfigureServices(WebApplicationBuilder builder, string[] args)
        {
            string connectionString = GetConnectionString(builder);
            Database.SetDatabase(builder);
            AddScoped(builder, args);
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
            AddJwtBearer(builder);
            RunErrorStrategy(builder);
        }
    }
}
