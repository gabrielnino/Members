using Application.Result;
using Domain.Interfaces.Entity;

namespace Application.UseCases.Repository.CRUD
{
    /// <summary>
    /// Checks if a given entity exists in the data store.
    /// </summary>
    /// <typeparam name="T">Type of the entity.</typeparam>
    public interface IUtilEntity<T> where T : class, IEntity
    {
        /// <summary>
        /// Verifies presence of the specified entity.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns>
        /// An Operation containing the entity when found, or error info.
        /// </returns>
        Task<Operation<T>> HasEntity(T entity);
    }
}
