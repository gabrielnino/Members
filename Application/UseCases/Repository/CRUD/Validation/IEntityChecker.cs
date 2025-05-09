using Application.Result;
using Domain.Interfaces.Entity;

namespace Application.UseCases.Repository.CRUD.Validation
{
    public interface IEntityChecker<T> where T : class, IEntity
    {
        Task<Operation<T>> HasEntity(string id);
    }
}
