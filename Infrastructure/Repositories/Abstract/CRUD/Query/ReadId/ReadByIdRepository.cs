using Application.Result;
using Application.UseCases.Repository.CRUD.Query;
using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Query.ReadId
{
    /// <summary>
    /// Retrieves an entity by its ID or returns a not-found error.
    /// </summary>
    public abstract class ReadByIdRepository<T>(DbContext context, IErrorHandler errorStrategyHandler)
        : EntityChecker<T>(context), IReadById<T>
        where T : class, IEntity
    {
        /// <summary>
        /// Finds an entity using the given ID.
        /// </summary>
        /// <param name="id">The unique identifier.</param>
        /// <returns>
        /// A successful operation with the entity, or a failure if missing.
        /// </returns>
        public async Task<Operation<T>> ReadById(string id)
        {
            try
            {
                var found = await HasId(id);
                if (found is null)
                {
                    var strategy = new BusinessStrategy<T>();
                    return OperationStrategy<T>.Fail("Not found", strategy);
                }

                var successMsg = ReadIdLabels.ReadIdSuccess;
                return Operation<T>.Success(found, successMsg);
            }
            catch (Exception ex)
            {
                return errorStrategyHandler.Fail<T>(ex);
            }
        }
    }
}
