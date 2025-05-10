using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Delete
{
    public abstract class DeleteRepository<T>(DbContext context, IErrorStrategyHandler errorStrategyHandler) : RepositoryDelete<T>(context), IDelete<T> where T : class, IEntity
    {
        public async Task<Operation<bool>> Delete(string id)
        {
            try
            {
                var validationResult = await HasId(id);
                if (validationResult is null)
                {
                    var strategy = new BusinessStrategy<bool>();
                    return OperationStrategy<bool>.Fail("Not found", strategy);
                }

                var entity = RepositoryHelper.ValidateArgument(validationResult);
                var result = await base.Delete(entity);
                var deletionSuccess = DeleteLabels.DeletionSuccess;
                var messageSuccess = string.Format(deletionSuccess, typeof(T).Name);
                return Operation<bool>.Success(result, messageSuccess);
            }
            catch (Exception ex)
            {
                return errorStrategyHandler.Fail<bool>(ex);
            }
        }
    }
}
