using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;

namespace Infrastructure.Repositories.Abstract.CRUD.Util
{
    /// <summary>
    /// Validates that an entity instance is provided.
    /// </summary>
    /// <typeparam name="T">The entity type.</typeparam>
    public class UtilEntity<T> : IUtilEntity<T>
        where T : class, IEntity
    {
        /// <summary>
        /// Returns a failure if the entity is null; otherwise returns success.
        /// </summary>
        /// <param name="entity">The entity to check.</param>
        /// <returns>
        /// Operation with the entity or a business error.
        /// </returns>
        public async Task<Operation<T>> HasEntity(T entity)
        {
            var failMsg = UtilEntityLabels.UtilEntityFailedNecesaryData;
            if (entity is null)
            {
                return OperationStrategy<T>.Fail(failMsg, new BusinessStrategy<T>());
            }

            var successMsg = UtilEntityLabels.UtilEntitySuccess;
            return Operation<T>.Success(entity, successMsg);
        }
    }
}
