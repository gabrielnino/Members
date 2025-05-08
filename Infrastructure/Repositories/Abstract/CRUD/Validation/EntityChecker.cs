using Application.Result;
using Application.UseCases.Repository.CRUD.Validation;
using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Validation
{
    public abstract class EntityChecker<T>(DbContext context) : Repository<T>(context), IEntityChecker<T> where T : class, IEntity
    {
        public virtual async Task<Operation<T>> HasEntity(string id)
        {
            var entityRepo = await ReadFilter(e => e.Id.Equals(id));
            var entityUnmodified = entityRepo?.FirstOrDefault();
            var hasEntity = entityUnmodified is not null;
            if (!hasEntity)
            {
                var validationResource = EntityCheckerLabels.EntityCheckerValidation;
                var messageExist = string.Format(validationResource, typeof(T).Name);
                return OperationStrategy<T>.Fail(messageExist, new BusinessStrategy<T>());
            }

            var successResource = EntityCheckerLabels.EntityCheckerSuccess;
            return Operation<T>.Success(entityUnmodified, successResource);
        }

        public virtual async Task<Operation<T>> HasId(string id)
        {
            var errorResource = EntityCheckerLabels.EntityCheckerFailedNecesaryData;
            if (string.IsNullOrWhiteSpace(id))
            {
                return OperationStrategy<T>.Fail(errorResource, new BusinessStrategy<T>());
            }

            var result = Utilities.GuidValidator.HasGuid(id);
            if (!result.IsSuccessful)
            {
                return result.AsType<T>();
            }

            return await HasEntity(id);
        }
    }
}
