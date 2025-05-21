using Application.Result;

namespace Autodesk.Application.UseCases.CRUD.User
{
    using User = Domain.User;

    /// <summary>
    /// Update a user.
    /// </summary>
    public interface IUserUpdate
    {
        /// <summary>
        /// Update the given user entity.
        /// </summary>
        /// <param name="entity">User to update.</param>
        /// <returns>
        /// Operation result: true if successful, false otherwise.
        /// </returns>
        Task<Operation<bool>> UpdateUserAsync(User entity);
    }
}
