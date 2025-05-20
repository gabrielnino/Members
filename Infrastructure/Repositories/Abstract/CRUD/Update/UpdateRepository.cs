using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Infrastructure.Repositories.Abstract.CRUD.Delete;
using Persistence.Context.Interface;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Update
{
    /// <summary>
    /// Base class to update entities with validation and error handling.
    /// </summary>
    public abstract class UpdateRepository<T>(IUnitOfWork unitOfWork) : RepositoryUpdate<T>(unitOfWork), IUpdate<T> where T : class, IEntity
    {
        /// <summary>
        /// Update an entity after checking existence and applying custom logic.
        /// </summary>
        /// <param name="modify">The modified entity.</param>
        /// <returns>Result true if updated, otherwise false.</returns>
        public async Task<Operation<bool>> UpdateEntity(T modify)
        {
            var entity = await HasId(modify.Id);
            if (entity is null)
            {
                var strategy = new BusinessStrategy<bool>();
                return OperationStrategy<bool>.Fail(UpdateLabels.EntityNotFound, strategy);
            }
            var modified = ApplyUpdates(modify, entity);
            Update(modified);
            var success = UpdateLabels.UpdationSuccess;
            var message = string.Format(success, typeof(T).Name);
            return Operation<bool>.Success(true, message);
        }

        public abstract T ApplyUpdates(T modified, T unmodified);
    }
}
