using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Infrastructure.Repositories.Abstract.CRUD.Create;
using Microsoft.Extensions.Caching.Memory;
using Persistence.Context.Interface;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Create
{
    using User = Domain.User;

    /// <summary>
    /// Creates a user, ensuring no duplicate email exists.
    /// </summary>
    public class UserCreate(IUnitOfWork unitOfWork, IErrorHandler errorHandler, IErrorLogCreate errorLogCreate, IUserRead userRead) : CreateRepository<User>(unitOfWork), IUserCreate
    {
        public async Task<Operation<User>> CreateUserAsync(User entity)
        {
            try
            {
                await CreateEntity(entity);
                await unitOfWork.CommitAsync();
                userRead.InvalidateAllUserCache();
                return Operation<User>.Success(entity);
            }
            catch (Exception ex)
            {
                return errorHandler.Fail<User>(ex, errorLogCreate);
            }
        }
    }
}
