using Microsoft.EntityFrameworkCore.Storage;
using Persistence.Context.Implementation;

namespace Persistence.Context.Interface
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync(IDbContextTransaction tx);
        Task RollbackAsync(IDbContextTransaction tx);
        DataContext Context { get; }
    }
}
