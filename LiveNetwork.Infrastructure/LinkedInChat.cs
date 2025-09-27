using System.Diagnostics;
using Configuration;
using LiveNetwork.Application.Services;
using LiveNetwork.Application.UseCases.CRUD.IMessageInteraction;
using LiveNetwork.Application.UseCases.CRUD.Profile;
using LiveNetwork.Application.UseCases.CRUD.Profile.Query;
using LiveNetwork.Domain;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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
        private readonly IMessageInteractionCreate _messageInteractionCreate;

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
            IProfileUpdate profileUpdate,
            IMessageInteractionCreate messageInteractionCreate)
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
            _messageInteractionCreate = messageInteractionCreate ?? throw new ArgumentNullException(nameof(messageInteractionCreate));
        }

        public async Task SendMessageAsync(List<ConnectionInfo>? connections = null)
        {
            int processed = 0, skipped = 0, errors = 0;
            connections = connections ?? await _trackingService.LoadCollectorConnectionsAsync("Connections_Collected.json") ?? [];
            var remainingConnections  = connections.ToList();
            var sw = Stopwatch.StartNew();
            try
            {

                _logger.LogInformation("Starting LinkedInChat.SendMessageAsync at {UtcNow} UTC", DateTimeOffset.UtcNow);


                var lastProcessedUtc = await _trackingService.LoadLastProcessedDateUtcAsync("Connections_LastProcessedUtc.txt"); // ✅ evita null
                var newestProcessedUtc = lastProcessedUtc;
                if (connections.Count == 0)
                {
                    _logger.LogWarning("No connections found to process.");
                    return;
                }

                _logger.LogInformation("Loaded {Count} connections. LastProcessedUtc={LastProcessedUtc:o}", connections.Count, lastProcessedUtc);

                // ✅ Login una sola vez antes del loop
                _logger.LogInformation("Starting LinkedIn login…");
                //await _loginService.LoginAsync();
                _logger.LogInformation("LinkedIn login completed successfully.");

                var connectionsNew = connections.Where(d => d.ConnectedOn > lastProcessedUtc);
                foreach (var connection in connectionsNew)
                {
                    if (connection?.ProfileUrl == null)
                    {
                        _logger.LogWarning("Connection missing ProfileUrl. Title/Position={Title}", connection?.TitleOrPosition);
                        skipped++;
                        continue;
                    }
                    string url = GetUrl(connection);

                    // Para buscar en tu store por path, conserva el PathAndQuery
                    var path = connection.ProfileUrl.OriginalString;

                    if (string.IsNullOrWhiteSpace(path))
                    {
                        _logger.LogWarning("ProfileUrl path is empty for URL: {Url}", url);
                        skipped++;
                        continue;
                    }

                    _logger.LogInformation("Resolving profile for URL: {Url} | ConnectedOn: {ConnectedOn:o}", url, connection.ConnectedOn);
                    var content = BuildWelcomeMessage();
                    var profileOp = await _profileRead.GetProfilesByUrlAsync(path, null, 10);
                    if (!profileOp.IsSuccessful)
                    {
                        _logger.LogWarning("Profile lookup failed for {Url}. Reason: {Message}", url, profileOp.Message);
                        skipped++;
                        continue;
                    }

                    var items = profileOp.Data?.Items;
                    if (items == null || !items.Any())
                    {
                        _logger.LogWarning("No profile found in repository for {Url}", url);
                        OpenUrlAndSendMessage(path, content);
                        remainingConnections.Remove(connection);
                        await _trackingService.SaveCollectorConnectionsAsync(remainingConnections, "Connections_Collected.json");
                        skipped++;
                        continue;
                    }

                    var firstProfile = items.FirstOrDefault();
                    if (firstProfile is null || string.IsNullOrWhiteSpace(firstProfile.Url.AbsolutePath))
                    {
                        _logger.LogWarning("Resolved profile is null or missing URL for {Url}", url);
                        skipped++;
                        continue;
                    }


                    var message = new MessageInteraction
                    (
                        Guid.NewGuid().ToString("N"),
                        content,
                        "WelcomeMessage",
                        InteractionStatus.Sent
                    )
                    {
                        ProfileId = firstProfile.Id
                    };

                    // ✅ Navega al perfil
                    _logger.LogInformation("Navigating to profile page: {ProfileUrl}", firstProfile.Url);
                    OpenUrlAndSendMessage(firstProfile.Url.OriginalString, content);
                    await _messageInteractionCreate.CreateMessageInteractionAsync(message);
                    if (connection.ConnectedOn == null)
                    {
                        _logger.LogWarning("Connection missing ConnectedOn date for profile {ProfileId}", firstProfile.Id);
                        skipped++;
                        continue;
                    }
                    if (connection.ConnectedOn > newestProcessedUtc)
                    {
                        newestProcessedUtc = connection.ConnectedOn.Value;
                    }
                    processed++;
                    remainingConnections.Remove(connection);
                    await _trackingService.SaveCollectorConnectionsAsync(remainingConnections, "Connections_Collected.json");
                }
                await _trackingService.SaveLastProcessedDateUtcAsync("Connections_LastProcessedUtc.txt", newestProcessedUtc);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LinkedIn login failed. Aborting message sending.");
                errors++;
                await _capture.CaptureArtifactsAsync(_executionOptions.ExecutionFolder, "login_failed");
            }
            sw.Stop();
            _logger.LogInformation("LinkedInChat finished in {Ms} ms. Processed={Processed}, Skipped={Skipped}, Errors={Errors}, Total={Total}.",
                sw.ElapsedMilliseconds, processed, skipped, errors, connections.Count);
            _logger.LogInformation("LinkedInChat finished. Processed={Processed} of {Total}.", processed, connections.Count);
        }

        private static string GetUrl(ConnectionInfo connection)
        {

            // ✅ Usa AbsoluteUri para la URL completa
            return connection.ProfileUrl.IsAbsoluteUri
                ? connection.ProfileUrl.AbsoluteUri
                : new Uri(new Uri("https://www.linkedin.com"), connection.ProfileUrl).AbsoluteUri;
        }

        private void OpenUrlAndSendMessage(string url, string content)
        {
            _logger.LogInformation("Navigating to {Url}...", url);
            _driver.Navigate().GoToUrl(url);

            var button = FindMessageButton();
            if (button == null)
            {
                _logger.LogWarning("Message button not found on page {Url}. Aborting Set().", url);
                return; // or throw, depending on desired behavior
            }

            _logger.LogInformation("Clicking message button...");
            button.Click();

            var textArea = FindMessageTextArea();
            if (textArea == null)
            {
                _logger.LogWarning("Message text area not found on page {Url}. Aborting Set().", url);
                return; // or throw
            }

            _logger.LogInformation("Entering content into message text area...");
            EnterTextInContentEditable(textArea, content);

            _logger.LogInformation("Message content set successfully on {Url}.", url);
        }


        public void CloseChatOverlay()
        {
            try
            {
                Thread.Sleep(500);
                var wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));

                // Try multiple strategies in sequence
                IWebElement closeButton = null;

                // Strategy 1: Try by data-test-icon (most reliable)
                try
                {
                    closeButton = wait.Until(drv =>
                    {
                        var buttons = drv.FindElements(By.XPath("//button[.//*[@data-test-icon='close-small']]"));
                        return buttons.FirstOrDefault(btn => btn.Displayed && btn.Enabled);
                    });
                    Console.WriteLine("✅ Found button by data-test-icon");
                }
                catch
                {
                    Console.WriteLine("❌ Button not found by data-test-icon");
                }

                // Strategy 2: Try by ID if first approach failed
                if (closeButton == null)
                {
                    try
                    {
                        closeButton = wait.Until(drv =>
                        {
                            var button = drv.FindElement(By.Id("ember500"));
                            return button.Displayed && button.Enabled ? button : null;
                        });
                        Console.WriteLine("✅ Found button by ID");
                    }
                    catch
                    {
                        Console.WriteLine("❌ Button not found by ID");
                    }
                }

                // Strategy 3: Try by aria-label
                if (closeButton == null)
                {
                    try
                    {
                        closeButton = wait.Until(drv =>
                        {
                            var buttons = drv.FindElements(By.CssSelector("button[aria-label*='Close your conversation']"));
                            return buttons.FirstOrDefault(btn => btn.Displayed && btn.Enabled);
                        });
                        Console.WriteLine("✅ Found button by aria-label");
                    }
                    catch
                    {
                        Console.WriteLine("❌ Button not found by aria-label");
                    }
                }

                if (closeButton != null)
                {
                    // Scroll into view and click
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView(true);", closeButton);
                    Thread.Sleep(500); // Brief pause
                    closeButton.Click();
                    Console.WriteLine("✅ Chat overlay closed successfully.");

                    // Wait for overlay to disappear
                    Thread.Sleep(1000);
                }
                else
                {
                    Console.WriteLine("❌ No close button found using any strategy");

                    // Emergency fallback: try to escape or find any close button
                    TryEmergencyClose();
                }
            }
            catch (WebDriverTimeoutException)
            {
                Console.WriteLine("❌ Close button not found within timeout. Chat overlay might not be open.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error closing chat overlay: {ex.Message}");
            }
        }

        private void TryEmergencyClose()
        {
            try
            {
                Console.WriteLine("⚠️ Trying emergency close strategies...");

                // Try ESC key
                new Actions(_driver).SendKeys(Keys.Escape).Perform();
                Console.WriteLine("Sent ESC key");
                Thread.Sleep(1000);

                // Try clicking outside the chat
                var body = _driver.FindElement(By.TagName("body"));
                new Actions(_driver).MoveToElement(body, 10, 10).Click().Perform();
                Console.WriteLine("Clicked outside chat area");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Emergency close failed: {ex.Message}");
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
                CloseChatOverlay();
            }
            catch
            {

            }
        }

        static string BuildWelcomeMessage()
        {
            return "Great to join your network! If you’re facing hiring or project challenges around C# or AI, I’d be glad to share ideas that could help.";
        }
    }
}
