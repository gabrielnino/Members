using Application.Common.Pagination;
using Application.Result;

namespace LiveNetwork.Application.UseCases.CRUD.Profile.Query
{
    using Profile = Domain.Profile;
    public interface IProfileRead
    {
        Task<Operation<PagedResult<Profile>>> GetProfilesPageAsync(string? id, string? name, string? cursor, int pageSize);
    }
}
