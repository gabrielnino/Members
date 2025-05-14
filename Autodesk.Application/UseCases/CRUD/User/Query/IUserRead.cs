using Application.Common.Pagination;
using Application.Result;

namespace Autodesk.Application.UseCases.CRUD.User.Query
{
    using User = Domain.User;

    /// <summary>
    /// Reads user data with paging support.
    /// </summary>
    public interface IUserRead
    {
        /// <summary>
        /// Retrieves a page of users filtered by optional parameters.
        /// </summary>
        /// <param name="id">Optional user ID filter.</param>
        /// <param name="name">Optional name filter.</param>
        /// <param name="cursor">Cursor for the next page.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>
        /// An operation result with a paged list of users or error details.
        /// </returns>
        Task<Operation<PagedResult<User>>> GetUsersPage(
            string? id,
            string? name,
            string? cursor,
            int pageSize);
    }
}
