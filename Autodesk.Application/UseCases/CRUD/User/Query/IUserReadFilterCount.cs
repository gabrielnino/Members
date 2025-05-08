using Application.Result;

namespace Autodesk.Application.UseCases.CRUD.User.Query
{
    public interface IUserReadFilterCount
    {
        Task<Operation<int>> ReadFilterCount(string filter);
    }
}
