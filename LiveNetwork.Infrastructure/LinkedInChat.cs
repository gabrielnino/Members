using Configuration;
using LiveNetwork.Application.Services;
using LiveNetwork.Application.UseCases.CRUD.Profile;
using LiveNetwork.Application.UseCases.CRUD.Profile.Query;
using LiveNetwork.Domain;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace LiveNetwork.Infrastructure.Services
{
    public class LinkedInChat : ILinkedInChat
    {
        private readonly AppConfig _config;
        private readonly IWebDriver _driver;
        private readonly ILogger<LinkedInChat> _logger; // ✅ corrige el tipo genérico
        private readonly ICaptureSnapshot _capture;
        private readonly ExecutionTracker _executionOptions;
        private readonly ITrackingService _trackingService;
        private readonly ILoginService _loginService;
        private readonly IUtil _util;
        private readonly IProfileRead _profileRead;
        private readonly IProfileUpdate _profileUpdate;

        public LinkedInChat(
            AppConfig config,
            IWebDriverFactory driverFactory,
            ILogger<LinkedInChat> logger,            // ✅ corrige aquí también
            ICaptureSnapshot capture,
            ExecutionTracker executionOptions,
            ITrackingService trackingService,
            ILoginService loginService,
            IUtil util,
            IProfileRead profileRead,
            IProfileUpdate profileUpdate)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _driver = driverFactory?.Create() ?? throw new ArgumentNullException(nameof(driverFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _capture = capture ?? throw new ArgumentNullException(nameof(capture));
            _executionOptions = executionOptions ?? throw new ArgumentNullException(nameof(executionOptions));
            _trackingService = trackingService ?? throw new ArgumentNullException(nameof(trackingService));
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
            _util = util ?? throw new ArgumentNullException(nameof(util));
            _profileRead = profileRead ?? throw new ArgumentNullException(nameof(profileRead));
            _profileUpdate = profileUpdate ?? throw new ArgumentNullException(nameof(profileUpdate));
        }

        public async Task SendMessageAsync()
        {
            try
            {
                _logger.LogInformation("Starting LinkedInChat.SendMessageAsync at {UtcNow} UTC", DateTimeOffset.UtcNow);

                var connections = await _trackingService.LoadCollectorConnectionsAsync("Connections_Collected.json") ?? [];
                var lastProcessedUtc = await _trackingService.LoadLastProcessedDateUtcAsync("Connections_LastProcessedUtc.txt"); // ✅ evita null
                var newestProcessedUtc = lastProcessedUtc;
                if (connections.Count == 0)
                {
                    _logger.LogWarning("No connections found to process.");
                    return;
                }

                _logger.LogInformation("Loaded {Count} connections. LastProcessedUtc={LastProcessedUtc:o}", connections.Count, lastProcessedUtc);

                // ✅ Login una sola vez antes del loop

                _logger.LogInformation("Attempting LinkedIn login…");
                await _loginService.LoginAsync();
                _logger.LogInformation("Login successful.");


                var processed = 0;
                foreach (var connection in connections)
                {
                    // ✅ Filtra por fecha de conexión procesada
                    if (connection.ConnectedOn <= lastProcessedUtc)
                    {
                        _logger.LogDebug("Skipping connection (older than last processed): {ConnectedOn:o}", connection.ConnectedOn);
                        continue;
                    }

                    if (connection?.ProfileUrl == null)
                    {
                        _logger.LogWarning("Connection missing ProfileUrl. Title/Position={Title}", connection?.TitleOrPosition);
                        continue;
                    }

                    // ✅ Usa AbsoluteUri para la URL completa
                    var url = connection.ProfileUrl.IsAbsoluteUri
                        ? connection.ProfileUrl.AbsoluteUri
                        : new Uri(new Uri("https://www.linkedin.com"), connection.ProfileUrl).AbsoluteUri;

                    // Para buscar en tu store por path, conserva el PathAndQuery
                    var path = connection.ProfileUrl.OriginalString;

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        _logger.LogWarning("ProfileUrl path is empty for URL: {Url}", url);
                        continue;
                    }

                    _logger.LogInformation("Resolving profile for URL: {Url} | ConnectedOn: {ConnectedOn:o}", url, connection.ConnectedOn);

                    var profileOp = await _profileRead.GetProfilesByUrlAsync(path, null, 10);
                    if (!profileOp.IsSuccessful)
                    {
                        _logger.LogWarning("Profile lookup failed for {Url}. Reason: {Message}", url, profileOp.Message);
                        continue;
                    }

                    var items = profileOp.Data?.Items;
                    if (items == null || !items.Any())
                    {
                        _logger.LogWarning("No profile found in repository for {Url}", url);
                        continue;
                    }

                    var firstProfile = items.FirstOrDefault();
                    if (firstProfile is null || string.IsNullOrWhiteSpace(firstProfile.Url.AbsolutePath))
                    {
                        _logger.LogWarning("Resolved profile is null or missing URL for {Url}", url);
                        continue;
                    }

                    var content = BuildWelcomeMessage();
                    var message = new MessageInteraction
                    (
                        firstProfile.Id,
                        content,
                        "WelcomeMessage",
                        InteractionStatus.Sent
                    );


                    // ✅ Navega al perfil
                    _logger.LogInformation("Navigating to profile page: {ProfileUrl}", firstProfile.Url);
                    _driver.Navigate().GoToUrl(firstProfile.Url);
                    var button = FindMessageButton();
                    button.Click();
                    var textArea = FindMessageTextArea();
                    EnterTextInContentEditable(textArea, content);
                    firstProfile.AddMessage(message);
                    await _profileUpdate.UpdateProfileAsync(firstProfile);
                    if (connection.ConnectedOn == null)
                    {
                        _logger.LogWarning("Connection missing ConnectedOn date for profile {ProfileId}", firstProfile.Id);
                        continue;
                    }
                    if (connection.ConnectedOn > newestProcessedUtc)
                    {
                        newestProcessedUtc = connection.ConnectedOn.Value;
                        await _trackingService.SaveLastProcessedDateUtcAsync("Connections_LastProcessedUtc.txt", newestProcessedUtc);
                    }
                }

                _logger.LogInformation("LinkedInChat finished. Processed={Processed} of {Total}.", processed, connections.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed. Aborting message sending.");
                await _capture.CaptureArtifactsAsync(_executionOptions.ExecutionFolder, "login_failed");
            }
        }


        /// <summary>
        /// Finds the "Message" button for a given profile (e.g., "Message Lisa").
        /// </summary>
        /// <param name="profileName">The profile name (e.g., "Lisa").</param>
        /// <returns>IWebElement of the button if found, otherwise null.</returns>
        public IWebElement? FindMessageButton()
        {
            try
            {
                // Wait for the button to be present
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

                // Find by multiple attributes for better reliability
                var button = wait.Until(drv =>
                {
                    var buttons = drv.FindElements(By.CssSelector("button[aria-label^='Message'][class*='artdeco-button']"));
                    return buttons.FirstOrDefault(btn => btn.Text.Contains("Message"));
                });

                return button;
            }
            catch (WebDriverTimeoutException)
            {
                throw new NoSuchElementException("Message button not found within the timeout period");
            }
        }

        public IWebElement FindMessageTextArea()
        {
            var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));

            return wait.Until(drv =>
            {
                return drv.FindElement(By.CssSelector("div[role='textbox'][aria-label='Write a message…']"));
            });
        }

        private void EnterTextInContentEditable(IWebElement textArea, string message)
        {
            try
            {
                textArea.Click();
                textArea.Clear();
                textArea.SendKeys(message);
                textArea.SendKeys(Keys.Enter); // To trigger any change events if necessary   
            }
            catch (Exception ex)
            {
                // Method 2: Fallback using JavaScript
                try
                {
                    IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
                    js.ExecuteScript(@"
                arguments[0].innerText = arguments[1];
                arguments[0].dispatchEvent(new Event('input', { bubbles: true }));
                arguments[0].dispatchEvent(new Event('change', { bubbles: true }));
            ", textArea, message);
                }
                catch (Exception jsEx)
                {
                    throw new Exception($"Failed to enter text in contenteditable div. Standard error: {ex.Message}, JS error: {jsEx.Message}");
                }
            }
        }

        static string BuildWelcomeMessage()
        {
            return "Great to join your network! If you’re facing hiring or project challenges around C# or AI, I’d be glad to share ideas that could help.";
        }
    }
}
