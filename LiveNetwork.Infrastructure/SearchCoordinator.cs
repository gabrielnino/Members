using LiveNetwork.Application.Services;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace LiveNetwork.Infrastructure.Services
{
    public class SearchCoordinator : ISearchCoordinator
    {
        private readonly ILoginService _loginService;
        private readonly ISearch _search;
        private readonly IProcessor _pageProcessor;
        private readonly ILogger<SearchCoordinator> _logger;
        private readonly IResumeDetailService _resumeDetailService;

        public SearchCoordinator(
            ILoginService loginService,
            ISearch search,
            IProcessor pageProcessor,
            ILogger<SearchCoordinator> logger,
            IResumeDetailService resumeDetailService)
        {
            _loginService = loginService;
            _search = search;
            _pageProcessor = pageProcessor;
            _logger = logger;
            _resumeDetailService = resumeDetailService;
        }

        public async Task SearchConnectionAsync()
        {
            _logger.LogInformation("🔍 Starting LinkedIn search connection process");

            try
            {
                _logger.LogDebug("🔑 Attempting to log in to LinkedIn...");
                await _loginService.LoginAsync();
                _logger.LogInformation("✅ Logged in to LinkedIn successfully");

                _logger.LogDebug("🌐 Initiating LinkedIn search...");
                await _search.RunSearchAsync();
                _logger.LogInformation("✅ LinkedIn search executed successfully");

                _logger.LogDebug("📄 Beginning page processing...");
                await _pageProcessor.ProcessAllPagesAsync();
                _logger.LogInformation("✅ Page processing completed");

                _logger.LogDebug("📑 Starting detailed resume extraction process...");
                await _resumeDetailService.RunResumeDetailProcessAsync();
                _logger.LogInformation("✅ Resume detail extraction process completed");

                _logger.LogInformation("🎉 LinkedIn search connection process completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ LinkedIn search connection failed | Error: {ErrorMessage}", ex.Message);
                throw;
            }
            finally
            {
                _logger.LogDebug("🏁 Finalizing LinkedIn search connection process");
            }
        }
    }
}