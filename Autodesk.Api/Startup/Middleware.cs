namespace Autodesk.Api.Startup
{
    public class Middleware : Foundation
    {
        protected static void ConfigureMiddleware(WebApplication app)
        {
            app.UseCors(policyBuilder => policyBuilder.AllowAnyHeader().AllowAnyMethod().SetIsOriginAllowed(_ => true).AllowCredentials());
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthentication();
            app.UseAuthorization();
            app.MapControllers();
        }
    }
}
