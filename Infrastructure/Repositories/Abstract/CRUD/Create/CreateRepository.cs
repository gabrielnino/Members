using Application.UseCases.Repository.CRUD;
using Domain.Interfaces.Entity;
using Persistence.Context.Interface;
using Persistence.Repositories;

namespace Infrastructure.Repositories.Abstract.CRUD.Create
{
    /// <summary>
    /// Base class to create entities with validation and error handling.
    /// </summary>
    public abstract class CreateRepository<T>(IUnitOfWork unitOfWork) : RepositoryCreate<T>(unitOfWork), ICreate<T> where T : class, IEntity
    {
        public new async Task Create(T entity)
        {
            await base.Create(entity);
        }
    }
}
