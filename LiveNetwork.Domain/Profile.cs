using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Domain;

namespace LiveNetwork.Domain
{
    [method: SetsRequiredMembers]
    public sealed class Profile(string id) : Entity(id)
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MinLength(3)]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [MaxLength(300)]
        public string Headline { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Location { get; set; } = string.Empty;

        [MaxLength(200)]
        public string CurrentCompany { get; set; } = string.Empty;

        [Url]
        [MaxLength(500)]
        public string ProfileImageUrl { get; set; } = string.Empty;

        [Url]
        [MaxLength(500)]
        public string BackgroundImageUrl { get; set; } = string.Empty;

        [MaxLength(50)]
        public string ConnectionDegree { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Connections { get; set; } = string.Empty;

        [MaxLength(50)]
        public string Followers { get; set; } = string.Empty;

        [MaxLength(5000)]
        public string AboutText { get; set; } = string.Empty;

        [Required]
        public required Uri Url { get; set; }

        public List<Experience> Experiences { get; private set; } = [];
        public List<Education> Educations { get; private set; } = [];
        public List<Interaction> Communications { get; private set; } = [];

        public bool HasActivity => Communications.Count != 0;

        public ConnectionInvite? GetInitialInvite() =>
            Communications.OfType<ConnectionInvite>().FirstOrDefault(i => i.Status == ConnectionStatus.Draft);

        public string AddInvite(ConnectionInvite invite)
        {
            ArgumentNullException.ThrowIfNull(invite);
            return AddCommunication(invite);
        }

        public string AddMessage(MessageInteraction message)
        {
            ArgumentNullException.ThrowIfNull(message);
            return AddCommunication(message);
        }

        private string AddCommunication(Interaction communication)
        {
            ArgumentNullException.ThrowIfNull(communication);

            if (string.IsNullOrWhiteSpace(communication.Id))
                throw new InvalidOperationException("Communication must have a non-empty Id.");

            Communications.Add(communication);
            return communication.Id;
        }

        // Convenience helpers
        public void AddExperience(Experience experience)
        {
            ArgumentNullException.ThrowIfNull(experience);
            Experiences.Add(experience);
        }

        public void AddEducation(Education education)
        {
            ArgumentNullException.ThrowIfNull(education);
            Educations.Add(education);
        }
    }
}
