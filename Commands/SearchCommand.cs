using LiveNetwork.Application.Services;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Commands
{
    public class SearchCommand(
        ISearchCoordinator linkedInService,
        ILogger<SearchCommand> logger) : ICommand
    {
        private readonly ISearchCoordinator _linkedInService = linkedInService;
        private readonly ILogger<SearchCommand> _logger = logger;

        public async Task ExecuteAsync(Dictionary<string, string>? arguments = null)
        {
            _logger.LogInformation("Starting job search...");
            await _linkedInService.SearchConnectionAsync();
        }
    }

}
