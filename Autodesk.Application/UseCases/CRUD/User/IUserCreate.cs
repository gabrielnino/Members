using Application.Result;

namespace Autodesk.Application.UseCases.CRUD.User
{
    using User = Domain.User;

    /// <summary>
    /// Defines the user creation operation.
    /// </summary>
    public interface IUserCreate
    {
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="entity">The user to add.</param>
        /// <returns>
        /// Operation result: true if created, false otherwise.
        /// </returns>
        Task<Operation<bool>> Create(User entity);
    }
}
