namespace LiveNetwork.Domain
{
    public class TrackingState
    {
        public int LastProcessedPage { get; set; } = 0;
        public List<Uri> Connections { get; set; } = [];
        public bool IsComplete { get; set; } = false;
    }
}
