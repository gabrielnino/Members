using Autodesk.Domain;

namespace Api.Startup
{
    public class Injection : Dependency
    {
        protected static void AddScoped(WebApplicationBuilder builder)
        {
            DataSeeder(builder);
            User(builder);
            Invoice(builder);
            Cache(builder);
            ErrorLog(builder);
            Composition(builder);
        }
    }
}
