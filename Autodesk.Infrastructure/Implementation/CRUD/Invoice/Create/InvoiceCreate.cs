using Application.Result;
using Autodesk.Application.UseCases.CRUD.Invoice;
using Infrastructure.Repositories.Abstract.CRUD.Create;
using Persistence.Context.Interface;

namespace Autodesk.Infrastructure.Implementation.CRUD.Invoice.Create
{
    using Invoice = Domain.Invoice;

    public class InvoiceCreate(
            IUnitOfWork unitOfWork,
            IErrorHandler errorHandler
        ) : CreateRepository<Invoice>(unitOfWork), IInvoiceCreate
    {
        private readonly IErrorHandler _errorHandler = errorHandler;

        public async Task<Operation<Invoice>> CreateInvoiceAsync(Invoice entity)
        {
            try
            {
                // Persist the invoice
                await CreateEntity(entity);
                await unitOfWork.CommitAsync();

                // Return the created entity
                return Operation<Invoice>.Success(entity);
            }
            catch (Exception ex)
            {
                // Delegate to the error handler
                return _errorHandler.Fail<Invoice>(ex);
            }
        }
    }
}
