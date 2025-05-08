using Persistence.Repositories.Interface;
using Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Repositories
{
    public abstract class Repository<T>(DbContext context) : Read<T>(context), IRepository<T> where T : class, IEntity
    {
        private new readonly DbContext _context = RepositoryHelper.ValidateArgument(context);
        private new readonly DbSet<T> _dbSet = context.Set<T>();

        public async Task<bool> Create(T? entity)
        {
            entity = RepositoryHelper.ValidateArgument(entity);
            _dbSet.Add(entity);
            var result = await SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> Update(T entity)
        {
            RepositoryHelper.ValidateArgument(entity);
            _context.Entry(entity).State = EntityState.Modified;
            var result = await SaveChangesAsync();
            return result > 0;
        }

        public async Task<bool> Delete(T entity)
        {
            RepositoryHelper.ValidateArgument(entity);
            _dbSet.Remove(entity);
            var result = await SaveChangesAsync();
            return result > 0;
        }

        private async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
