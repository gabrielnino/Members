using Application.Result;
using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;

namespace Infrastructure.Repositories.Abstract.CRUD.Util
{
    public class UtilEntity<T> : IUtilEntity<T> where T : class, IEntity
    {
        public async Task<Operation<T>> HasEntity(T entity)
        {
            var utilEntityFailedNecesaryData = UtilEntityLabels.UtilEntityFailedNecesaryData;
            if (entity is null)
            {
                return OperationStrategy<T>.Fail(utilEntityFailedNecesaryData, new BusinessStrategy<T>());
            }

            var utilEntitySuccess = UtilEntityLabels.UtilEntitySuccess;
            return Operation<T>.Success(entity, utilEntitySuccess);
        }
    }
}
