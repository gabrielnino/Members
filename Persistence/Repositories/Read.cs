using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Persistence.Repositories
{
    /// <summary>
    /// Base class for read operations on <typeparamref name="T"/> entities.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="context">EF Core context.</param>
    public abstract class Read<T>(DbContext context) where T : class
    {
        /// <summary>
        /// Validated EF Core context.
        /// </summary>
        protected readonly DbContext _context = RepositoryHelper.ValidateArgument(context);

        /// <summary>
        /// EF Core set for <typeparamref name="T"/>.
        /// </summary>
        protected readonly DbSet<T> _dbSet = context.Set<T>();

        /// <summary>
        /// Get entities matching <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>
        /// <returns>Matching entities.</returns>
        protected Task<IQueryable<T>> ReadFilter(Expression<Func<T, bool>> predicate)
        {
            RepositoryHelper.ValidateArgument(predicate);
            return Task.FromResult(_dbSet.Where(predicate));
        }
    }
}
