using Application.Result;
using System.Linq.Expressions;

namespace Autodesk.Application.UseCases.CRUD.User.Query
{
    using User = Domain.User;
    public interface IUserReadFilter
    {
        Task<Operation<IQueryable<User>>> ReadFilter(Expression<Func<User, bool>> predicate);
    }
}
