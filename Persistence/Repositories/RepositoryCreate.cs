using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public abstract class RepositoryCreate<T>(DbContext context) : Read<T>(context) where T : class, IEntity
    {
        protected async Task<bool> Create(T? entity)
        {
            entity = RepositoryHelper.ValidateArgument(entity);
            _dbSet.Add(entity);
            var result = await SaveChangesAsync();
            return result > 0;
        }
    }
}
