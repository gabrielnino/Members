using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Create
{
    /// <summary>
    /// Base class to create entities with validation and error handling.
    /// </summary>
    public abstract class CreateRepository<T>(
        DbContext context,
        IUtilEntity<T> utilEntity,
        IErrorStrategyHandler errorStrategyHandler
    ) : RepositoryCreate<T>(context), ICreate<T>
        where T : class, IEntity
    {
        /// <summary>
        /// Create a new entity after checking existence and validation.
        /// </summary>
        public new async Task<Operation<bool>> Create(T entity)
        {
            try
            {
                // Check if entity already exists
                var hasEntity = await utilEntity.HasEntity(entity);
                if (!hasEntity.IsSuccessful)
                    return hasEntity.ConvertTo<bool>();

                // Run custom validation logic
                var validationResult = await CreateEntity(entity);
                if (!validationResult.IsSuccessful)
                    return validationResult.ConvertTo<bool>();

                // (Optional) ensure no duplicate ID
                await base.ReadFilter(e => e.Id == entity.Id);

                // Save entity
                var created = await base.Create(validationResult.Data);

                // Build success message
                var template = await GetCreationSuccess();
                var message = string.Format(template, typeof(T).Name);

                return Operation<bool>.Success(created, message);
            }
            catch (Exception ex)
            {
                // Handle errors centrally
                return errorStrategyHandler.Fail<bool>(ex);
            }
        }

        /// <summary>
        /// Get the template for creation success message.
        /// </summary>
        protected virtual Task<string> GetCreationSuccess()
            => Task.FromResult(CreateLabels.CreationSuccess);

        /// <summary>
        /// Implement entity-specific validation before saving.
        /// </summary>
        protected abstract Task<Operation<T>> CreateEntity(T entity);
    }
}
