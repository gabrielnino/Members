using System.Linq.Expressions;

namespace Persistence.Repositories.Interface
{
    /// <summary>
    /// Defines a contract for be an Read.
    /// </summary>
    public interface IRead<T>
           where T : class
    {
        /// <summary>
        /// Retrives all entites that satisfy the filter.
        /// </summary>
        /// <param name="predicate">A lambda expression to filter.</param>
        /// <returns>The entities matching the filter.</returns>
        Task<IQueryable<T>> ReadFilter(Expression<Func<T, bool>> predicate);
        /// <summary>
        /// Retrives the count entites that satisfy the filter.
        /// </summary>
        /// <param name="predicate">A lambda expression to filter.</param>
        /// <returns>The count the filter.</returns>
        Task<int> ReadCountFilter(Expression<Func<T, bool>> predicate);

        /// <summary>
        /// Retrives all entites that satisfy the filter by page.
        /// </summary>
        /// <param name="predicate">A lambda expression to filter.</param>
        /// <param name="pageNumber">The page number.</param>
        /// <param name="pageSize">The size number per page.</param>
        /// <returns></returns>
        Task<IQueryable<T>> ReadPageByFilter(Expression<Func<T, bool>> predicate, int pageNumber, int pageSize);
    }
}
