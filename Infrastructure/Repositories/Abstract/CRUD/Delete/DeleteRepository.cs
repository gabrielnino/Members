using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Infrastructure.Repositories.Abstract.CRUD.Validation;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories;
using static Application.Constants.Messages;

namespace Infrastructure.Repositories.Abstract.CRUD.Delete
{
    public abstract class DeleteRepository<T>(DbContext context, IErrorStrategyHandler errorStrategyHandler) : EntityChecker<T>(context), IDelete<T> where T : class, IEntity
    {
        public async Task<Operation<bool>> Delete(string id)
        {
            try
            {
                Operation<T> validationResult = await HasId(id);
                if (!validationResult.IsSuccessful)
                {
                    return validationResult.ConvertTo<bool>();
                }

                var entity = RepositoryHelper.ValidateArgument(validationResult.Data);
                var result = await Delete(entity);
                var deletionSuccess = DeleteLabels.DeletionSuccess;
                var messageSuccess = string.Format(deletionSuccess, typeof(T).Name);
                return Operation<bool>.Success(result, messageSuccess);
            }
            catch (Exception ex)
            {
                return errorStrategyHandler.Fail<bool>(ex, "failedToUploadImage");
            }
        }
    }
}
