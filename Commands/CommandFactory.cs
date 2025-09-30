using Microsoft.Extensions.DependencyInjection;
using Services;

namespace Commands
{
    public class CommandFactory
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly CommandArgs _jobCommandArgs;

        public CommandFactory(IServiceProvider serviceProvider, CommandArgs jobCommandArgs)
        {
            _serviceProvider = serviceProvider;
            _jobCommandArgs = jobCommandArgs;
        }

        public IEnumerable<ICommand> CreateCommand()
        {
            var commands = new List<ICommand>();

            switch (_jobCommandArgs.MainCommand.ToLowerInvariant())
            {
                case CommandArgs.invite:
                    commands.Add(_serviceProvider.GetRequiredService<InviteCommand>());
                    break;
                case CommandArgs.search:
                    commands.Add(_serviceProvider.GetRequiredService<SearchCommand>());
                    break;
                case CommandArgs.prompt:
                    commands.Add(_serviceProvider.GetRequiredService<PromtCommand>());
                    break;
                case CommandArgs.load:
                    commands.Add(_serviceProvider.GetRequiredService<CollectorCommand>());
                    break;
                case CommandArgs.chat:
                    commands.Add(_serviceProvider.GetRequiredService<ChatCommand>());
                    break;
                case CommandArgs.scrape:
                    commands.Add(_serviceProvider.GetRequiredService<ScraperCommand>());
                    break;
                default:
                    commands.Add(_serviceProvider.GetRequiredService<HelpCommand>());
                    break;
            }

            return commands;
        }
    }
}

