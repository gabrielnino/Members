using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    /// <summary>
    /// Handles updating entities in the database.
    /// </summary>
    public abstract class RepositoryUpdate<T>(DbContext context) : EntityChecker<T>(context)
        where T : class, IEntity
    {
        /// <summary>
        /// Marks an entity as modified and saves changes.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>True if the save succeeded; otherwise false.</returns>
        protected async Task<bool> Update(T entity)
        {
            RepositoryHelper.ValidateArgument(entity);
            _context.Entry(entity).State = EntityState.Modified;
            var result = await SaveChangesAsync();
            return result > 0;
        }
    }
}
