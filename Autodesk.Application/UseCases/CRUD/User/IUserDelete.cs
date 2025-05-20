using Application.Result;

namespace Autodesk.Application.UseCases.CRUD.User
{
    /// <summary>
    /// Delete a user by its ID.
    /// </summary>
    public interface IUserDelete
    {
        /// <summary>
        /// Deletes the user with the given ID.
        /// Returns an operation indicating success.
        /// </summary>
        Task<Operation<bool>> DeleteUserAsync(string id);
    }
}
