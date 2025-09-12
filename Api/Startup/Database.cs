using Microsoft.AspNetCore.Builder;
using Persistence.CreateStructure.Constants.ColumnType;
using Persistence.CreateStructure.Constants.ColumnType.Database;

namespace Api.Startup
{
    public class Database : Middleware
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
