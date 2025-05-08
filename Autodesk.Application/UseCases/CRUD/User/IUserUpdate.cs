using Application.Result;

namespace Autodesk.Application.UseCases.CRUD.User
{
    using User = Domain.User;
    public interface IUserUpdate
    {
        Task<Operation<bool>> Update(User entity);
    }
}
