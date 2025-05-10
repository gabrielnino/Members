using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Update
{
    public abstract class UpdateRepository<T>(DbContext context, IErrorStrategyHandler errorStrategyHandler, IUtilEntity<T> utilEntity) : RepositoryUpdate<T>(context), IUpdate<T> where T : class, IEntity
    {
        public new async Task<Operation<bool>> Update(T entity)
        {
            try
            {
                Operation<T> hasEntity = await utilEntity.HasEntity(entity);
                if (!hasEntity.IsSuccessful)
                    return hasEntity.ConvertTo<bool>();
                var resultExist = await HasId(entity.Id);
                if (resultExist is null)
                {
                    var strategy = new BusinessStrategy<bool>();
                    return OperationStrategy<bool>.Fail("Not found", strategy);
                }
                Operation<T> resultModifyEntity = await UpdateEntity(entity, resultExist);
                if (!resultModifyEntity.IsSuccessful)
                {
                    return resultModifyEntity.ConvertTo<bool>();
                }
                var updateResult = await base.Update(resultModifyEntity.Data);
                var updateSuccess = "UpdateSuccess";
                var messageSuccess = string.Format(updateSuccess, typeof(T).Name);
                return Operation<bool>.Success(updateResult, messageSuccess);
            }
            catch (Exception ex)
            {
                return errorStrategyHandler.Fail<bool>(ex);
            }
        }

        public virtual async Task<Operation<T>> UpdateEntity(T entityModified, T entityUnmodified)
        {
            var updateEntitySearchSuccess = "UpdateEntitySearchSuccess";
            var messageSuccessfully = string.Format(updateEntitySearchSuccess, typeof(T).Name);
            return Operation<T>.Success(entityModified, messageSuccessfully);
        }
    }
}
