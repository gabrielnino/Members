using Domain;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Autodesk.Domain
{
    [method: SetsRequiredMembers]
    public class Invoice(string id) : Entity(id)
    {
        [Required(ErrorMessage = "Invoice number is required.")]
        [StringLength(50, ErrorMessage = "Invoice number cannot exceed 50 characters.")]
        public string? InvoiceNumber { get; set; }

        [Required(ErrorMessage = "Invoice date is required.")]
        public DateTime InvoiceDate { get; set; }

        [Required(ErrorMessage = "Customer name is required.")]
        [MinLength(3, ErrorMessage = "Customer name must be at least 3 characters long.")]
        [MaxLength(200, ErrorMessage = "Customer name must be maximum 200 characters long.")]
        public string? CustomerName { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Total amount must be non-negative.")]
        public decimal TotalAmount { get; set; }

        public ICollection<Product> Products { get; init; } = [];
    }
}
