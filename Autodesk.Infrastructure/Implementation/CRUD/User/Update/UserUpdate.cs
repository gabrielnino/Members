using Application.Result;
using Application.UseCases.Repository.CRUD;
using Autodesk.Application.UseCases.CRUD.User;
using Infrastructure.Repositories.Abstract.CRUD.Update;
using Infrastructure.Result;
using Persistence.Context.Interface;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Update
{
    using User = Domain.User;

    /// <summary>
    /// Handles updating a user record.
    /// </summary>
    public class UserUpdate(
        IUnitOfWork unitOfWork,
        IErrorHandler errorHandler
    ) : UpdateRepository<User>(unitOfWork), IUserUpdate
    {
        public override User ApplyUpdates(User modified, User unmodified)
        {
            unmodified.Name = modified.Name;
            unmodified.Lastname = modified.Lastname;
            unmodified.Email = modified.Email;
            return unmodified;
        }

        public async Task<Operation<bool>> UpdateUserAsync(User entity)
        {
            try
            {
                var result = await UpdateEntity(entity);
                await unitOfWork.CommitAsync();
                return result;
            }
            catch (Exception ex)
            {
                return errorHandler.Fail<bool>(ex);
            }
        }
    }
}
