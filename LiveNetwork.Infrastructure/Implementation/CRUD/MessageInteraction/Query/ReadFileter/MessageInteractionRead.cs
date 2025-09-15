using System;
using System.Linq.Expressions;
using Application.Common.Pagination;
using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Infrastructure.Repositories.Abstract.CRUD.Query.Read;
using LiveNetwork.Application.UseCases.CRUD.Profile.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using Persistence.Context.Implementation;
using Persistence.Context.Interface;

namespace LiveNetwork.Infrastructure.Implementation.CRUD.MessageInteraction.Query.ReadFilter
{
    using static OpenQA.Selenium.PrintOptions;
    using InteractionStatus = Domain.InteractionStatus;
    using MessageInteraction = Domain.MessageInteraction;

    /// <summary>
    /// Read-side repository for MessageInteraction with cursor-based pagination and cache.
    /// Default ordering: CreatedAt ASC, then Id ASC.
    /// </summary>
    public class MessageInteractionRead(
        IUnitOfWork unitOfWork,
        IErrorHandler errorHandler,
        IMemoryCache cache,
        IErrorLogCreate errorLogCreate)
        : ReadRepository<MessageInteraction>(unitOfWork, q => q.OrderBy(i => i.CreatedAt).ThenBy(i => i.Id)), IMessageInteractionRead
    {
        private readonly IErrorHandler _errorHandler = errorHandler;
        private readonly IMemoryCache _cache = cache;

        // Cursor is based on (CreatedAt, Id)
        private readonly Func<MessageInteraction, (DateTimeOffset Primary, string Secondary)> _cursorSelector
            = i => (i.CreatedAt, i.Id);

        private static CancellationTokenSource _interactionCacheTokenSource = new();

        private static Expression<Func<MessageInteraction, bool>> BuildIdFilter(string id)   => i => i.Id == id;

  
        public async Task<Operation<PagedResult<MessageInteraction>>> GetMessageInteractionPageAsync(string? id, string? cursor, int pageSize)
        {
            try
            {
                var cacheKey = $"MessageInteraction:{id}";
                if (cache.TryGetValue(cacheKey, out PagedResult<MessageInteraction> cached))
                {
                    return Operation<PagedResult<MessageInteraction>>.Success(cached);
                }
                var result = await GetPageAsync(BuildIdFilter(id), cursor, pageSize);
                var pagedResult = result.Data;


                var cacheOptions = new MemoryCacheEntryOptions()
                    .AddExpirationToken(new CancellationChangeToken(_interactionCacheTokenSource.Token))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

                cache.Set(cacheKey, pagedResult, cacheOptions);

                return Operation<PagedResult<MessageInteraction>>.Success(pagedResult);
            }
            catch (Exception ex)
            {
                return errorHandler.Fail<PagedResult<MessageInteraction>>(ex, errorLogCreate);
            }
        }

        protected override IQueryable<MessageInteraction> ApplyCursorFilter(IQueryable<MessageInteraction> query, string cursor)
        {
            var parts = Uri.UnescapeDataString(cursor).Split('|', 2);
            var createdStr = parts[0];
            var lastId = parts.Length > 1 ? parts[1] : string.Empty;

            // Parse ISO 8601
            DateTimeOffset createdCursor;
            if (!DateTimeOffset.TryParse(createdStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out createdCursor))
            {
                // If parsing fails, return original query to avoid hiding results
                return query;
            }

            return query.Where(i =>
                i.CreatedAt > createdCursor ||
                (i.CreatedAt == createdCursor && DataContext.StringCompareOrdinal(i.Id, lastId) > 0)
            );
        }

        protected override string? BuildNextCursor(List<MessageInteraction> items, int size)
        {
            if (items.Count <= size) return null;
            var extra = items[size];
            var (createdAt, id) = _cursorSelector(extra);
            return Uri.EscapeDataString($"{createdAt:O}|{id}");
        }
    }
}
