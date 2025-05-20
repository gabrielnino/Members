using Application.Result;
using Domain.Interfaces.Entity;

namespace Application.UseCases.Repository.CRUD
{
    public interface ICreate<T> where T : class, IEntity
    {
        Task<Operation<bool>> CreateEntity(T entity);
    }
}
