using Configuration;
using LiveNetwork.Application.Services;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using Services.Interfaces;

namespace LiveNetwork.Infrastructure.Services
{
    public class ChromeDriverFactory : IWebDriverFactory, IDisposable
    {
        private bool _hide;
        private readonly ILogger<ChromeDriverFactory> _logger;
        private readonly ChromeDriverService _driverService;
        private readonly AppConfig _appConfig;
        private IWebDriver? _driver;

        public ChromeDriverFactory(ILogger<ChromeDriverFactory> logger, AppConfig appConfig)
        {
            _logger = logger;
            _driverService = ChromeDriverService.CreateDefaultService();
            _driverService.HideCommandPromptWindow = true;
            _appConfig = appConfig;
        }

        public IWebDriver Create(bool hide = false)
        {
            _hide = hide;
            if (_driver == null)
            {
                var downloadFolder = _appConfig.Paths.DownloadFolder;
                var options = _hide ? GetHideOptions(downloadFolder) : GetDefaultOptions(downloadFolder);
                _driver = new ChromeDriver(_driverService, options);
                SetTimeouts(_driver);
                _logger.LogInformation("Creating new ChromeDriver instance");
            }
            return _driver;
        }

        private static void SetTimeouts(IWebDriver driver)
        {
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
            driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);
        }

        public IWebDriver Create(Action<ChromeOptions> configureOptions)
        {
            DisposeDriverIfExists();
            var downloadFolder = _appConfig.Paths.DownloadFolder;
            var options = _hide ? GetHideOptions(downloadFolder) : GetDefaultOptions(downloadFolder); ;
            configureOptions?.Invoke(options);
            return CreateDriver(options);
        }

        private IWebDriver CreateDriver(ChromeOptions options)
        {
            try
            {
                _logger.LogInformation("Creating new ChromeDriver instance");
                _driver = new ChromeDriver(_driverService, options);
                _driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                _driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(120);
                _driver.Manage().Timeouts().AsynchronousJavaScript = TimeSpan.FromSeconds(10);
                return _driver;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ChromeDriver");
                throw new WebDriverException("Failed to initialize ChromeDriver", ex);
            }
        }

        public ChromeOptions GetDefaultOptions(string downloadFolder)
        {
            var options = new ChromeOptions();
            options.AddArguments("--start-maximized");
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            DownloadConfigure(downloadFolder, options);
            return options;
        }

        public static ChromeOptions GetHideOptions(string downloadFolder)
        {
            var options = new ChromeOptions();
            options.AddArguments("--headless=new"); // 👈 Hides the browser window
            options.AddArguments("--disable-gpu"); // 👈 Optional but recommended on Windows
            options.AddArguments("--window-size=1920,1080"); // 👈 Optional for headless layout
            options.AddArguments("--start-maximized");
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);
            DownloadConfigure(downloadFolder, options);
            return options;
        }

        private static void DownloadConfigure(string downloadFolder, ChromeOptions options)
        {
            options.AddUserProfilePreference("download.default_directory", downloadFolder);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("profile.default_content_settings.popups", 0);
            options.AddUserProfilePreference("safebrowsing.enabled", true);
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true); // avoid Chrome PDF viewer
        }



        private void DisposeDriverIfExists()
        {
            if (_driver != null)
            {
                _logger.LogInformation("Disposing existing ChromeDriver instance");
                try { _driver.Quit(); } catch { /* ignore */ }
                _driver.Dispose();
                _driver = null;
            }
        }

        public void Dispose()
        {
            if (_driver != null)
            {
                _logger.LogInformation("Disposing ChromeDriver instance");
                _driver.Quit();
                _driver.Dispose();
                _driver = null;
            }
            _driverService?.Dispose();
        }
    }
}
