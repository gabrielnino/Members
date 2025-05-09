using Domain.Interfaces.Entity;

namespace Persistence.Repositories.Interface
{
    /// <summary>
    /// Defines a contract for be an Repository.
    /// </summary>
    public interface IRepository<T> where T : class, IEntity
    {
        /// <summary>
        /// Create an entity
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        /// <returns>The task.</returns>
        Task<bool> Create(T entity);
        /// <summary>
        /// Update the entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>The task</returns>
        Task<bool> Update(T entity);
        /// <summary>
        /// Delete the entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>The task.</returns>
        Task<bool> Delete(T entity);
    }
}
