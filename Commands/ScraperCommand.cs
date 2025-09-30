using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveNetwork.Application.Services;
using LiveNetwork.Infrastructure.Services;
using Microsoft.Extensions.Logging;

namespace Commands
{
    public class ScraperCommand : ICommand
    {
        private readonly ILogger<IScraperService> _logger;
        private readonly IScraperService _reviewScraperService;
        public ScraperCommand(ILogger<IScraperService> logger, IScraperService reviewScraperService)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _reviewScraperService = reviewScraperService ?? throw new ArgumentNullException(nameof(reviewScraperService));
        }

        public async Task ExecuteAsync(Dictionary<string, string>? arguments = null)
        {
            _logger.LogInformation("InviteCommand: starting. args={@Args}", arguments);
            await _reviewScraperService.ScrapeAsync();
            _logger.LogInformation("InviteCommand: finished.");
        }
    }
}
