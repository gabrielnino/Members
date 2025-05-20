using Application.Common.Pagination;
using Application.Result;
using Autodesk.Application.UseCases.CRUD.Invoice.Query;
using Infrastructure.Repositories.Abstract.CRUD.Query.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Persistence.Context.Interface;
using System.Linq.Expressions;

namespace Autodesk.Infrastructure.Implementation.CRUD.Invoice.Query.ReadFilter
{
    using Invoice = Domain.Invoice;

    /// <summary>
    /// Reads invoices with optional filtering, cursor-based paging, and caching.
    /// </summary>
    public class InvoiceRead(
        IUnitOfWork unitOfWork,
        IErrorHandler errorHandler,
        IMemoryCache cache
    ) : ReadRepository<Invoice>(
            unitOfWork,
            // Order first by InvoiceDate, then by Id to disambiguate
            q => q.Include(i => i.Products).OrderBy(i => i.InvoiceDate).ThenBy(i => i.Id)
        ), IInvoiceRead
    {
        private readonly IErrorHandler _errorHandler = errorHandler;
        private readonly IMemoryCache _cache = cache;
        // Use ISO-8601 date strings so lexical compare matches chronological
        private readonly Func<Invoice, (string Primary, string Secondary)> _cursorSelector
            = i => (i.InvoiceDate.ToString("o"), i.Id);

        /// <summary>
        /// Get a page of invoices, optionally filtered by invoiceNumber or customerName.
        /// </summary>
        public async Task<Operation<PagedResult<Invoice>>> GetInvoicesPageAsync(
            string? invoiceNumber,
            string? customerName,
            string? cursor,
            int pageSize)
        {
            try
            {
                var cacheKey = $"invoices:{invoiceNumber}:{customerName}:{cursor}:{pageSize}";
                if (_cache.TryGetValue(cacheKey, out PagedResult<Invoice> cached))
                    return Operation<PagedResult<Invoice>>.Success(cached);

                var filter = BuildFilter(invoiceNumber, customerName);
                var result = await GetPageAsync(filter, cursor, pageSize);
                var paged = result.Data;

                _cache.Set(cacheKey, paged, TimeSpan.FromMinutes(5));
                return Operation<PagedResult<Invoice>>.Success(paged);
            }
            catch (Exception ex)
            {
                return _errorHandler.Fail<PagedResult<Invoice>>(ex);
            }
        }

        private static Expression<Func<Invoice, bool>> BuildFilter(
            string? invoiceNumber,
            string? customerName)
        {
            if (!string.IsNullOrWhiteSpace(invoiceNumber))
                return i => i.InvoiceNumber == invoiceNumber!;

            if (!string.IsNullOrWhiteSpace(customerName))
                return i => EF.Functions.Like(i.CustomerName!, $"%{customerName}%");

            return i => true;
        }

        protected override IQueryable<Invoice> ApplyCursorFilter(
            IQueryable<Invoice> query,
            string cursor)
        {
            var parts = Uri.UnescapeDataString(cursor).Split('|', 2);
            var dateString = parts[0];
            var lastId = parts.Length > 1 ? parts[1] : string.Empty;

            return query.Where(i =>
                // Primary compare on date string
                Persistence.Context.Implementation.DataContext
                    .StringCompareOrdinal(i.InvoiceDate.ToString("o"), dateString) > 0
                // If equal date, compare by Id
                || (i.InvoiceDate.ToString("o") == dateString
                    && Persistence.Context.Implementation.DataContext
                        .StringCompareOrdinal(i.Id, lastId) > 0)
            );
        }

        protected override string? BuildNextCursor(
            List<Invoice> items,
            int size)
        {
            if (items.Count <= size) return null;
            var extra = items[size];
            var (p, s) = _cursorSelector(extra);
            return Uri.EscapeDataString($"{p}|{s}");
        }

        protected override IQueryable<Invoice> BuildBaseQuery(Expression<Func<Invoice, bool>>? filter)
        {
            var q = base.BuildBaseQuery(filter);
            return q.Include(i => i.Products);
        }
    }
}
