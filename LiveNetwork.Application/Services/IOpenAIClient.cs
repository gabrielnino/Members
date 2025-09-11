using LiveNetwork.Domain;

namespace Services.Interfaces
{
    public interface IOpenAIClient
    {
        Task<string> GetChatCompletionAsync(Prompt prompt);
    }
}
