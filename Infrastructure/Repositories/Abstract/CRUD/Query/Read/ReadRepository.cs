using Domain.Interfaces.Entity;
using Application.Result;
using Microsoft.EntityFrameworkCore;
using Application.Common.Pagination;
using System.Linq.Expressions;
using Domain;

namespace Infrastructure.Repositories.Abstract.CRUD.Query.Read
{
    public abstract class ReadRepository<T>(
        DbContext context,
        IErrorStrategyHandler errorHandler,
        Func<IQueryable<T>, IOrderedQueryable<T>> orderBy,
        Func<T, (string Primary, string Secondary)> cursorSelector) where T : class, IEntity
    {
        public async Task<Operation<PagedResult<T>>> GetPageAsync(Expression<Func<T, bool>>? filter,  string? cursor, int pageSize)
        {
            try
            {

                IQueryable<T> query = context.Set<T>().AsNoTracking();
                if (filter is not null)
                {
                    query = query.Where(filter);
                }


                query = orderBy(query);

                // Apply cursor if present
                if (!string.IsNullOrEmpty(cursor))
                {
                    var parts = Uri.UnescapeDataString(cursor).Split('|', 2);
                    var lastPrimary = parts[0];
                    var lastSecondary = parts.Length > 1 ? parts[1] : string.Empty;
                    query = query.Where(e => IsAfterCursor(e, lastPrimary, lastSecondary));
                }

                // Fetch one extra to detect next page
                var items = await query
                    .Take(pageSize + 1)
                    .ToListAsync();

                string? nextCursor = null;
                if (items.Count == pageSize + 1)
                {
                    var extra = items[pageSize];
                    var (p, s) = cursorSelector(extra);
                    nextCursor = Uri.EscapeDataString($"{p}|{s}");
                    items.RemoveAt(pageSize);
                }

                var result = new PagedResult<T>
                {
                    Items      = items,
                    NextCursor = nextCursor
                };

                return Operation<PagedResult<T>>.Success(result);
            }
            catch (Exception ex)
            {
                return errorHandler.Fail<PagedResult<T>>(ex);
            }
        }

        private bool IsAfterCursor(T entity, string lastPrimary, string lastSecondary)
        {
            return IsPrimary(entity, lastPrimary) || (IsSecondary(entity, lastPrimary, lastSecondary));
        }

        private bool IsPrimary(T entity, string lastPrimary)
        {
            return ReadRepository<T>.IsGreaterThan(entity, lastPrimary, e => GetPrimary(e));
        }

        private bool IsSecondary(T entity, string lastPrimary, string lastSecondary)
        {
            return GetPrimary(entity) == lastPrimary && ReadRepository<T>.IsGreaterThan(entity, lastSecondary, e => GetSecondary(e));
        }

        private static bool IsGreaterThan(T entity, string lastValue, Func<T, string> selector)
        {
            return string.Compare(selector(entity), lastValue, StringComparison.Ordinal) > 0;
        }

        private string GetPrimary(T entity) => cursorSelector(entity).Primary;
        private string GetSecondary(T entity) => cursorSelector(entity).Secondary;
    }
}

     
 
