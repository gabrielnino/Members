using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Persistence.Repositories
{
    public abstract class Read<T>(DbContext context) : Repository<T>(context) where T : class, IEntity
    {
        //private new readonly DbContext _context = RepositoryHelper.ValidateArgument(context);
        //private new readonly DbSet<T> _dbSet = context.Set<T>();

        //protected async Task<bool> Create(T? entity)
        //{
        //    entity = RepositoryHelper.ValidateArgument(entity);
        //    _dbSet.Add(entity);
        //    var result = await SaveChangesAsync();
        //    return result > 0;
        //}

        //protected async Task<bool> Update(T entity)
        //{
        //    RepositoryHelper.ValidateArgument(entity);
        //    _context.Entry(entity).State = EntityState.Modified;
        //    var result = await SaveChangesAsync();
        //    return result > 0;
        //}

        //protected async Task<bool> Delete(T entity)
        //{
        //    RepositoryHelper.ValidateArgument(entity);
        //    _dbSet.Remove(entity);
        //    var result = await SaveChangesAsync();
        //    return result > 0;
        //}



        /// <summary>
        /// Get entities matching <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Filter expression.</param>
        /// <returns>Matching entities.</returns>
        protected Task<IQueryable<T>> ReadFilter(Expression<Func<T, bool>> predicate)
        {
            RepositoryHelper.ValidateArgument(predicate);
            return Task.FromResult(_dbSet.Where(predicate));
        }
    }
}
