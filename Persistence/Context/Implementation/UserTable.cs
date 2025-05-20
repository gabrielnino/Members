using Autodesk.Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.CreateStruture.Constants;
using Persistence.CreateStruture.Constants.ColumnType;

namespace Persistence.Context.Implementation
{
    /// <summary>
    /// Represents a Data helper.
    /// </summary>
    public static class UserTable
    {
        /// <summary>
        /// Help to create the Tables.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure entity mappings.</param>
        /// <param name="columnTypes">The column types used to set the type of columns.</param>
        public static void Users(ModelBuilder modelBuilder, IColumnTypes columnTypes)
        {
            modelBuilder.Entity<User>().ToTable(Database.Tables.Users);
            modelBuilder.Entity<User>(entity =>
            {
                entity.Property(u => u.Id).HasColumnType(columnTypes.TypeVar);
                entity.HasKey(u => u.Id);
                entity.Property(u => u.Email).HasColumnType(columnTypes.TypeVar).IsRequired();
                entity.HasIndex(u => u.Email, Database.Index.IndexEmail).IsUnique(true);
                entity.Property(u => u.Active).HasColumnType(columnTypes.TypeBool).IsRequired();
            });
        }
    }
}
