using Autodesk.Shared.Pagination;
using System.ComponentModel.DataAnnotations;

namespace Autodesk.Shared.Models
{
    public class InvoiceQueryParams
    {
        /// <summary>
        /// Filter by exact invoice number.
        /// </summary>
        [StringLength(50, ErrorMessage = "InvoiceNumber cannot exceed 50 characters.")]
        public string? InvoiceNumber { get; set; }

        /// <summary>
        /// Filter by partial customer name.
        /// </summary>
        [StringLength(200, ErrorMessage = "CustomerName cannot exceed 200 characters.")]
        public string? CustomerName { get; set; }

        /// <summary>
        /// Cursor for pagination.
        /// </summary>
        public string? Cursor { get; set; }

        /// <summary>
        /// Number of items per page.
        /// </summary>
        [Range(1, PaginationDefaults.MaxPageSize,
            ErrorMessage = "PageSize must be between 1 and " + nameof(PaginationDefaults.MaxPageSize))]
        public int PageSize { get; set; } = PaginationDefaults.DefaultPageSize;

        public bool IncludeProducts { get; set; }
    }
}
