using Application.Result;
using Application.UseCases.Repository.CRUD;
using Autodesk.Application.UseCases.CRUD.User;
using Infrastructure.Repositories.Abstract.CRUD.Update;
using Persistence.Context.Interface;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Update
{
    using User = Domain.User;

    /// <summary>
    /// Handles updating a user record.
    /// </summary>
    public class UserUpdate(
        IUnitOfWork unitOfWork,
        IErrorHandler errorHandler,
        IUtilEntity<User> utilEntity
    ) : UpdateRepository<User>(unitOfWork, errorHandler, utilEntity), IUserUpdate
    {
        /// <summary>
        /// Copies new values into the existing user and returns success.
        /// </summary>
        /// <param name="entityModified">User object with updated fields.</param>
        /// <param name="entityUnmodified">Original user from the database.</param>
        /// <returns>
        /// Operation result containing the updated user and a success message.
        /// </returns>
        public override async Task<Operation<User>> UpdateEntity(
            User entityModified,
            User entityUnmodified
        )
        {
            // Apply changes
            entityUnmodified.Name     = entityModified.Name;
            entityUnmodified.Lastname = entityModified.Lastname;
            entityUnmodified.Email    = entityModified.Email;

            // Prepare success message
            var template = UserUpdateLabels.UpdateSuccessfullySearchGeneric;
            var message = string.Format(template, nameof(User));
            return Operation<User>.Success(entityUnmodified, message);
        }
    }
}
