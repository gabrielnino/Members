using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveNetwork.Application.Services;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Commands
{
    public class CollectorCommand : ICommand
    {
        private readonly ILogger<CollectorCommand> _logger;
        private readonly IConnectionInfoCollector _iConnectionInfoCollector;

        public CollectorCommand(ILogger<CollectorCommand> logger, IConnectionInfoCollector iConnectionInfoCollector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _iConnectionInfoCollector = iConnectionInfoCollector ?? throw new ArgumentNullException(nameof(iConnectionInfoCollector));
        }

        public async Task ExecuteAsync(Dictionary<string, string>? arguments = null)
        {
            _logger.LogInformation("InviteCommand: starting. args={@Args}", arguments);
            await _iConnectionInfoCollector.LoadConnectionsAsync();
            _logger.LogInformation("InviteCommand: finished.");
        }
    }
}
