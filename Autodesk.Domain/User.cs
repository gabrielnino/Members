using Domain;
using Domain.Interfaces.Entity;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Autodesk.Domain
{
    // <summary>
    /// Represents a user.
    /// </summary>
    [method: SetsRequiredMembers]    // <summary>
    /// Represents a user.
    /// </summary>
    public class User(string id) : Entity(id)
    {

        /// <summary>
        /// Gets or sets the user's display name.
        /// </summary>
        [Required(ErrorMessage = "Name is required.")]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters long.")]
        [MaxLength(100, ErrorMessage = "Name must be maximun 100 characters long.")]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the user's display lastname.
        /// </summary>
        [Required(ErrorMessage = "Lastname is required.")]
        [MinLength(3, ErrorMessage = "Lastname must be at least 3 characters long.")]
        [MaxLength(100, ErrorMessage = "Lastname must be maximun 100 characters long.")]
        public string? Lastname { get; set; }

    }
}
