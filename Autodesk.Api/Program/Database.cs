using Microsoft.AspNetCore.Http;
using Persistence.CreateStruture.Constants.ColumnType;
using Persistence.CreateStruture.Constants.ColumnType.Database;

namespace Autodesk.Api.Program
{
    internal class Database : Middleware
    {
        protected static void SetDatabase(WebApplicationBuilder builder)
        {
            builder.Services.AddScoped<IColumnTypes, SQLite>();
            AddDbContextSQLite(builder, GetConnectionString(builder));
        }

        protected static string GetConnectionString(WebApplicationBuilder builder)
        {
            var section = builder.Configuration.GetSection("ConnectionStrings");
            var connetionString = section[Settings.SQLite]  ??string.Empty;
            return connetionString; 
        }
    }
}
