using Application.Result;
using System.Linq.Expressions;

namespace Application.UseCases.Repository.CRUD.Query
{
    public interface IReadFilter<T> where T : class
    {
        Task<Operation<IQueryable<T>>> ReadFilter(Expression<Func<T, bool>> predicate);
    }
}
