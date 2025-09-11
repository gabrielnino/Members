namespace LiveNetwork.Domain
{
    public sealed class ProcessedProfileRecord
    {
        public required string Url { get; set; }
        public string? FullName { get; set; }
        public string? SavedJsonPath { get; set; }
        public DateTime SavedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
