using Application.Result;
using Domain.Interfaces.Entity;

namespace Application.UseCases.Repository.CRUD
{
    public interface IUpdate<T> where T : class, IEntity
    {
        Task<Operation<bool>> Update(T entity);
    }
}
