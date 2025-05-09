using Application.Common.Pagination;
using Application.Result;
using Autodesk.Application.UseCases.CRUD.User.Query;
using Autodesk.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Autodesk.Infrastructure.Implementation.CRUD.User.Query.ReadFilter
{
    using User = Domain.User;

    public class UserReadFilterCursor(DataContext context, IErrorStrategyHandler errorStrategyHandler): IUserReadFilterCursor
    {
        public async Task<Operation<PagedResult<User>>> ReadFilterCursor(
            string? id,
            string? name,
            string? cursor,
            int pageSize)
        {
            try
            {
                var query = context.Users
                                   .AsNoTracking()
                                   .Where(BuildIdOrNameFilter(id, name))
                                   .OrderBy(u => u.Name!)
                                   .ThenBy(u => u.Id);

                if (!string.IsNullOrEmpty(cursor))
                {
                    var parts = Uri.UnescapeDataString(cursor).Split('|', 2);
                    var lastName = parts[0];
                    var lastId = parts[1];
                    query = (IOrderedQueryable<User>)query.Where(BuildCursorFilter(lastName, lastId));
                }

                var items = await query.Take(pageSize + 1).ToListAsync();

                string? nextCursor = null;
                if (items.Count == pageSize + 1)
                {
                    var extra = items[pageSize];
                    nextCursor = Uri.EscapeDataString($"{extra.Name}|{extra.Id}");
                    items.RemoveAt(pageSize);
                }

                var result = new PagedResult<User>
                {
                    Items      = items,
                    NextCursor = nextCursor
                };
                return Operation<PagedResult<User>>.Success(result);
            }
            catch (Exception ex)
            {
                return errorStrategyHandler.Fail<PagedResult<User>>(ex);
            }
        }

        private static Expression<Func<User, bool>> BuildIdOrNameFilter(string? id, string? name)
        {
            if (!string.IsNullOrWhiteSpace(id))
            {
                return u => u.Id == id;
            }
                
            if (!string.IsNullOrWhiteSpace(name))
            {
                return u => EF.Functions.Like(u.Name!, $"%{name}%");
            }

            return u => true;
        }

        private static Expression<Func<User, bool>> BuildCursorFilter(string lastName, string lastId) =>
            u => u.Name!.CompareTo(lastName) > 0
              || (u.Name == lastName && string.Compare(u.Id, lastId, StringComparison.Ordinal) > 0);
    }
}

