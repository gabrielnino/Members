using Configuration;
using LiveNetwork.Application.Services;
using LiveNetwork.Application.UseCases.CRUD.Profile;
using LiveNetwork.Application.UseCases.CRUD.Profile.Query;
using LiveNetwork.Domain;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;


namespace LiveNetwork.Infrastructure.Services
{
    public class ConnectionInfoCollector : IConnectionInfoCollector
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
        private readonly IProfileCreate _profileCreate;
        private readonly IProfileRead _profileRead;
        private readonly ILinkedInChat _linkedInChat;

        public ConnectionInfoCollector(
            AppConfig config,
            IWebDriverFactory driverFactory,
            ILogger<InviteConnections> logger,
            ICaptureSnapshot capture,
            ExecutionTracker executionOptions,
            ITrackingService trackingService,
            ILoginService loginService,
            IUtil util,
            IProfileCreate profileCreate,
            IProfileRead profileRead,
            ILinkedInChat linkedInChat)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _driver = driverFactory?.Create(true) ?? throw new ArgumentNullException(nameof(driverFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            //_capture = capture ?? throw new ArgumentNullException(nameof(capture));
            _executionOptions = executionOptions ?? throw new ArgumentNullException(nameof(executionOptions));
            _trackingService = trackingService ?? throw new ArgumentNullException(nameof(trackingService));
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
            _util = util;
            _profileCreate = profileCreate ?? throw new ArgumentNullException(nameof(profileCreate));
            _profileRead = profileRead ?? throw new ArgumentNullException(nameof(profileRead));
            _linkedInChat = linkedInChat ?? throw new ArgumentNullException(nameof(linkedInChat));
        }

        public async Task LoadConnectionsAsync()
        {

            var url = "https://www.linkedin.com/mynetwork/invite-connect/connections/";
            _logger.LogInformation("Navigating to profile: {ProfileUrl}", url);
            await _loginService.LoginAsync();
            _driver.Navigate().GoToUrl(url);
            await ScrollWorkspaceRepeatedlyAsync();
            var list = await GetConnections();
            await _linkedInChat.SendMessageAsync(list);

        }


        private async Task ScrollWorkspaceRepeatedlyAsync()
        {
            _logger.LogInformation("Starting workspace scrolling sequence...");

            for (int i = 0; i < 15; i++)
            {
                _logger.LogInformation("Iteration {Iteration}: Waiting for page load...", i + 1);
                await _util.WaitForPageLoadAsync(10);

                _logger.LogInformation("Iteration {Iteration}: Scrolling workspace down...", i + 1);
                ScrollWorkspaceDown();

                _logger.LogInformation("Iteration {Iteration}: Delay 50ms before next scroll...", i + 1);
                await Task.Delay(50);
            }

            _logger.LogInformation("Workspace scrolling sequence completed successfully.");
        }


        public async Task<List<ConnectionInfo>> GetConnections()
        {
            // Load existing collected connections
            var results = await _trackingService.LoadCollectorConnectionsAsync("Connections_Collected.json") ?? [];

            // Build a fast lookup of existing keys (canonical profile URL)
            var existingKeys = new HashSet<string>(
                results.Select(r => BuildKey(r.ProfileUrl)),
                StringComparer.OrdinalIgnoreCase
            );

            var added = 0;
            var skipped = 0;

            try
            {
                var main = "main#workspace div[data-view-name='connections-list'] > div[componentkey^='auto-component-']";
                // 1) Find all connection cards inside the workspace
                var cards = _driver.FindElements(By.CssSelector(main));

                foreach (var card in cards)
                {
                    try
                    {
                        var info = new ConnectionInfo();

                        // Profile URL
                        var profileTag = "a[data-view-name='connections-profile']";
                        var profileLink = card.FindElements(By.CssSelector(profileTag))
                                              .FirstOrDefault();
                        if (profileLink != null)
                        {
                            var href = profileLink.GetAttribute("href");
                            if (Uri.TryCreate(href, UriKind.Absolute, out var uri))
                                info.ProfileUrl = Canonicalize(uri);
                        }

                        // Extract all <p> tags text
                        var paragraphs = card.FindElements(By.TagName("p"))
                                             .Select(p => p.Text.Trim())
                                             .Where(t => !string.IsNullOrWhiteSpace(t))
                                             .ToList();

                        // Title/position: skip "Connected on ..." and name-only lines
                        var title = paragraphs.FirstOrDefault(t =>
                            !t.StartsWith("Connected on", StringComparison.OrdinalIgnoreCase) &&
                            !IsLikelyJustAName(t));
                        if (!string.IsNullOrEmpty(title))
                            info.TitleOrPosition = title;

                        // Connected date
                        var connectedLine = paragraphs.FirstOrDefault(t => t.StartsWith("Connected on", StringComparison.OrdinalIgnoreCase));
                        if (!string.IsNullOrEmpty(connectedLine))
                        {
                            var dateText = connectedLine.Replace("Connected on", "", StringComparison.OrdinalIgnoreCase).Trim();
                            if (DateTime.TryParse(dateText, out var dt))
                                info.ConnectedOn = dt;
                        }

                        // Dedup by canonical URL key. If no URL, we can't guarantee uniqueness → skip.
                        var key = BuildKey(info.ProfileUrl);
                        if (string.IsNullOrEmpty(key))
                        {
                            skipped++;
                            _logger.LogDebug("Skipping connection without a valid profile URL.");
                            continue;
                        }

                        if (existingKeys.Contains(key))
                        {
                            skipped++;
                            _logger.LogTrace("Duplicate connection skipped: {Key}", key);
                            continue;
                        }

                        // New unique entry
                        results.Add(info);
                        existingKeys.Add(key);
                        added++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to parse a connection card, skipping.");
                    }
                }

                if (added > 0)
                {
                    await _trackingService.SaveCollectorConnectionsAsync(results, "Connections_Collected.json");
                }

                _logger.LogInformation("Parsed connections. Added: {Added}, Duplicates/Skipped: {Skipped}, Total stored: {Total}.",
                    added, skipped, results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while extracting connections list.");
            }

            return results;

            // --- local helpers ---
            static string BuildKey(Uri? uri) => uri is null ? string.Empty : uri.ToString().TrimEnd('/').ToLowerInvariant();

            static Uri Canonicalize(Uri uri)
            {
                // Normalize LinkedIn profile URLs by removing query/fragment and trailing slash
                var builder = new UriBuilder(uri)
                {
                    Query = string.Empty,
                    Fragment = string.Empty
                };
                var normalized = builder.Uri.ToString().TrimEnd('/');
                return new Uri(normalized);
            }
        }


        private static bool IsLikelyJustAName(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return true;
            var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var hasCompanyCue = text.Contains(" at ", StringComparison.OrdinalIgnoreCase) ||
                                text.Contains("@") ||
                                text.Contains("|") ||
                                text.Contains("•");
            return !hasCompanyCue && words.Length <= 3;
        }


        private void ScrollWorkspaceDown()
        {
            try
            {
                //for (int i = 0; i < 10; i++)
                //{
                var workspace = _driver.FindElement(By.CssSelector("main#workspace"));
                var js = (IJavaScriptExecutor)_driver;
                js.ExecuteScript("arguments[0].scrollTop = arguments[0].scrollHeight;", workspace);
                _logger.LogDebug("Scrolled workspace element (id=workspace) to bottom successfully.");
                //}

            }
            catch (NoSuchElementException)
            {
                _logger.LogWarning("ScrollWorkspaceDown: Workspace element not found.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ScrollWorkspaceDown: Unexpected error while scrolling workspace.");
            }
        }
    }
}

