using Application.Result;

namespace Application.UseCases.Repository.CRUD.Query
{
    public interface IReadFilterPage<T> where T : class
    {
        Task<Operation<IQueryable<T>>> ReadFilterPage(int pageNumber, int pageSize, string filter);
    }
}
