using Domain;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Autodesk.Domain
{
    [method: SetsRequiredMembers]
    public class Product(string id) : Entity(id)
    {
        [Required(ErrorMessage = "Product name is required.")]
        [MinLength(2, ErrorMessage = "Product name must be at least 2 characters long.")]
        [MaxLength(100, ErrorMessage = "Product name must be maximum 100 characters long.")]
        public string? Name { get; set; }

        [MaxLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }
    }
}
