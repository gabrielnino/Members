using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    /// <summary>
    /// Base class to delete entities from the database.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    public abstract class RepositoryDelete<T>(DbContext context) : EntityChecker<T>(context)
        where T : class, IEntity
    {
        /// <summary>
        /// Remove the given entity and save changes.
        /// </summary>
        /// <param name="entity">The item to delete (must not be null).</param>
        /// <returns>
        /// True if the delete was successful; otherwise false.
        /// </returns>
        protected async Task<bool> Delete(T entity)
        {
            RepositoryHelper.ValidateArgument(entity);
            _dbSet.Remove(entity);
            var result = await SaveChangesAsync();
            return result > 0;
        }
    }
}
