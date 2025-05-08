using Application.Result;
using Domain.Interfaces.Entity;

namespace Application.UseCases.Repository.CRUD
{
    public interface IDelete<T> where T : class, IEntity
    {
        Task<Operation<bool>> Delete(string id);
    }
}
