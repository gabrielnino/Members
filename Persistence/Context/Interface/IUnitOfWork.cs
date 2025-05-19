using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Context.Interface
{
    public interface IUnitOfWork
    {
        Task<int> CommitAsync();
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task CommitTransactionAsync(IDbContextTransaction tx);
        Task RollbackAsync(IDbContextTransaction tx);
        DbContext Context { get; }
    }
}
