using Application.Result;
namespace Autodesk.Application.UseCases.CRUD.User
{
    using User = Domain.User;
    public interface IUserCreate
    {
        Task<Operation<bool>> Create(User entity);
    }
}
