using LiveNetwork.Application.Services;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace Commands
{
    public class PromtCommand(
        ILogger<PromtCommand> logger,
        IPromptGenerator promptGenerator) : ICommand
    {

        private readonly ILogger<PromtCommand> _logger = logger;
        private readonly IPromptGenerator _promptGenerator = promptGenerator;

        public async Task ExecuteAsync(Dictionary<string, string>? arguments = null)
        {
            _logger.LogInformation("Starting job application process...");
            await _promptGenerator.GeneratPrompt();
        }

    }
}
