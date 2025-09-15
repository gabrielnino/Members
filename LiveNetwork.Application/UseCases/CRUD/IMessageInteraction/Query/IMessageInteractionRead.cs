using Application.Common.Pagination;
using Application.Result;
using LiveNetwork.Domain;

namespace LiveNetwork.Application.UseCases.CRUD.Profile.Query
{
    public interface IMessageInteractionRead
    {
        Task<Operation<PagedResult<MessageInteraction>>> GetMessageInteractionPageAsync(string? id, string? cursor, int pageSize);
    }
}
