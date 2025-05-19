using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Context.Interface;

namespace Persistence.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DbContext _context;

        public UnitOfWork(DbContext context) => _context = context;

        public DbContext Context => _context;

        public async Task<int> CommitAsync() => await _context.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync()=> await _context.Database.BeginTransactionAsync();
        public async Task CommitTransactionAsync(IDbContextTransaction tx)
        {
            await _context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        public async Task RollbackAsync(IDbContextTransaction tx) => await tx.RollbackAsync();

        public void Dispose() => _context.Dispose();
    }
}
