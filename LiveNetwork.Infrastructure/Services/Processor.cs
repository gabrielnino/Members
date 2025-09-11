using Configuration;
using LiveNetwork.Application.Services;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Services.Interfaces;

namespace LiveNetwork.Infrastructure.Services
{
    public class Processor : IProcessor
    {
        private readonly IWebDriver _driver;
        private readonly AppConfig _config;
        private readonly ILogger<Processor> _logger;
        private readonly ExecutionTracker _executionOptions;
        private const string FolderName = "Page";
        private readonly ISecurityCheck _securityCheck;
        private string FolderPath => Path.Combine(_executionOptions.ExecutionFolder, FolderName);
        private readonly ICaptureSnapshot _capture;
        private readonly IDirectoryCheck _directoryCheck;
        private readonly ITrackingService _trackingService;

        private readonly WebDriverWait _wait;
        private readonly IUtil _util;
        public Processor(IWebDriverFactory driverFactory,
            AppConfig config,
            ILogger<Processor> logger,
            ExecutionTracker executionOptions,
            ICaptureSnapshot capture,
            ISecurityCheck securityCheck,
            IDirectoryCheck directoryCheck,
            ITrackingService trackingService,
            IUtil util)
        {
            _driver = driverFactory.Create();
            _config = config;
            _logger = logger;
            _executionOptions = executionOptions;
            _capture = capture;
            _securityCheck = securityCheck;
            _directoryCheck = directoryCheck;
            _directoryCheck.EnsureDirectoryExists(FolderPath);
            _trackingService = trackingService;
            _util = util;
        }

        public async Task ProcessAllPagesAsync()
        {
            var searchId = _executionOptions.TimeStamp;
            var searchText = _config.Search.SearchText;
            var maxPages = _config.Search.MaxPages;

            _logger.LogInformation($"🔎 ID:{searchId} Starting search processing for '{searchText}'");

            // Load saved tracking state
            var trackingState = await _trackingService.LoadStateAsync();
            _logger.LogInformation($"📂 ID:{searchId} Loaded tracking state: Page {trackingState.LastProcessedPage}, Offers: {trackingState.Connections?.Count ?? 0}");

            // If previous run completed, reuse cached results
            if (trackingState.IsComplete && trackingState.Connections?.Any() == true)
            {
                _logger.LogInformation($"✅ ID:{searchId} Cached complete result found. Skipping processing.");
                return;
            }

            var connections = trackingState.Connections?.ToList() ?? [];
            int startPage = trackingState.LastProcessedPage + 1;
            var initialPage = true;
            for (int currentPage = startPage; currentPage <= maxPages; currentPage++)
            {
                if (startPage != 1 && initialPage)
                {
                    for (int i = 0; i < startPage; i++)
                    {
                        _util.ScrollMove();
                        await _util.NavigateToNextPageAsync();
                    }
                    initialPage = false;
                }

                _logger.LogInformation($"📄 ID:{searchId} Processing page {currentPage}...");

                try
                {
                    await _capture.CaptureArtifactsAsync(FolderPath, $"Page_{currentPage}");



                    _util.ScrollMove();
                    await Task.Delay(3000);

                    var pageOffers = GetCurrentPage();

                    if (pageOffers == null || !pageOffers.Any())
                    {
                        _logger.LogWarning($"⚠️ ID:{searchId} No offers found on page {currentPage}. Aborting pagination.");
                        break;
                    }

                    connections.AddRange(pageOffers);

                    // Save progress
                    trackingState.LastProcessedPage = currentPage;
                    trackingState.Connections = connections;
                    await _trackingService.SavePageStateAsync(trackingState);

                    _logger.LogInformation($"✔️ ID:{searchId} Page {currentPage} done. Offers found: {pageOffers.Count()}");

                    if (currentPage >= maxPages || !await _util.NavigateToNextPageAsync())
                    {
                        _logger.LogInformation($"⏹️ ID:{searchId} No more pages to process or max page limit reached.");
                        break;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"❌ ID:{searchId} Error on page {currentPage}. Aborting run.");
                    break;
                }
            }

            trackingState.IsComplete = true;
            trackingState.Connections = connections;
            await _trackingService.SavePageStateAsync(trackingState);
            var outputFile = Path.Combine(_executionOptions.ExecutionFolder, _config.Paths.SearchUrlOutputFilePath);
            await _trackingService.SaveConnectionsAsync(connections, Path.Combine(_executionOptions.ExecutionFolder, _config.Paths.SearchUrlOutputFilePath));

            _logger.LogInformation($"🏁 ID:{searchId} Finished search. Total offers: {connections.Count}");
        }








        private IEnumerable<Uri> GetCurrentPage()
        {
            _logger.LogInformation("🔍 Extracting unique profile links from current LinkedIn page...");

            try
            {
                var profileElements = _driver.FindElements(By.XPath("//ul[@role='list']//a[contains(@href, '/in/')]"));

                if (profileElements == null || profileElements.Count == 0)
                {
                    _logger.LogWarning("⚠️ No profile links found using XPath.");
                    return [];
                }

                var uniqueUrls = new HashSet<Uri>();
                int invalidUrls = 0, duplicates = 0;

                foreach (var element in profileElements)
                {
                    var rawHref = element.GetAttribute("href");

                    if (string.IsNullOrWhiteSpace(rawHref))
                    {
                        _logger.LogDebug("⛔ Skipped empty or null href.");
                        continue;
                    }

                    try
                    {
                        // Strip query parameters and fragments
                        var cleanUrl = rawHref.Split('?')[0].Trim();

                        if (!cleanUrl.StartsWith("https://www.linkedin.com/in/"))
                        {
                            _logger.LogDebug("❌ Skipped non-profile URL: {Url}", cleanUrl);
                            continue;
                        }

                        if (Uri.TryCreate(cleanUrl, UriKind.Absolute, out var uri))
                        {
                            if (uniqueUrls.Add(uri))
                            {
                                _logger.LogDebug("✅ Added clean URL: {Url}", cleanUrl);
                            }
                            else
                            {
                                duplicates++;
                                _logger.LogTrace("🔁 Duplicate skipped: {Url}", cleanUrl);
                            }
                        }
                        else
                        {
                            invalidUrls++;
                            _logger.LogWarning("⚠️ Invalid URI skipped: {Url}", rawHref);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ Exception while processing URL: {Url}", rawHref);
                    }
                }

                _logger.LogInformation("📦 Extraction complete. Total unique profiles: {Count}, Invalid: {Invalid}, Duplicates: {Duplicates}",
                    uniqueUrls.Count, invalidUrls, duplicates);

                return uniqueUrls;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Unexpected error during profile link extraction.");
                return [];
            }
        }
    }
}
