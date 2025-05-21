using Persistence.Context.Implementation;

namespace Autodesk.Api.Startup
{


    public class Program : Services
    {
        private static void Main(string[] args)
        {
            WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            ConfigureServices(builder);
            WebApplication app = builder.Build();
            ConfigureMiddleware(app);
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<DataContext>();
                if (!db.Initialize())
                {
                    throw new Exception("Database initialization failed");
                }
            }
            var services = builder.Services;
            app.Run();
        }
    }
}