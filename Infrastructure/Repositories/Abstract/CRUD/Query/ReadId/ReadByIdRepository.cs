using Application.Result;
using Application.UseCases.Repository.CRUD.Query;
using Domain.Interfaces.Entity;
using Infrastructure.Repositories.Abstract.CRUD.Validation;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories.Abstract.CRUD.Query.ReadId
{
    public abstract class ReadByIdRepository<T>(DbContext context, IErrorStrategyHandler errorStrategyHandler) : EntityChecker<T>(context), IReadById<T> where T : class, IEntity
    {
        public async Task<Operation<T>> ReadById(string id)
        {
            try
            {
                Operation<T> validationResult = await HasId(id);
                if (!validationResult.IsSuccessful)
                {
                    return validationResult.ConvertTo<T>();
                }

                T? entity = validationResult.Data;
                var readIdSuccess = ReadIdLabels.ReadIdSuccess;
                return Operation<T>.Success(entity, readIdSuccess);
            }
            catch (Exception ex)
            {
                return errorStrategyHandler.Fail<T>(ex);
            }
        }

        public async Task<Operation<T>> ReadByBearerToken(string bearerToken)
        {
            try
            {
                var resultbearer = JwtHelper.ExtractJwtPayload(bearerToken);
                if (!resultbearer.IsSuccessful)
                {
                    var strategy = new DatabaseStrategy<T>();
                    return OperationStrategy<T>.Fail(resultbearer.Message, strategy);
                }

                Operation<T> validationResult = await HasId(resultbearer.Data);
                if (!validationResult.IsSuccessful)
                {
                    return validationResult.ConvertTo<T>();
                }

                T? entity = validationResult.Data;
                var readByBearerSuccess = ReadIdLabels.ReadByBearerSuccess;
                return Operation<T>.Success(entity, readByBearerSuccess);
            }
            catch (Exception ex)
            {
                return errorStrategyHandler.Fail<T>(ex);
            }
        }
    }
}
