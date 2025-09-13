using System.Linq.Expressions;
using Application.Common.Pagination;
using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Infrastructure.Repositories.Abstract.CRUD.Query.Read;
using LiveNetwork.Application.UseCases.CRUD.Profile.Query;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Persistence.Context.Implementation;
using Persistence.Context.Interface;
using Microsoft.EntityFrameworkCore;


namespace LiveNetwork.Infrastructure.Implementation.CRUD.Profile.Query.ReadFilter
{
    using Profile = Domain.Profile;
    public class ProfileRead(IUnitOfWork unitOfWork, IErrorHandler errorHandler, IMemoryCache cache, IErrorLogCreate errorLogCreate) : ReadRepository<Profile>(unitOfWork, q => q.OrderBy(u => u.FullName!).ThenBy(u => u.Id)), IProfileRead
    {
        private readonly IErrorHandler errorHandler = errorHandler;
        private readonly IMemoryCache cache = cache;
        private readonly Func<Profile, (string Primary, string Secondary)> cursorSelector = u => (u.FullName!, u.Id);
        private static CancellationTokenSource _ProfileCacheTokenSource = new();


        /// <summary>
        /// Fetch a page of Profiles filtered by id or name, with cursor-based paging and caching.
        /// </summary>
        /// <param name="id">Optional Profile ID filter.</param>
        /// <param name="name">Optional Profile name filter.</param>
        /// <param name="cursor">Cursor for the next page.</param>
        /// <param name="pageSize">Number of items per page.</param>
        /// <returns>Operation result with paged Profiles or error.</returns>
        public async Task<Operation<PagedResult<Profile>>> GetProfilesPageAsync(
            string? id,
            string? name,
            string? cursor,
            int pageSize)
        {
            try
            {
                var cacheKey = $"Profiles:{id}:{name}:{cursor}:{pageSize}";
                if (cache.TryGetValue(cacheKey, out PagedResult<Profile> cached))
                {
                    return Operation<PagedResult<Profile>>.Success(cached);
                }
                var result = await GetPageAsync(BuildFilter(id, name), cursor, pageSize);
                var pagedResult = result.Data;


                var cacheOptions = new MemoryCacheEntryOptions()
                    .AddExpirationToken(new CancellationChangeToken(_ProfileCacheTokenSource.Token))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                cache.Set(cacheKey, pagedResult, cacheOptions);

                return Operation<PagedResult<Profile>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                return errorHandler.Fail<PagedResult<Profile>>(ex, errorLogCreate);
            }
        }

        /// <summary>
        /// Choose filter by id or name, or none.
        /// </summary>
        private static Expression<Func<Profile, bool>> BuildFilter(string? id, string? name)
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
        /// Filter Profiles by id.
        /// </summary>
        private static Expression<Func<Profile, bool>> BuildIdFilter(string id) => u => u.Id == id;

        /// <summary>
        /// Filter Profiles by name pattern.
        /// </summary>
        private static Expression<Func<Profile, bool>> BuildNameFilter(string name) => u => EF.Functions.Like(u.FullName!, $"%{name}%");

        /// <summary>
        /// No filter: return all Profiles.
        /// </summary>
        private static Expression<Func<Profile, bool>> ReturnDefaultFilter() => u => true;

        protected override IQueryable<Profile> ApplyCursorFilter(IQueryable<Profile> query, string cursor)
        {
            var parts = Uri.UnescapeDataString(cursor).Split('|', 2);
            var name = parts[0];
            var lastS = parts.Length > 1 ? parts[1] : string.Empty;

            return query.Where
            (
                u => DataContext.StringCompareOrdinal(u.FullName!, name) > 0 || (u.FullName == name && DataContext.StringCompareOrdinal(u.Id, lastS) > 0)
            );
        }

        protected override string? BuildNextCursor(List<Profile> items, int size)
        {
            if (items.Count <= size) return null;
            var extra = items[size];
            var (p, s) = cursorSelector(extra);
            return Uri.EscapeDataString($"{p}|{s}");
        }

        public void InvalidateAllProfileCache()
        {
            // Disparamos el token actual, invalidando todo lo cacheado con él
            _ProfileCacheTokenSource.Cancel();
            // Creamos un nuevo CancellationTokenSource para futuras operaciones
            _ProfileCacheTokenSource = new CancellationTokenSource();
        }

        public List<Profile> GetStreamProfiles(CancellationToken cancellationToken = default)
        {
            var result = GetAllMembers(cancellationToken).Result;
            return [.. result.Data.Items];
        }
    }
}
