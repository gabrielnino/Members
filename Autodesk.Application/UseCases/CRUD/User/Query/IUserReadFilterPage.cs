using Application.Result;

namespace Autodesk.Application.UseCases.CRUD.User.Query
{
    using User = Domain.User;
    public interface IUserReadFilterPage
    {
        Task<Operation<IQueryable<User>>> ReadFilterPage(int pageNumber, int pageSize, string filter);
    }
}
