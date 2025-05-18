using Application.Common.Pagination;
using Application.Result;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Domain;
using Autodesk.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

/// <summary>
/// Reads users with filters, paging, and caching.
/// </summary>
public class UserRead(DataContext context, IErrorStrategyHandler errorStrategyHandler, IMemoryCache cache) : IUserRead
{
    private readonly DataContext context = context;
    private readonly IErrorStrategyHandler errorStrategyHandler = errorStrategyHandler;
    private readonly IMemoryCache cache = cache;

    private readonly Func<IQueryable<User>, IOrderedQueryable<User>> orderBy = q => q.OrderBy(u => u.Name!).ThenBy(u => u.Id);
    private readonly Func<User, (string Primary, string Secondary)> cursorSelector = u => (u.Name!, u.Id);


    /// <summary>
    /// Fetch a page of users filtered by id or name, with cursor-based paging and caching.
    /// </summary>
    /// <param name="id">Optional user ID filter.</param>
    /// <param name="name">Optional user name filter.</param>
    /// <param name="cursor">Cursor for the next page.</param>
    /// <param name="pageSize">Number of items per page.</param>
    /// <returns>Operation result with paged users or error.</returns>
    public async Task<Operation<PagedResult<User>>> GetUsersPage(
        string? id,
        string? name,
        string? cursor,
        int pageSize)
    {
        try
        {
            var filter = BuildFilter(id, name);
            var query = BuildBaseQuery(filter);

            if (!string.IsNullOrEmpty(cursor))
            {
                query = ApplyCursorFilter(query, cursor);
            }
               

            var items = await query.Take(pageSize + 1).ToListAsync();

            var next = BuildNextCursor(items, pageSize);
            if (next != null) items.RemoveAt(pageSize);

            var result = new PagedResult<User> { Items = items, NextCursor = next };
            return Operation<PagedResult<User>>.Success(result);
        }
        catch (Exception ex)
        {
            return errorStrategyHandler.Fail<PagedResult<User>>(ex);
        }
    }


    private IQueryable<User> BuildBaseQuery(Expression<Func<User, bool>>? filter)
    {
        var q = context.Set<User>().AsNoTracking();
        if (filter != null) q = q.Where(filter);
        return orderBy(q);
    }

    private static IQueryable<User> ApplyCursorFilter(IQueryable<User> query, string cursor)
    {
        var parts = Uri.UnescapeDataString(cursor).Split('|', 2);
        var name = parts[0];
        var lastS = parts.Length > 1 ? parts[1] : string.Empty;

        return query.Where(u =>
                        DataContext.StringCompareOrdinal(u.Name!, name) > 0
                        || (u.Name == name
                            && DataContext.StringCompareOrdinal(u.Id, lastS) > 0)
                    );
    }

    private string? BuildNextCursor(List<User> items, int size)
    {
        if (items.Count <= size) return null;
        var extra = items[size];
        var (p, s) = cursorSelector(extra);
        return Uri.EscapeDataString($"{p}|{s}");
    }

    /// <summary>
    /// Choose filter by id or name, or none.
    /// </summary>
    private static Expression<Func<User, bool>> BuildFilter(string? id, string? name)
    {
        if (ShouldFilterById(id))
        {
            return BuildIdFilter(id!);
        }
           
        if (ShouldFilterByName(name))
        {
            return BuildNameFilter(name!);
        }
            
        return ReturnDefaultFilter();
    }

    /// <summary>
    /// Check if id is provided.
    /// </summary>
    private static bool ShouldFilterById(string? id) => !string.IsNullOrWhiteSpace(id);

    /// <summary>
    /// Check if name is provided.
    /// </summary>
    private static bool ShouldFilterByName(string? name) => !string.IsNullOrWhiteSpace(name);

    /// <summary>
    /// Filter users by id.
    /// </summary>
    private static Expression<Func<User, bool>> BuildIdFilter(string id) => u => u.Id == id;

    /// <summary>
    /// Filter users by name pattern.
    /// </summary>
    private static Expression<Func<User, bool>> BuildNameFilter(string name) => u => EF.Functions.Like(u.Name!, $"%{name}%");

    /// <summary>
    /// No filter: return all users.
    /// </summary>
    private static Expression<Func<User, bool>> ReturnDefaultFilter() => u => true;
}
