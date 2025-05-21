using Application.Common.Pagination;
using Application.Result;
using Application.UseCases.Repository.UseCases.CRUD;
using Autodesk.Application.UseCases.CRUD.Invoice.Query;
using Infrastructure.Repositories.Abstract.CRUD.Query.Read;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Persistence.Context.Interface;
using System.Linq.Expressions;

namespace Autodesk.Infrastructure.Implementation.CRUD.Invoice.Query.ReadFilter
{
    using Invoice = Domain.Invoice;

    public class InvoiceRead(
        IUnitOfWork unitOfWork,
        IErrorHandler errorHandler,
        IMemoryCache cache,
        IErrorLogCreate errorLogCreate
    ) : ReadRepository<Invoice>(
            unitOfWork,
            // no Include here—just ordering
            q => q.OrderBy(i => i.InvoiceDate).ThenBy(i => i.Id)
        ), IInvoiceRead
    {
        private readonly IErrorHandler _errorHandler = errorHandler;
        private readonly IMemoryCache _cache = cache;
        private bool _includeProducts;  // ← toggled per call

        // Use ISO-8601 strings so lexical compare follows date order
        private readonly Func<Invoice, (string Primary, string Secondary)> _cursorSelector
            = i => (i.InvoiceDate.ToString("o"), i.Id);

        public async Task<Operation<PagedResult<Invoice>>> GetInvoicesPageAsync(
            string? invoiceNumber,
            string? customerName,
            string? cursor,
            int pageSize,
            bool includeProducts = false)      // ← new parameter
        {
            try
            {
                _includeProducts = includeProducts;

                var cacheKey = $"invoices:{invoiceNumber}:{customerName}:{cursor}:{pageSize}:{includeProducts}";
                if (_cache.TryGetValue(cacheKey, out PagedResult<Invoice> cached))
                {
                    return Operation<PagedResult<Invoice>>.Success(cached);
                }

                var filter = BuildFilter(invoiceNumber, customerName);
                var result = await GetPageAsync(filter, cursor, pageSize);
                var paged = result.Data;

                _cache.Set(cacheKey, paged, TimeSpan.FromMinutes(5));
                return Operation<PagedResult<Invoice>>.Success(paged);
            }
            catch (Exception ex)
            {
                return _errorHandler.Fail<PagedResult<Invoice>>(ex, errorLogCreate);
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
                Persistence.Context.Implementation.DataContext
                    .StringCompareOrdinal(i.InvoiceDate.ToString("o"), dateString) > 0
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

        /// <summary>
        /// Conditionally eager‐loads Products only if the flag was set.
        /// </summary>
        protected override IQueryable<Invoice> BuildBaseQuery(
            Expression<Func<Invoice, bool>>? filter)
        {
            var q = base.BuildBaseQuery(filter);

            if (_includeProducts)
                q = q.Include(i => i.Products);

            return q;
        }
    }
}
