using Application.Result;
using Domain.Interfaces.Entity;

namespace Application.UseCases.Repository.CRUD
{
    public interface IUtilEntity<T> where T : class, IEntity
    {
        Task<Operation<T>> HasEntity(T entity);
    }
}
