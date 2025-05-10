using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repositories
{
    public abstract class RepositoryUpdate<T>(DbContext context) : EntityChecker<T>(context) where T : class, IEntity
    {
        protected async Task<bool> Update(T entity)
        {
            RepositoryHelper.ValidateArgument(entity);
            _context.Entry(entity).State = EntityState.Modified;
            var result = await SaveChangesAsync();
            return result > 0;
        }
    }
}
