using Persistence.Repositories.Interface;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Persistence.Repositories
{
    /// <summary>
    /// Base class for read operations on <typeparamref name="T"/> entities.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    /// <param name="context">EF Core context.</param>
    public abstract class Read<T>(DbContext context) : IRead<T> where T : class
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
        public Task<IQueryable<T>> ReadFilter(Expression<Func<T, bool>> predicate)
        {
            RepositoryHelper.ValidateArgument(predicate);
            return Task.FromResult(_dbSet.Where(predicate));
        }

        /// <summary>
        /// Count entities matching <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>
        /// <returns>Match count.</returns>
        public Task<int> ReadCountFilter(Expression<Func<T, bool>> predicate)
        {
            RepositoryHelper.ValidateArgument(predicate);
            return Task.FromResult(_dbSet.Count(predicate));
        }

        /// <summary>
        /// Get a page of entities matching <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>
        /// <param name="pageNumber">Zero-based page index.</param>
        /// <param name="pageSize">Items per page.</param>
        /// <returns>Paged entities.</returns>
        public Task<IQueryable<T>> ReadPageByFilter(
            Expression<Func<T, bool>> predicate,
            int pageNumber,
            int pageSize)
        {
            RepositoryHelper.ValidateArgument(predicate);
            int skip = pageNumber * pageSize;
            return Task.FromResult(_dbSet.Where(predicate).Skip(skip).Take(pageSize));
        }
    }
}
