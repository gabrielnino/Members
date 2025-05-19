using Application.Result;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Persistence.Context;
using Infrastructure.Repositories.Abstract.CRUD.Delete;
using Persistence.Context.Interface;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Delete
{
    using User = Domain.User;

    /// <summary>
    /// Deletes a user and returns the result.
    /// </summary>
    /// <param name="context">Database context for user data.</param>
    /// <param name="errorStrategyHandler">Service to handle errors.</param>
    public class UserDelete(
        IUnitOfWork unitOfWork,
        IErrorHandler errorStrategyHandler
    ) : DeleteRepository<User>(unitOfWork, errorStrategyHandler), IUserDelete
    {
        public override async Task<Operation<bool>> Delete(string id)
        {
            var result = await base.Delete(id);
            await unitOfWork.CommitAsync();
            return result;
        }
    }
}
