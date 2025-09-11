//using Models;

using LiveNetwork.Domain;

namespace LiveNetwork.Application.Services
{
    public interface ITrackingService
    {
        Task<TrackingState> LoadStateAsync();
        Task SavePageStateAsync(TrackingState state);
        Task<List<Uri>> LoadConnectionsFromSearchAsync(string searchUrlOutputFilePath);
        /// <summary>Returns a set of URL strings that have already been processed for this search.</summary>
        Task<ISet<string>> LoadProcessedUrlsAsync(string searchText);
        Task<List<ConversationThread>> LoadConversationThreadAsync(string conversationOutputFilePath);
        Task<List<LinkedInProfile>> LoadDetailedProfilesAsync(string detailedProfilesOutputFilePath);

        /// <summary>Saves the full profile JSON to disk and returns the saved path.</summary>
        Task<string> SaveProfileJsonAsync(string searchText, LinkedInProfile profile, string folderPath);
        Task SaveConnectionsAsync(List<Uri> connections, string searchUrlOutputFilePath);
        Task SaveLinkedInProfilesAsync(List<LinkedInProfile> detailed, string detailedProfilesOutputFilePath);
        Task SaveConversationThreadAsync(List<ConversationThread> threads, string conversationOutputFilePath);
        Task SaveCollectorConnectionsAsync(List<ConnectionInfo> connectionsInfo, string collectorInfoOutputFilePath);

        Task<List<ConnectionInfo>> LoadCollectorConnectionsAsync(string collectorInfoOutputFilePath);

        Task<DateTime> LoadLastProcessedDateUtcAsync(string deltaFileName);
        Task SaveLastProcessedDateUtcAsync(string deltaFileName, DateTime dtUtc);
    }
}
