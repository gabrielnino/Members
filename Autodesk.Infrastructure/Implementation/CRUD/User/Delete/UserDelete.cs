using Application.Result;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Persistence.Context;
using Infrastructure.Repositories.Abstract.CRUD.Delete;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Delete
{
    using User = Domain.User;

    /// <summary>
    /// Deletes a user and returns the result.
    /// </summary>
    /// <param name="context">Database context for user data.</param>
    /// <param name="errorStrategyHandler">Service to handle errors.</param>
    public class UserDelete(
        DataContext context,
        IErrorHandler errorStrategyHandler
    ) : DeleteRepository<User>(context, errorStrategyHandler), IUserDelete
    {
        // Inherits deletion logic from DeleteRepository.
    }
}
