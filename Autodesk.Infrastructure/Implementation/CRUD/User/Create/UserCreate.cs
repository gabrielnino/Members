using Application.Result;
using Application.UseCases.Repository.CRUD;
using Autodesk.Application.UseCases.CRUD.User;
using Autodesk.Persistence.Context;
using Infrastructure.Repositories.Abstract.CRUD.Create;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Create
{
    using User = Domain.User;

    /// <summary>
    /// Creates a user, ensuring no duplicate email exists.
    /// </summary>
    public class UserCreate(
        DataContext context,
        IUtilEntity<User> utilEntity,
        IErrorHandler errorStrategyHandler
    ) : CreateRepository<User>(context, utilEntity, errorStrategyHandler), IUserCreate
    {
        /// <summary>
        /// Validates that the email is unique then returns success or a business error.
        /// </summary>
        /// <param name="entity">The user to create.</param>
        /// <returns>
        /// A failure if the email is already registered; otherwise a success with the user.
        /// </returns>
        protected override async Task<Operation<User>> CreateEntity(User entity)
        {
            var email = entity?.Email ?? string.Empty;
            var userByEmail = await ReadFilter(p => p.Email == email);
            var userExistByEmail = userByEmail.FirstOrDefault();
            if (userExistByEmail != null)
            {
                var error = UserCreateLabels.CreateAlreadyRegisteredErrorEmail;
                return OperationStrategy<User>.Fail(error, new BusinessStrategy<User>());
            }

            return Operation<User>.Success(entity);
        }
    }
}
