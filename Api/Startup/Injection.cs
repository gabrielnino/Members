using Autodesk.Domain;

namespace Api.Startup
{
    public class Injection : Dependency
    {
        protected static void AddScoped(WebApplicationBuilder builder)
        {
            DataSeeder(builder);
            DataBase(builder);
            User(builder);
            Profile(builder);
            Invoice(builder);
            Cache(builder);
            ErrorLog(builder);
            Configuration(builder);
            Composition(builder);
        }
    }
}
