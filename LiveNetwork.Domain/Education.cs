using System.Diagnostics.CodeAnalysis;
using Domain;

namespace LiveNetwork.Domain
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents an education record.
    /// </summary>
    [method: SetsRequiredMembers]
    public sealed class Education(string id) : Entity(id)
    {
        public string ProfileId { get; private set; } = string.Empty; // FK
        public Profile? Profile { get; private set; }
        [Required(ErrorMessage = "School is required.")]
        [MinLength(2)]
        [MaxLength(200)]
        public string School { get; set; } = string.Empty;

        [Url]
        [MaxLength(500)]
        public string SchoolUrl { get; set; } = string.Empty;

        [Url]
        [MaxLength(500)]
        public string LogoUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string LogoAlt { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Degree { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Field { get; set; } = string.Empty;

        [MaxLength(100)]
        public string DateRange { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;
    }
}
