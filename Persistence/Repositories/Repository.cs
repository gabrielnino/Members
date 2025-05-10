using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Persistence.Repositories
{
    /// <summary>
    /// Base class for read operations on <typeparamref name="T"/> entities.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="context">EF Core context.</param>
    public abstract class Repository<T>(DbContext context) where T : class
    {
        /// <summary>
        /// Validated EF Core context.
        /// </summary>
        protected readonly DbContext _context = RepositoryHelper.ValidateArgument(context);

        /// <summary>
        /// EF Core set for <typeparamref name="T"/>.
        /// </summary>
        protected readonly DbSet<T> _dbSet = context.Set<T>();

        protected async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
