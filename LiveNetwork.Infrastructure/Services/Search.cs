using System.Diagnostics;
using Configuration;
using LiveNetwork.Application.Services;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Services.Interfaces;

namespace LiveNetwork.Infrastructure.Services
{
    public class Search : ISearch
    {
        private readonly IWebDriver _driver;
        private readonly AppConfig _config;
        private readonly ILogger<Search> _logger;
        private readonly ExecutionTracker _executionOptions;
        private const string FolderName = "Search";
        private string FolderPath => Path.Combine(_executionOptions.ExecutionFolder, FolderName);
        private readonly IDirectoryCheck _directoryCheck;
        private readonly IUtil _util;
        public Search(IWebDriverFactory driverFactory,
            AppConfig config,
            ILogger<Search> logger,
            ICaptureSnapshot capture,
            ExecutionTracker executionOptions,
            IDirectoryCheck directoryCheck,
            IUtil util)
        {
            _driver = driverFactory.Create();
            _config = config;
            _logger = logger;
            _executionOptions = executionOptions;
            _logger.LogInformation($"📁 Created execution folder at: {_executionOptions.ExecutionFolder}");
            _directoryCheck = directoryCheck;
            _directoryCheck.EnsureDirectoryExists(FolderPath);
            _util = util;
        }

        public async Task RunSearchAsync()
        {
            _logger.LogInformation("🔍 Starting LinkedIn search process | Search text: {SearchText}", _config.Search.SearchText);

            try
            {
                // Construct URL with proper encoding for search text
                var encodedSearchText = Uri.EscapeDataString(_config.Search.SearchText);
                var url = $"https://www.linkedin.com/search/results/people/?geoUrn=[\"102044150\"]&keywords={encodedSearchText}";

                _logger.LogDebug("🌐 Navigating to LinkedIn search URL: {SearchUrl}", url);
                _driver.Navigate().GoToUrl(url);
                _logger.LogInformation("✅ LinkedIn search completed successfully | Search text: {SearchText}", _config.Search.SearchText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ LinkedIn search failed | Search text: {SearchText} | Error: {ErrorMessage}",
                    _config.Search.SearchText, ex.Message);
                throw;
            }
        }
    }
}
