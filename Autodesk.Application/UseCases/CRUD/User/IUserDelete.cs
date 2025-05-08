using Application.Result;

namespace Autodesk.Application.UseCases.CRUD.User
{
    public interface IUserDelete
    {
        Task<Operation<bool>> Delete(string id);
    }
}
