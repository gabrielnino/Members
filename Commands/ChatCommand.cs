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
    public class ChatCommand : ICommand
    {
        private readonly ILogger<ChatCommand> _logger;
        private readonly ILinkedInChat _iLinkedInChat;

        public ChatCommand(ILogger<ChatCommand> logger, ILinkedInChat iLinkedInChat)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _iLinkedInChat = iLinkedInChat ?? throw new ArgumentNullException(nameof(iLinkedInChat));
        }

        public async Task ExecuteAsync(Dictionary<string, string>? arguments = null)
        {
            _logger.LogInformation("ChatCommand: starting. args={@Args}", arguments);
            await _iLinkedInChat.SendMessageAsync();
            _logger.LogInformation("ChatCommand: finished.");
        }
    }
}
