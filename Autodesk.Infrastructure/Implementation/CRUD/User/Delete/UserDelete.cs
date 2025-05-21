using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Autodesk.Application.UseCases.CRUD.User;
using Infrastructure.Repositories.Abstract.CRUD.Delete;
using Persistence.Context.Interface;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Delete
{
    using User = Domain.User;

    /// <summary>
    /// Deletes a user and returns the result.
    /// </summary>
    /// <param name="context">Database context for user data.</param>
    /// <param name="errorHandler">Service to handle errors.</param>
    public class UserDelete(IUnitOfWork unitOfWork, IErrorHandler errorHandler, IErrorLogCreate errorLogCreate) : DeleteRepository<User>(unitOfWork), IUserDelete
    {
        public async Task<Operation<bool>> DeleteUserAsync(string id)
        {
            try
            {
                var result = await DeleteEntity(id);
                await unitOfWork.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                return errorHandler.Fail<bool>(ex, errorLogCreate);
            }
        }
    }
}
