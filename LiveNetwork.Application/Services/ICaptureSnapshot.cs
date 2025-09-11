namespace LiveNetwork.Application.Services
{
    public interface ICaptureSnapshot
    {
        Task<string> CaptureArtifactsAsync(string executionFolder, string stage);
    }
}
