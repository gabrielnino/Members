using System.Diagnostics.CodeAnalysis;

namespace LiveNetwork.Domain
{
    [method: SetsRequiredMembers]
    public sealed class MessageInteraction(string id, string content, string experiment, InteractionStatus status = InteractionStatus.Draft)
        : Interaction<InteractionStatus>(id, content, experiment, status)
    {
        protected override bool IsAllowedTransition(InteractionStatus from, InteractionStatus to) => (from, to) switch
        {
            (InteractionStatus.Draft, InteractionStatus.Sent) or
            (InteractionStatus.Sent, InteractionStatus.Delivered) or
            (InteractionStatus.Delivered, InteractionStatus.Read) or
            (InteractionStatus.Sent, InteractionStatus.Failed) => true,
            _ => false
        };

        public void Send()
        {
            ChangeStatus(InteractionStatus.Sent);
        }

        public void MarkDelivered()
        {
            ChangeStatus(InteractionStatus.Delivered);
        }

        public void MarkRead()
        {
            ChangeStatus(InteractionStatus.Read);
        }

        public void Fail()
        {
            ChangeStatus(InteractionStatus.Failed);
        }
    }
    public enum InteractionStatus { Draft, Sent, Delivered, Read, Failed }
}
