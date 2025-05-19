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
    public class UserCreate(IUnitOfWork unitOfWork) : CreateRepository<User>(unitOfWork), IUserCreate
    {
        public async Task<Operation<User>> CreateEntity(User entity)
        {
            await Create(entity);
            await unitOfWork.CommitAsync();
            return Operation<User>.Success(entity);
        }
    }
}
