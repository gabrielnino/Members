using Api.Startup;
using Persistence.Context.Implementation;

namespace Autodesk.Api
{


    public class Program : Builder
    {
        private static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            ConfigureServices(builder, args);
            var app = builder.Build();
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