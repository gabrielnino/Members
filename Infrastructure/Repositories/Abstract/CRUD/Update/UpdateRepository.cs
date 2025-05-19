using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Update
{
    /// <summary>
    /// Base class to update entities with validation and error handling.
    /// </summary>
    public abstract class UpdateRepository<T>(
        IUnitOfWork unitOfWork,
        IErrorHandler errorStrategyHandler,
        IUtilEntity<T> utilEntity
    ) : RepositoryUpdate<T>(unitOfWork), IUpdate<T>
        where T : class, IEntity
    {
        /// <summary>
        /// Update an entity after checking existence and applying custom logic.
        /// </summary>
        /// <param name="entity">The modified entity.</param>
        /// <returns>Result true if updated, otherwise false.</returns>
        public new async Task<Operation<bool>> Update(T entity)
        {
            try
            {
                // Ensure entity exists
                var hasEntity = await utilEntity.HasEntity(entity);
                if (!hasEntity.IsSuccessful)
                    return hasEntity.ConvertTo<bool>();

                // Load original by ID
                var existing = await HasId(entity.Id);
                if (existing is null)
                {
                    var strategy = new BusinessStrategy<bool>();
                    return OperationStrategy<bool>.Fail("Not found", strategy);
                }

                // Apply update-specific logic
                var modifiedResult = await UpdateEntity(entity, existing);
                if (!modifiedResult.IsSuccessful)
                    return modifiedResult.ConvertTo<bool>();

                // Save changes to database
                base.Update(modifiedResult.Data);
                // Build success message
                var template = "UpdateSuccess";
                var message = string.Format(template, typeof(T).Name);
                return Operation<bool>.Success(true, message);
            }
            catch (Exception ex)
            {
                // Handle errors
                return errorStrategyHandler.Fail<bool>(ex);
            }
        }

        /// <summary>
        /// Customize how fields are applied to the original entity.
        /// </summary>
        /// <param name="entityModified">New data values.</param>
        /// <param name="entityUnmodified">Original entity instance.</param>
        /// <returns>Operation with updated entity or error.</returns>
        public virtual async Task<Operation<T>> UpdateEntity(
            T entityModified,
            T entityUnmodified
        )
        {
            var template = "UpdateEntitySearchSuccess";
            var message = string.Format(template, typeof(T).Name);
            return Operation<T>.Success(entityModified, message);
        }
    }
}
