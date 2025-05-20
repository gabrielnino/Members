using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using Persistence.Context.Interface;

namespace Persistence.Repositories
{
    public abstract class RepositoryCreate<T>(IUnitOfWork unitOfWork) 
        : Read<T>(unitOfWork) where T : class, IEntity
    {
        protected async Task Create(T entity)
        {
            await _dbSet.AddAsync(entity);
        }
    }
}
