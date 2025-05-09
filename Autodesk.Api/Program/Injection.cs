namespace Autodesk.Api.Program
{
    internal class Injection : Dependency
    {
        protected static void AddScoped(WebApplicationBuilder builder)
        {
            DataSeeder(builder);
            User(builder);
            Util(builder);
        }
    }
}
