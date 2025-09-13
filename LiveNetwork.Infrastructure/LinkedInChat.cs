using Configuration;
using LiveNetwork.Application.Services;
using LiveNetwork.Domain;
using LiveNetwork.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;

namespace LiveNetwork.Infrastructure.Services
{
    public class LinkedInChat : ILinkedInChat
    {
        private readonly AppConfig _config;
        private readonly IWebDriver _driver;
        private readonly ILogger<InviteConnections> _logger;
        //private readonly ICaptureSnapshot _capture;
        private readonly ExecutionTracker _executionOptions;
        private readonly ITrackingService _trackingService;
        private readonly ILoginService _loginService;
        //const string ExecutionFolder = "Invite";
        //const string Stage = "Send";
        private readonly IUtil _util;
        public LinkedInChat(AppConfig config,
          IWebDriverFactory driverFactory,
            ILogger<InviteConnections> logger,
            ICaptureSnapshot capture,
            ExecutionTracker executionOptions,
            ITrackingService trackingService,
            ILoginService loginService,
            IUtil util)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _driver = driverFactory?.Create(true) ?? throw new ArgumentNullException(nameof(driverFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            //_capture = capture ?? throw new ArgumentNullException(nameof(capture));
            _executionOptions = executionOptions ?? throw new ArgumentNullException(nameof(executionOptions));
            _trackingService = trackingService ?? throw new ArgumentNullException(nameof(trackingService));
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
            _util = util;
        }

        public async Task SendMessageAsync()
        {
            var results = await _trackingService.LoadCollectorConnectionsAsync("Connections_Collected.json") ?? [];
            var threadsPath = _config.Paths.ConversationOutputFilePath;
            var threads = await _trackingService.LoadConversationThreadAsync(threadsPath) ?? [];
            var lastProcessedUtc = await _trackingService.LoadLastProcessedDateUtcAsync("Connections_LastProcessedUtc.txt");
            var threadByUrl = new Dictionary<string, ConversationThread>(StringComparer.OrdinalIgnoreCase);
            var addedMessages = 0;
            var unmatched = 0;
            var newestProcessedUtc = lastProcessedUtc;

            foreach (var t in threads)
            {
                var url = NormalizeUrlString(GetProfileUrlFromThread(t));
                if (!string.IsNullOrEmpty(url) && !threadByUrl.ContainsKey(url))
                {
                    threadByUrl[url] = t;
                }
            }


            var candidates = results
                .Where(c => c.ProfileUrl != null && c.ConnectedOn.HasValue)
                .Select(c => new { Url = NormalizeUrlString(c.ProfileUrl), WhenUtc = ToUtcSafe(c.ConnectedOn!.Value), Raw = c })
                .Where(x => !string.IsNullOrEmpty(x.Url) && x.WhenUtc > lastProcessedUtc)
                .OrderBy(x => x.WhenUtc)
                .ToList();

            if (candidates.Count == 0)
            {
                _logger.LogInformation("No new connections to process. LastProcessedUtc={LastProcessedUtc:o}", lastProcessedUtc);
                return;
            }
            const string experimentTag = "AutoWelcome";
            foreach (var c in candidates)
            {
                if (!threadByUrl.TryGetValue(c.Url!, out var thread))
                {
                    unmatched++;
                    _logger.LogDebug("No existing thread found for URL: {Url}. Skipping welcome message.", c.Url);
                    continue;
                }
                var content = BuildWelcomeMessage(thread);
                var message = new Message(content, experimentTag, MessageStatus.Draft);
                thread.AddCommunication(message);
                addedMessages++;
                _logger.LogInformation("Added welcome draft message for {Url}.", c.Url);


                if (c.WhenUtc > newestProcessedUtc)
                {
                    newestProcessedUtc = c.WhenUtc;
                    await _trackingService.SaveLastProcessedDateUtcAsync("Connections_LastProcessedUtc.txt", newestProcessedUtc);
                }

                //SaveConversationThreadAsync
            }
        }



        static DateTime ToUtcSafe(DateTime dt)
            => dt.Kind switch
            {
                DateTimeKind.Utc => dt,
                DateTimeKind.Local => dt.ToUniversalTime(),
                _ => DateTime.SpecifyKind(dt, DateTimeKind.Utc)
            };

        static string NormalizeUrlString(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return string.Empty;

            try
            {
                var u = new Uri(url, UriKind.Absolute);
                var builder = new UriBuilder(u) { Query = string.Empty, Fragment = string.Empty };
                var normalized = builder.Uri.ToString().TrimEnd('/').ToLowerInvariant();
                return normalized;
            }
            catch
            {
                // If it's not a valid absolute URI, fallback to trimmed/lower
                return url.Trim().TrimEnd('/').ToLowerInvariant();
            }
        }
        static string NormalizeUrlString(Uri? uri) => uri is null ? string.Empty : NormalizeUrlString(uri.ToString());
        static string? GetProfileUrlFromThread(ConversationThread t) => t?.TargetProfile?.Url?.ToString();

        static string BuildWelcomeMessage(ConversationThread thread)
        {
            // Keep it simple and neutral; personalize lightly if you have name fields on LinkedInProfile
            return "Thanks for connecting! 👋\n\n" +
                   "Great to have you in my network. If there’s anything you’re building or exploring that I can help with—especially automation/AI workflows—happy to share ideas or resources.\n\n" +
                   "Welcome aboard!";
        }
    }
}