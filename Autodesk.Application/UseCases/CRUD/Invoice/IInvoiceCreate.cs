using Application.Result;

namespace Autodesk.Application.UseCases.CRUD.Invoice
{
    using Invoice = Domain.Invoice;
    public interface IInvoiceCreate
    {
        Task<Operation<Invoice>> CreateInvoiceAsync(Invoice entity);
    }
}
