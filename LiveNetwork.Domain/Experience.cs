using System.Diagnostics.CodeAnalysis;
using Domain;

namespace LiveNetwork.Domain
{
    using System.ComponentModel.DataAnnotations;

    /// <summary>
    /// Represents a professional work experience at a company.
    /// </summary>
    [method: SetsRequiredMembers]
    public sealed class Experience(string id) : Entity(id)
    {

        public string ProfileId { get; private set; } = string.Empty; // FK
        public Profile? Profile { get; private set; }

        [Required(ErrorMessage = "Company is required.")]
        [MinLength(2)]
        [MaxLength(200)]
        public string Company { get; set; } = string.Empty;

        [Url]
        [MaxLength(500)]
        public string CompanyUrl { get; set; } = string.Empty;

        [Url]
        [MaxLength(500)]
        public string CompanyLogoUrl { get; set; } = string.Empty;

        [MaxLength(200)]
        public string CompanyLogoAlt { get; set; } = string.Empty;

        [MaxLength(150)]
        public string EmploymentSummary { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        public List<ExperienceRole> Roles { get; set; } = [];
        public void AddRole(ExperienceRole role)
        {
            ArgumentNullException.ThrowIfNull(role);
            Roles.Add(role);
        }
    }
}
