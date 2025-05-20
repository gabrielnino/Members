using Application.Common.Pagination;
using Application.Result;

namespace Autodesk.Application.UseCases.CRUD.Invoice.Query
{
    using Invoice = Domain.Invoice;
    public interface IInvoiceRead
    {
        Task<Operation<PagedResult<Invoice>>> GetInvoicesPageAsync(
            string? invoiceNumber,
            string? customerName,
            string? cursor,
            int pageSize);
    }
}
