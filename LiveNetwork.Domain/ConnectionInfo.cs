namespace LiveNetwork.Domain
{
    public sealed class ConnectionInfo
    {
        public Uri? ProfileUrl { get; set; }
        public string? TitleOrPosition { get; set; }
        public DateTime? ConnectedOn { get; set; }
    }
}
