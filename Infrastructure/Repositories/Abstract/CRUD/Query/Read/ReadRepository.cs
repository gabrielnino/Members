using Domain.Interfaces.Entity;
using Application.Result;
using Microsoft.EntityFrameworkCore;
using Application.Common.Pagination;
using System.Linq.Expressions;
using Persistence.Context.Interface;

namespace Infrastructure.Repositories.Abstract.CRUD.Query.Read
{
    public abstract class ReadRepository<T>(
        IUnitOfWork unitOfWork,
        IErrorHandler errorHandler,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy) where T : class, IEntity
    {
        public async Task<Operation<PagedResult<T>>> GetPageAsync(Expression<Func<T, bool>>? filter, string? cursor, int pageSize)
        {
            var query = BuildBaseQuery(filter);
            var count = query.Count();
            if (!string.IsNullOrEmpty(cursor))
            {
                query = ApplyCursorFilter(query, cursor);
            }
            var items = await query.Take(pageSize + 1).ToListAsync();
            var next = BuildNextCursor(items, pageSize);
            if (next != null)
            {
                items.RemoveAt(pageSize);
            }
            var result = new PagedResult<T> { Items = items, NextCursor = next, TotalCount = count };
            return Operation<PagedResult<T>>.Success(result);
        }
        private IQueryable<T> BuildBaseQuery(Expression<Func<T, bool>>? filter)
        {
            var q = unitOfWork.Context.Set<T>().AsNoTracking();
            if (filter != null) q = q.Where(filter);
            return orderBy(q);
        }
        protected abstract IQueryable<T> ApplyCursorFilter(IQueryable<T> query, string cursor);
        protected abstract string? BuildNextCursor(List<T> items, int size);
    }
}

     
 
