using Application.Result;
using Application.UseCases.Repository.CRUD.Query;
using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Query.ReadId
{
    public abstract class ReadByIdRepository<T>(DbContext context, IErrorStrategyHandler errorStrategyHandler) : EntityChecker<T>(context), IReadById<T> where T : class, IEntity
    {
        public async Task<Operation<T>> ReadById(string id)
        {
            try
            {
                var validationResult = await HasId(id);
                if (validationResult is null)
                {
                    var strategy = new BusinessStrategy<T>();
                    return OperationStrategy<T>.Fail("Not found", strategy);
                }

                T? entity = validationResult;
                var readIdSuccess = ReadIdLabels.ReadIdSuccess;
                return Operation<T>.Success(entity, readIdSuccess);
            }
            catch (Exception ex)
            {
                return errorStrategyHandler.Fail<T>(ex);
            }
        }
    }
}
