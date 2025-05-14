using Application.Result;
using Domain.Interfaces.Entity;

namespace Application.UseCases.Repository.CRUD
{
    /// <summary>
    /// Adds a new entity and returns whether it succeeded.
    /// </summary>
    /// <typeparam name="T">Entity type.</typeparam>
    public interface ICreate<T> where T : class, IEntity
    {
        /// <summary>
        /// Create the given entity.
        /// </summary>
        /// <param name="entity">The item to add.</param>
        /// <returns>
        /// An operation result with true if created, false otherwise.
        /// </returns>
        Task<Operation<bool>> Create(T entity);
    }
}
