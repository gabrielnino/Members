using Application.Result;
using Autodesk.Application.UseCases.CRUD.User;
using Infrastructure.Repositories.Abstract.CRUD.Create;
using Persistence.Context.Interface;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Create
{
    using User = Domain.User;

    /// <summary>
    /// Creates a user, ensuring no duplicate email exists.
    /// </summary>
    public class UserCreate(IUnitOfWork unitOfWork, IErrorHandler errorHandler) : CreateRepository<User>(unitOfWork), IUserCreate
    {
        public async Task<Operation<User>> CreateUserAsync(User entity)
        {
            try
            {
                await CreateEntity(entity);
                await unitOfWork.CommitAsync();
                return Operation<User>.Success(entity);
            }
            catch (Exception ex)
            {
                return errorHandler.Fail<User>(ex);
            }
        }
    }
}
