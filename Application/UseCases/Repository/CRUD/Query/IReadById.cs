using Application.Result;
using Domain.Interfaces.Entity;

namespace Application.UseCases.Repository.CRUD.Query
{
    public interface IReadById<T> where T : class, IEntity
    {
        Task<Operation<T>> ReadById(string id);
    }
}
