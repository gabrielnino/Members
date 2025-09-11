using System.Diagnostics.CodeAnalysis;

namespace LiveNetwork.Domain
{
    [method: SetsRequiredMembers]
    public sealed class ConnectionInvite(string id, string content, string experiment, ConnectionStatus status = ConnectionStatus.Draft)
        : Interaction<ConnectionStatus>(id, content, experiment, status)
    {
        public DateTimeOffset? SentAt { get; private set; }
        public DateTimeOffset? CompletedAt { get; private set; }


        // Domain operations (express intent + set timestamps)
        public void Send()
        {
            ChangeStatus(ConnectionStatus.Sent);
        }

        protected override bool IsAllowedTransition(ConnectionStatus from, ConnectionStatus to)
        {
            return (from, to) switch
            {
                (ConnectionStatus.Draft, ConnectionStatus.Sent) or
                (ConnectionStatus.Sent, ConnectionStatus.Accepted) or
                (ConnectionStatus.Sent, ConnectionStatus.Ignored) or
                (ConnectionStatus.Sent, ConnectionStatus.Withdrawn) => true,
                _ => false
            };
        }

        public void Accept()
        {
            ChangeStatus(ConnectionStatus.Accepted);
        }

        public void Ignore()
        {
            ChangeStatus(ConnectionStatus.Ignored);
        }

        public void Withdraw()
        {
            ChangeStatus(ConnectionStatus.Withdrawn);
        }
    }

    public enum ConnectionStatus { Draft, Sent, Accepted, Ignored, Withdrawn }
}
