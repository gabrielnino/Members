using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Context.Interface;


namespace Persistence.Context.Implementation
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DataContext _context;

        public UnitOfWork(DataContext context) => _context = context;

        public DataContext Context => _context;

        public async Task<int> CommitAsync() => await Context.SaveChangesAsync();

        public async Task<IDbContextTransaction> BeginTransactionAsync() => await Context.Database.BeginTransactionAsync();
        public async Task CommitTransactionAsync(IDbContextTransaction tx)
        {
            await Context.SaveChangesAsync();
            await tx.CommitAsync();
        }
        public async Task RollbackAsync(IDbContextTransaction tx) => await tx.RollbackAsync();

        public void Dispose() => Context.Dispose();
    }
}
