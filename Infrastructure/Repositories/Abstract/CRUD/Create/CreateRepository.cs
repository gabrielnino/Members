using Application.Result;
using Application.Result.EnumType;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Create
{
    public abstract class CreateRepository<T>(DbContext context, IUtilEntity<T> utilEntity, IErrorStrategyHandler errorStrategyHandler) : Repository<T>(context), ICreate<T> where T : class, IEntity
    {

        public new async Task<Operation<bool>> Create(T entity)
        {
            try
            {
                var hasEntity = await utilEntity.HasEntity(entity);
                if (!hasEntity.IsSuccessful)
                {
                    return hasEntity.ConvertTo<bool>();
                }

                var validationResult = await CreateEntity(entity);
                if (!validationResult.IsSuccessful)
                {
                    return validationResult.ConvertTo<bool>();
                }

                var unique = base.ReadCountFilter(E => E.Id == entity.Id);
                var addedEntityResult = await base.Create(validationResult.Data);
                var creationSuccess = await GetCreationSuccess();
                var successMessage = string.Format(creationSuccess, typeof(T).Name);
                return Operation<bool>.Success(addedEntityResult, successMessage);
            }
            catch (Exception ex)
            {
                return errorStrategyHandler.Fail<bool>(ex, "failedToUploadImage");
            }
        }

        protected virtual Task<string> GetCreationSuccess()
        {
            return Task.FromResult(CreateLabels.CreationSuccess);
        }

        protected abstract Task<Operation<T>> CreateEntity(T entity);
    }
}
