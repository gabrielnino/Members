using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LiveNetwork.Application.Services;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Commands
{
    public class InviteCommand : ICommand
    {
        private readonly ILogger<InviteCommand> _logger;
        private readonly IInviteConnections _iInviteCommand;

        public InviteCommand(ILogger<InviteCommand> logger, IInviteConnections iInviteCommand)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _iInviteCommand = iInviteCommand ?? throw new ArgumentNullException(nameof(iInviteCommand));
        }

        public async Task ExecuteAsync(Dictionary<string, string>? arguments = null)
        {
            _logger.LogInformation("InviteCommand: starting. args={@Args}", arguments);
            await _iInviteCommand.Invite();
            _logger.LogInformation("InviteCommand: finished.");
        }
    }
}
