using Application.Result;
using Application.UseCases.Repository.CRUD.Query;
using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Query.ReadId
{
    public abstract class ReadByIdRepository<T>(IUnitOfWork unitOfWork, IErrorHandler errorHandler)
        : EntityChecker<T>(unitOfWork), IReadById<T> where T : class, IEntity
    {
        public async Task<Operation<T>> ReadById(string id)
        {
            var found = await HasId(id);
            if (found is null)
            {
                var strategy = new BusinessStrategy<T>();
                return OperationStrategy<T>.Fail(ReadIdLabels.EntityNotFound, strategy);
            }

            var successMsg = ReadIdLabels.ReadIdSuccess;
            return Operation<T>.Success(found, successMsg);
        }
    }
}
