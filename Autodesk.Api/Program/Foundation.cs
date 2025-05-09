using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Autodesk.Persistence.Context;
using System.Data;
using System.Text;
using Domain.Settings;

namespace Autodesk.Api.Program
{
    internal class Foundation : Injection
    {
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
                options.UseSqlite(connectionString);
            });
        }

        protected static void AddConfigureSettings(WebApplicationBuilder builder)
        {
            IConfigurationSection jwtConfiguration = builder.Configuration.GetSection(Settings.JWTConfiguration);
            builder.Services.Configure<LoginSettings>(jwtConfiguration);
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
