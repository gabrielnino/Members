using Autodesk.Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;
using Persistence.CreateStruture.Constants.ColumnType;

namespace Autodesk.Persistence.Context
{
    /// <summary>
    /// Represents a DataContentext
    /// </summary>
    /// <param name="options">The options to be used by DbContext</param>
    /// <param name="columnTypes">The column types used for the database</param>
    public class DataContext(DbContextOptions options, IColumnTypes columnTypes) : DbContext(options), IDataContext
    {
        protected readonly IColumnTypes _columnTypes = columnTypes;
        public virtual DbSet<User> Users { get; set; }
        /// <summary>
        ///  Initializes the data context. This typically includes opening connections, applying migrations, creating
        ///  the database if it does not exist, and seeding any required initial data.
        /// </summary>
        /// <returns>
        /// <c>true</c> if initialization succeeded; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Initialize()
        {
            try
            {
                Database.EnsureCreated();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred while initializing the database:");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                return false;
            }
        }


        /// <summary>
        /// Configures the EF Core model for this context.
        /// Sets up tables, keys, indexes, and column mappings using the provided <see cref="IColumnTypes"/>.
        /// </summary>
        /// <param name="modelBuilder">The model builder used to configure entity mappings.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            DataHelper.SetTableUsers(modelBuilder, _columnTypes);
        }
    }
}
