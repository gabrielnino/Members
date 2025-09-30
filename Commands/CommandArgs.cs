namespace Commands
{
    public class CommandArgs
    {
        public const string search = "--search";
        public const string prompt = "--prompt";
        public const string invite = "--invite";
        public const string load = "--load";
        public const string chat = "--chat";
        public const string scrape = "--scrape-reviews";

        private static readonly HashSet<string> ValidCommands = new(StringComparer.OrdinalIgnoreCase)
        {
            search,
            prompt,
            invite,
            load,
            chat,
            scrape
        };

        public string MainCommand { get; }
        public Dictionary<string, string> Arguments { get; }

        public CommandArgs(string[] args)
        {
            if (args == null || args.Length == 0)
            {
                // Workaround: no arguments passed
                MainCommand = string.Empty;
                Arguments = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            MainCommand = args.FirstOrDefault(IsCommand) ?? args.FirstOrDefault(IsArgument).Split("=").FirstOrDefault();
            Arguments = args
                .Where(IsArgument)
                .Select(arg =>
                {
                    var parts = arg.Split('=', 2);
                    var key = parts[0];
                    var value = parts.Length > 1 ? parts[1] : string.Empty;
                    return new KeyValuePair<string, string>(key, value);
                })
                .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase);
        }

        private static bool IsCommand(string arg) => ValidCommands.Contains(arg);

        private static bool IsArgument(string arg) => arg.Contains("=");
    }
}
