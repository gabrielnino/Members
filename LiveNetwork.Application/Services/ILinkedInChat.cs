using LiveNetwork.Domain;

namespace LiveNetwork.Application.Services
{
    public interface ILinkedInChat
    {
        Task SendMessageAsync(List<ConnectionInfo>? connections = null);
    }
}
