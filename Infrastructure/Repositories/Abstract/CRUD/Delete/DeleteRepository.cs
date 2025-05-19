using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Delete
{
    /// <summary>
    /// Provides deletion of entities with validation and error handling.
    /// </summary>
    public abstract class DeleteRepository<T>(IUnitOfWork unitOfWork, IErrorHandler errorStrategyHandler)
        : RepositoryDelete<T>(unitOfWork), IDelete<T>
        where T : class, IEntity
    {
        /// <summary>
        /// Deletes the entity with the given ID if it exists.
        /// </summary>
        /// <param name="id">ID of the entity to delete.</param>
        /// <returns>
        /// An operation result: true if deleted, false or error otherwise.
        /// </returns>
        public virtual async Task<Operation<bool>> Delete(string id)
        {
            try
            {
                // Verify the ID and entity exist
                var validationResult = await HasId(id);
                if (validationResult is null)
                {
                    var strategy = new BusinessStrategy<bool>();
                    return OperationStrategy<bool>.Fail(DeleteLabels.EntityNotFound, strategy);
                }

                // Remove the entity
                var entity = RepositoryHelper.ValidateArgument(validationResult);
                Delete(entity);
                // Build and return success message
                var template = DeleteLabels.DeletionSuccess;
                var message = string.Format(template, typeof(T).Name);
                return Operation<bool>.Success(true, message);
            }
            catch (Exception ex)
            {
                // Handle any errors
                return errorStrategyHandler.Fail<bool>(ex);
            }
        }
    }
}
