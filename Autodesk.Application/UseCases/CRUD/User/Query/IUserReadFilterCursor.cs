using Application.Common.Pagination;
using Application.Result;

namespace Autodesk.Application.UseCases.CRUD.User.Query
{
    using User = Domain.User;
    public interface IUserReadFilterCursor
    {
        Task<Operation<PagedResult<User>>> ReadFilterCursor(
            string name,
            string? cursor,
            int pageSize);
    }
}
