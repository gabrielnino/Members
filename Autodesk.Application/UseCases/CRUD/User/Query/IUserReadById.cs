using global::Application.Result;
namespace Autodesk.Application.UseCases.CRUD.User.Query
{
    using User = Domain.User;
    public interface IUserReadById
    {
        Task<Operation<User>> ReadById(string id);
        Task<Operation<User>> ReadByBearerToken(string bearerToken);
    }
}
