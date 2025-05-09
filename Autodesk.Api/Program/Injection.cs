namespace Autodesk.Api.Program
{
    internal class Injection : Dependency
    {
        protected static void AddScoped(WebApplicationBuilder builder)
        {
            User(builder);
            Util(builder);
        }
    }
}
