using System.Diagnostics.CodeAnalysis;
using Domain;

namespace LiveNetwork.Domain
{
    [method: SetsRequiredMembers]
    public abstract class Interaction(string id) : Entity(id)
    {
        public string ProfileId { get; init; } = string.Empty; // FK
        public Profile? Profile { get; private set; }
        public string Content { get; protected set; } = string.Empty;
        public string Experiment { get; protected set; } = string.Empty;
        public string? FeedbackNotes { get; protected set; }
        public DateTimeOffset CreatedAt { get; protected set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset UpdatedAt { get; protected set; } = DateTimeOffset.UtcNow;
        protected void Touch() => UpdatedAt = DateTimeOffset.UtcNow;

    }

    public abstract class Interaction<TStatus> : Interaction where TStatus : struct, Enum
    {
        [method: SetsRequiredMembers]
        protected Interaction(string id, string content, string experiment, TStatus initialStatus) : base(id)
        {
            Content = content;
            Experiment = experiment;
            Status = initialStatus;
        }

        protected void ChangeStatus(TStatus to)
        {
            if (!IsAllowedTransition(Status, to))
                throw new InvalidOperationException(
                    $"Transition {typeof(TStatus).Name}: {Status} → {to} is not allowed for {GetType().Name}.");

            Status = to;
            Touch(); // updates UpdatedAt timestamp
        }

        protected abstract bool IsAllowedTransition(TStatus from, TStatus to);
        public TStatus Status { get; protected set; }
    }
}
