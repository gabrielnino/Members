using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public abstract class RepositoryDelete<T>(DbContext context) : EntityChecker<T>(context) where T : class, IEntity
    {
        protected async Task<bool> Delete(T entity)
        {
            RepositoryHelper.ValidateArgument(entity);
            _dbSet.Remove(entity);
            var result = await SaveChangesAsync();
            return result > 0;
        }
    }
}
