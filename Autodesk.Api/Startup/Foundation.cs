using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Autodesk.Persistence.Context;
using System.Text;
using Application.Result;
using Autodesk.Persistence.Context.Interceptors;

namespace Autodesk.Api.Startup
{
    public class Foundation : Injection
    {
        protected static void RunErrorStrategy(WebApplicationBuilder builder)
        {
            using IServiceScope scope = builder.Services.BuildServiceProvider().CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<IErrorStrategyHandler>();
            if (!context.Any())
            {
                var errorStrategyHandler = scope.ServiceProvider.GetRequiredService<IErrorStrategyHandler>();
                errorStrategyHandler.LoadErrorMappings("ErrorMappings.json");
            }
        }

        protected static void AddJwtBearer(WebApplicationBuilder builder)
        {
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
            {
                var jwtKey = builder?.Configuration[Settings.JwtKey] ?? string.Empty;
                byte[] key = Encoding.UTF8.GetBytes(jwtKey);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        protected static void AddDbContextSQLite(WebApplicationBuilder builder, string? connectionString)
        {
            connectionString = ValidateArgument(connectionString);
            builder.Services.AddDbContext<DataContext>(options =>
            {
                options.UseSqlite(connectionString, b => b.MigrationsAssembly("Autodesk.Api"))
                .AddInterceptors(new SqliteFunctionInterceptor());
            });
        }

        private static string ValidateArgument(string? argument)
        {
            if (string.IsNullOrEmpty(argument) || string.IsNullOrWhiteSpace(argument))
            {
                throw new ArgumentNullException(nameof(argument));
            }

            return argument ?? string.Empty;
        }
    }
}
