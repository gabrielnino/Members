using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;

namespace LiveNetwork.Domain
{
    public class ConversationThread(LinkedInProfile targetProfile)
    {

        public LinkedInProfile TargetProfile { get; } = targetProfile ?? throw new ArgumentNullException(nameof(targetProfile));

        public List<Communication> Communications { get; set; } = [];

        public bool HasActivity => Communications is not null && Communications.Count != 0;

        public Communication? GetInitialInvite() =>
           Communications.FirstOrDefault(i => IsInvite(i));

        private static bool IsInvite(Communication i)
        {
            return i.TypeName == nameof(Invite) && i.Status == InviteStatus.Draft.ToString();
        }

        public string AddInvite(Invite invite)
        {
            if (Communications.OfType<Invite>().Any())
                throw new InvalidOperationException("An initial invite already exists.");
            return AddCommunication(invite);
        }

        private static string GenerateHashId(Communication c, string? salt = null)
        {
            // nonce asegura unicidad incluso con mismo contenido/fecha
            var nonce = salt ?? Guid.NewGuid().ToString("N");
            var payload = $"{c.GetType().Name}|{c.CreateDate.ToUniversalTime():O}|{c.Content}|{nonce}";
            var bytes = Encoding.UTF8.GetBytes(payload);

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(bytes);

            // Trunca a 16 bytes (128 bits) -> 32 chars hex. Suficiente y legible.
            return Convert.ToHexString(hash, 0, 16); // mayúsculas A-F
        }

        public string AddCommunication(Communication c)
        {
            ArgumentNullException.ThrowIfNull(c);
            if (c.CreateDate == default) c.CreateDate = DateTime.UtcNow;

            // Genera Id solo si no existe (p. ej. al rehidratar desde disco)
            if (string.IsNullOrWhiteSpace(c.Id))
            {
                c.Id = GenerateHashId(c);
            }

            Communications.Add(c);
            return c.Id;
        }

        public bool TryUpdateComunicationStatus(string inviteId, InviteStatus newStatus)
        {
            if (string.IsNullOrWhiteSpace(inviteId)) { return false; }
            var invite = Communications.FirstOrDefault(i => string.Equals(i.Id, inviteId, StringComparison.OrdinalIgnoreCase));
            if (invite is null) { return false; }
            invite.Status = newStatus.ToString();
            return true;
        }
    }

    public class Communication
    {
        [JsonInclude] public string Id { get; internal set; } = string.Empty;
        [JsonInclude] public string Content { get; private set; } = string.Empty;
        [JsonInclude] public DateTime CreateDate { get; internal set; }
        [JsonInclude] public string? FeedbackNotes { get; set; }
        [JsonInclude] public string Experiment { get; set; } = string.Empty;
        [JsonInclude] public string Status { get; set; } = string.Empty;
        [JsonInclude] public string TypeName { get; set; } = string.Empty;

        // 👇 Necesario para STJ cuando no hay polimorfismo ni JsonConstructor
        public Communication() { }

        protected Communication(string content, string experiment, string status, string typeName = "Communication")
        {
            Content = string.IsNullOrWhiteSpace(content)
                ? throw new ArgumentException("Content cannot be empty.", nameof(content))
                : content;
            Experiment = experiment;
            Status = status;
            TypeName = typeName;
        }
    }

    public sealed class Invite : Communication
    {
        public Invite(string content, string experiment, InviteStatus status = InviteStatus.Draft)
            : base(content, experiment, status.ToString(), nameof(Invite)) { }
    }

    public sealed class Message : Communication
    {
        public Message(string content, string experiment, MessageStatus status = MessageStatus.Draft)
            : base(content, experiment, status.ToString(), nameof(Message)) { }
    }

    public enum InviteStatus { Draft, Sent, Accepted, Ignored, Withdrawn }
    public enum MessageStatus { Draft, Sent, Delivered, Read, Failed }
}
