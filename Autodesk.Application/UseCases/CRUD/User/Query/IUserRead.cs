using Application.Common.Pagination;
using Application.Result;
using System.Linq.Expressions;

namespace Autodesk.Application.UseCases.CRUD.User.Query
{
    using User = Domain.User;
    public interface IUserRead
    {
        Task<Operation<PagedResult<User>>> GetUsersPage(string? id, string? name, string? cursor, int pageSize);
    }
}
