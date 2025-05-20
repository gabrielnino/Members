using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Delete
{
    public abstract class DeleteRepository<T>(IUnitOfWork unitOfWork)
        : RepositoryDelete<T>(unitOfWork), IDelete<T> where T : class, IEntity
    {
        public async Task<Operation<bool>> DeleteEntity(string id)
        {
            var entity = await HasId(id);
            if (entity is null)
            {
                var strategy = new BusinessStrategy<bool>();
                return OperationStrategy<bool>.Fail(DeleteLabels.EntityNotFound, strategy);
            }
            Delete(entity);
            var success = DeleteLabels.DeletionSuccess;
            var message = string.Format(success, typeof(T).Name);
            return Operation<bool>.Success(true, message);
        }
    }
}
