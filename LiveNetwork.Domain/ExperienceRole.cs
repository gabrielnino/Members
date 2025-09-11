using System.Diagnostics.CodeAnalysis;
using Domain;

namespace LiveNetwork.Domain
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents a role within a work experience.
    /// </summary>
    [method: SetsRequiredMembers]
    public sealed class ExperienceRole(string id) : Entity(id)
    {
        public string ExperienceId { get; private set; } = string.Empty; // FK
        public Experience? Experience { get; private set; }

        [Required(ErrorMessage = "Title is required.")]
        [MinLength(2)]
        [MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Date range is required.")]
        [MaxLength(100)]
        public string DateRange { get; set; } = string.Empty;

        [MaxLength(50)]
        public string WorkArrangement { get; set; } = string.Empty;

        [MaxLength(2000)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(200)]
        public string ContextualSkills { get; set; } = string.Empty;
    }
}
