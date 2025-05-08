using Application.Result;

namespace Application.UseCases.Repository.CRUD.Query
{
    public interface IReadFilterCount<T> where T : class
    {
        Task<Operation<int>> ReadFilterCount(string filter);
    }
}
