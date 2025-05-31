using Application.Common.Pagination;
using Autodesk.Application.UseCases.CRUD.Invoice;
using Autodesk.Application.UseCases.CRUD.Invoice.Query;
using Autodesk.Domain;
using Autodesk.Shared.Models;
using Microsoft.AspNetCore.Mvc;

namespace Autodesk.Api.Controllers.api.v1.Autodesk
{
    [ApiController]
    [Route("api/v1/invoices")]
    public class InvoiceController(IInvoiceCreate invoiceCreate, IInvoiceRead invoiceRead) : ControllerBase
    {
        [HttpPost]
        [ProducesResponseType(typeof(Invoice), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] Invoice invoice)
        {
            var op = await invoiceCreate.CreateInvoiceAsync(invoice);
            if (!op.IsSuccessful)
                return BadRequest(op.Message);

            return CreatedAtAction(nameof(Create), new { id = invoice.Id }, invoice);
        }

        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<Invoice>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get([FromQuery] InvoiceQueryParams qp)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var op = await invoiceRead.GetInvoicesPageAsync(
                qp.InvoiceNumber,
                qp.CustomerName,
                qp.Cursor,
                qp.PageSize,
                qp.IncludeProducts
            );

            if (!op.IsSuccessful)
                return BadRequest(op.Message);

            return Ok(op.Data);
        }
    }
}
