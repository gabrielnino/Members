namespace Autodesk.Api.Startup
{
    public class Injection : Dependency
    {
        protected static void AddScoped(WebApplicationBuilder builder)
        {
            DataSeeder(builder);
            User(builder);
            Cache(builder);
            Util(builder);
        }
    }
}
