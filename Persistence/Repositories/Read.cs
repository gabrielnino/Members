using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Persistence.Repositories
{
    public abstract class Read<T>(DbContext context) : Repository<T>(context) where T : class, IEntity
    {
        protected Task<IQueryable<T>> ReadFilter(Expression<Func<T, bool>> predicate)
        {
            RepositoryHelper.ValidateArgument(predicate);
            return Task.FromResult(_dbSet.Where(predicate));
        }
    }
}
