using Domain.Interfaces.Entity;

namespace Persistence.Repositories.Interface
{
    /// <summary>
    /// Defines a contract for be an Repository.
    /// </summary>
    public interface IRepository<T> : IRead<T> where T : class, IEntity
    {
        /// <summary>
        /// Create an entity
        /// </summary>
        /// <param name="entity">The entity to create.</param>
        /// <returns>The task.</returns>
        Task Create(T entity);
        /// <summary>
        /// Update the entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        /// <returns>The task</returns>
        Task Update(T entity);
        /// <summary>
        /// Delete the entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        /// <returns>The task.</returns>
        Task Delete(T entity);
    }
}
