using Application.Common.Pagination;
using Application.Result;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Domain;
using Autodesk.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Linq.Expressions;

/// <summary>
/// Reads users with filters, paging, and caching.
/// </summary>
public class UserRead(DataContext context, IErrorStrategyHandler errorStrategyHandler, IMemoryCache cache) : IUserRead
{
    private readonly DataContext context = context;
    private readonly IErrorStrategyHandler errorStrategyHandler = errorStrategyHandler;
    private readonly IMemoryCache cache = cache;

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
            var cacheKey = $"users:{id}:{name}:{cursor}:{pageSize}";
            if (cache.TryGetValue(cacheKey, out PagedResult<User> cached))
            {
                return Operation<PagedResult<User>>.Success(cached);
            }

            var query = context.Users
                .AsNoTracking()
                .Where(BuildUserFilter(id, name))
                .OrderBy(u => u.Name!)
                .ThenBy(u => u.Id);

            if (!string.IsNullOrEmpty(cursor))
            {
                var parts = Uri.UnescapeDataString(cursor).Split('|', 2);
                var lastName = parts[0];
                var lastId = parts[1];

                query = query
                    .Where(u =>
                        DataContext.StringCompareOrdinal(u.Name!, lastName) > 0
                        || (u.Name == lastName
                            && DataContext.StringCompareOrdinal(u.Id, lastId) > 0)
                    )
                    .OrderBy(u => u.Name!)
                    .ThenBy(u => u.Id);
            }

            var items = await query
                .Take(pageSize + 1)
                .ToListAsync();

            string? nextCursor = null;
            if (items.Count == pageSize + 1)
            {
                var extra = items[pageSize];
                nextCursor     = Uri.EscapeDataString($"{extra.Name}|{extra.Id}");
                items.RemoveAt(pageSize);
            }

            var result = new PagedResult<User>
            {
                Items      = items,
                NextCursor = nextCursor
            };

            cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            return Operation<PagedResult<User>>.Success(result);
        }
        catch (Exception ex)
        {
            return errorStrategyHandler.Fail<PagedResult<User>>(ex);
        }
    }

    /// <summary>
    /// Choose filter by id or name, or none.
    /// </summary>
    private static Expression<Func<User, bool>> BuildUserFilter(string? id, string? name)
    {
        if (ShouldFilterById(id))
            return BuildIdFilter(id!);
        if (ShouldFilterByName(name))
            return BuildNameFilter(name!);
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
