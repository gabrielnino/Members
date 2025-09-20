using System.Diagnostics;
using Configuration;
using LiveNetwork.Application.Services;
using LiveNetwork.Domain;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace LiveNetwork.Infrastructure.Services
{
    public class InviteConnections : IInviteConnections
    {
        private readonly AppConfig _config;
        private readonly IWebDriver _driver;
        private readonly ILogger<InviteConnections> _logger;
        private readonly ICaptureSnapshot _capture;
        private readonly ExecutionTracker _executionOptions;
        private readonly ITrackingService _trackingService;
        private readonly ILoginService _loginService;
        const string ExecutionFolder = "Invite";
        const string Stage = "Send";
        public InviteConnections(
            AppConfig config,
            IWebDriverFactory driverFactory,
            ILogger<InviteConnections> logger,
            ICaptureSnapshot capture,
            ExecutionTracker executionOptions,
            ITrackingService trackingService,
            ILoginService loginService)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _driver = driverFactory?.Create(true) ?? throw new ArgumentNullException(nameof(driverFactory));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _capture = capture ?? throw new ArgumentNullException(nameof(capture));
            _executionOptions = executionOptions ?? throw new ArgumentNullException(nameof(executionOptions));
            _trackingService = trackingService ?? throw new ArgumentNullException(nameof(trackingService));
            _loginService = loginService ?? throw new ArgumentNullException(nameof(loginService));
        }

        public async Task Invite()
        {
            var sw = Stopwatch.StartNew();
            var correlationId = _executionOptions?.TimeStamp ?? DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            var threadsPath = _config.Paths.ConversationOutputFilePath;
            _logger.LogInformation("({CorrelationId}) Invite workflow started. Loading threads from {ThreadsPath}", correlationId, threadsPath);

            var threads = await LoadThreadsOrWarnAsync(threadsPath, correlationId);
            if (threads is null) { return; }

            var connections = GetActiveThreadsOrWarn(threads, correlationId);
            if (connections is null) { return; }

            if (!await EnsureLoggedInOrLogAbortAsync(correlationId)) { return; }

            _logger.LogInformation("({CorrelationId}) Loaded {Count} active conversation thread(s). Beginning processing.", correlationId, connections.Count);

            var processed = 0;
            var skipped = 0;
            var errors = 0;

            for (int index = 0; index < connections.Count; index++)
            {
                var thread = connections[index];
                if (!IsThreadProcessable(thread))
                {
                    skipped++;
                    continue;
                }

                try
                {
                    Navegate(thread);
                    if (!ExecuteInviteFlow(thread))
                    {
                        skipped++;
                    }
                    processed++;
                    await _trackingService.SaveConversationThreadAsync(threads, threadsPath);
                }
                catch (Exception ex)
                {
                    errors++;
                    _logger.LogError(ex, "Error processing profile {ProfileUrl}. Thread skipped.", thread.TargetProfile?.Url);
                    try { await _capture.CaptureArtifactsAsync(ExecutionFolder, Stage); } catch { /* best-effort */ }
                }
            }

            sw.Stop();
            _logger.LogInformation(
                "({CorrelationId}) Invite workflow finished in {ElapsedMs} ms. Processed: {Processed}, Skipped: {Skipped}, Errors: {Errors}",
                correlationId, sw.ElapsedMilliseconds, processed, skipped, errors);
        }

        private void Navegate(ConversationThread? thread)
        {
            var profileUrl = thread?.TargetProfile.Url;
            _logger.LogInformation("Navigating to profile: {ProfileUrl}", profileUrl);
            _driver.Navigate().GoToUrl(profileUrl);
            _logger.LogInformation("Navigation completed for {ProfileUrl}", profileUrl);
        }

        private bool ExecuteInviteFlow(ConversationThread? thread)
        {
            _logger.LogInformation("ExecuteInviteFlow: Starting invite flow for {ProfileName} ({ProfileUrl}).",
                thread?.TargetProfile?.FullName ?? "Unknown", thread?.TargetProfile?.Url?.ToString() ?? "N/A");

            var invitationComunication = thread?.GetInitialInvite();
            if (invitationComunication == null)
            {
                _logger.LogWarning("ExecuteInviteFlow: No initial invite communication found. Skipping.");
                return false;
            }

            thread.TryUpdateComunicationStatus(invitationComunication.Id, InviteStatus.Sent);
            _logger.LogDebug("ExecuteInviteFlow: Invite status set to 'Sent' for InviteId={InviteId}", invitationComunication.Id);

            var invitation = invitationComunication.Content;

            if (!ClickConnect())
            {
                if (!ClickMoreActions())
                {
                    _logger.LogWarning("ExecuteInviteFlow: Could not open 'More actions' menu. Skipping thread.");
                    return true; // lógica original preservada
                }
                else
                {
                    if (!ClickConnectFromOverflow())
                    {
                        _logger.LogWarning("ExecuteInviteFlow: Could not open 'Connect' dialog (invite modal). Skipping thread.");
                        return false;
                    }
                    _logger.LogDebug("ExecuteInviteFlow: 'Connect' dialog opened successfully (via overflow).");

                    if (SkipIfNoInviteContent(invitation))
                    {
                        _logger.LogInformation("ExecuteInviteFlow: Skipping thread because invite content is empty.");
                        return false;
                    }

                    if (!SendInvitation(invitation))
                    {
                        _logger.LogError("ExecuteInviteFlow: Failed to send invitation.");
                        return false;
                    }
                }
                _logger.LogDebug("ExecuteInviteFlow: 'More actions' menu opened.");
            }
            else
            {
                if (SkipIfNoInviteContent(invitation))
                {
                    _logger.LogInformation("ExecuteInviteFlow: Skipping thread because invite content is empty.");
                    return false;
                }

                if (!SendInvitation(invitation))
                {
                    _logger.LogError("ExecuteInviteFlow: Failed to send invitation.");
                    return false;
                }
            }
            _logger.LogInformation("ExecuteInviteFlow: Invitation sent successfully to {ProfileName}.",
                thread?.TargetProfile?.FullName ?? "Unknown");
            return true;
        }

        private bool SendInvitation(string messageText)
        {
            if (_config.Options.EnableCustomMessages)
            {
                if (!ClickAddFreeNote(messageText))
                {
                    _logger.LogWarning("Failed to send invite with a note (modal did not close or button unavailable). Skipping thread.");
                    return false;
                }
                return true;
            }

            if (!ClickSendWithoutNote())
            {
                _logger.LogWarning("Failed to send invite without a note (modal did not close or button unavailable). Skipping thread.");
                return false;
            }

            return true;
        }

        private bool IsThreadProcessable(ConversationThread? thread)
        {
            if (SkipIfNullThread(thread))
            {
                return false;
            }

            var targetProfile = thread?.TargetProfile;
            if (SkipIfNullTargetProfile(targetProfile))
            {
                return false;
            }

            var profileUrl = targetProfile?.Url;
            if (SkipIfInvalidateUrl(profileUrl))
            {
                return false;
            }

            return true;
        }

        private bool SkipIfInvalidateUrl(Uri? profileUrl)
        {
            if (profileUrl is null || string.IsNullOrWhiteSpace(profileUrl.Host))
            {
                _logger.LogWarning("Target profile URL is missing/invalid: {ProfileUrl}. Skipping thread.", profileUrl?.ToString());
                return true;
            }

            return false;
        }

        private bool SkipIfNullTargetProfile(LinkedInProfile? targetProfile)
        {
            if (targetProfile is null)
            {
                _logger.LogWarning("Target profile is null. Skipping thread.");
                return true;
            }

            return false;
        }

        private bool SkipIfNoInviteContent(string? invitation)
        {
            if (!string.IsNullOrWhiteSpace(invitation))
            {
                return false;
            }
            _logger.LogDebug("Thread has no initial invite content. Skipping.");
            return true; // signal caller to continue
        }

        private async Task<bool> EnsureLoggedInOrLogAbortAsync(string correlationId)
        {
            var ok = await EnsureLoggedInAsync(correlationId);
            if (ok) return true;

            _logger.LogError("({CorrelationId}) Aborting workflow due to unsuccessful login.", correlationId);
            try { await _capture.CaptureArtifactsAsync(ExecutionFolder, "Login"); } catch { /* best-effort */ }
            return false;
        }

        private async Task<List<ConversationThread>?> LoadThreadsOrWarnAsync(string threadsPath, string correlationId)
        {
            var threads = await _trackingService.LoadConversationThreadAsync(threadsPath);
            if (threads is null || threads.Count == 0)
            {
                var profilesPath = _config.Paths.DetailedProfilesOutputFilePath;
                _logger.LogInformation("Step 1/6: Loading detailed profiles from path: {ProfilesPath}", profilesPath);
                var swProfiles = Stopwatch.StartNew();
                var profiles = await _trackingService.LoadDetailedProfilesAsync(profilesPath);
                threads.AddRange(profiles.Select(p => new ConversationThread(p)));
                _logger.LogWarning("({CorrelationId}) No conversation threads found at {ThreadsPath}. Nothing to process.", correlationId, threadsPath);
            }
            return threads;
        }

        private bool SkipIfNullThread(ConversationThread? thread)
        {
            if (thread is null)
            {
                _logger.LogWarning("Thread is null. Skipping.");
                return true; // signal caller to continue
            }
            return false;
        }

        private List<ConversationThread>? GetActiveThreadsOrWarn(List<ConversationThread> threads, string correlationId)
        {
            var connections = threads
                .Where(t => t.Communications.Any(c =>
                c.TypeName == nameof(Invite) &&
                c.Status == InviteStatus.Draft.ToString())).ToList();

            if (connections.Count == 0)
            {
                _logger.LogWarning("({CorrelationId}) No threads with activity found. Nothing to process.", correlationId);
                return null;
            }

            return connections;
        }

        /// <summary>
        /// Logs in with detailed messages; returns true if login appears successful.
        /// </summary>
        private async Task<bool> EnsureLoggedInAsync(string correlationId)
        {
            _logger.LogInformation("({CorrelationId}) Attempting LinkedIn login…", correlationId);
            var sw = Stopwatch.StartNew();
            try
            {
                await _loginService.LoginAsync();
                sw.Stop();
                _logger.LogInformation("({CorrelationId}) Login successful in {ElapsedMs} ms.", correlationId, sw.ElapsedMilliseconds);
                return true;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "({CorrelationId}) Login failed after {ElapsedMs} ms.", correlationId, sw.ElapsedMilliseconds);
                try { await _capture.CaptureArtifactsAsync(ExecutionFolder, Stage); } catch { /* best-effort */ }
                return false;
            }
        }

        private bool ClickConnectFromOverflow()
        {
            var wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(12), TimeSpan.FromMilliseconds(250));

            var container = wait.Until(d =>
                d.FindElements(By.CssSelector("div.artdeco-dropdown__content-inner"))
                 .FirstOrDefault(e => e.Displayed));

            if (container is null)
            {
                _logger.LogWarning("Overflow menu not found/visible; cannot click 'Connect'.");
                return false;
            }

            By[] candidates =
            [
                By.XPath(".//div[@role='button' and contains(@class,'artdeco-dropdown__item') and contains(@aria-label,'Invite') and contains(@aria-label,'connect')]"),
                By.XPath(".//div[@role='button' and contains(@class,'artdeco-dropdown__item') and .//span[normalize-space()='Connect']]"),
                By.XPath(".//div[@role='button' and .//use[contains(@href,'connect-medium')]]")
            ];

            IWebElement? connect = null;
            foreach (var by in candidates)
            {
                connect = container.FindElements(by).FirstOrDefault(e => e.Displayed && e.Enabled);
                if (connect != null) break;
            }

            if (connect is null)
            {
                _logger.LogWarning("'Connect' item not present in overflow menu (maybe already 1st-degree or different UI).");
                return false;
            }

            try
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", connect);
            }
            catch { /* non-fatal */ }

            try
            {
                _logger.LogDebug("Clicking 'Connect' in overflow menu…");
                connect.Click();
            }
            catch (Exception)
            {
                _logger.LogDebug("Normal click failed. Retrying with JS click…");
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", connect);
            }

            try
            {
                wait.Until(d =>
                    d.FindElements(By.XPath("//div[@role='dialog']"))
                     .Any(dialog => dialog.Displayed &&
                                    dialog.FindElements(By.XPath(".//button[normalize-space()='Add a note' or normalize-space()='Send' or normalize-space()='Send without a note']")).Any()));
                _logger.LogInformation("'Connect' dialog opened.");
            }
            catch (WebDriverTimeoutException)
            {
                _logger.LogWarning("'Connect' clicked, but invite dialog did not appear within the timeout.");
            }

            return true;
        }

        private bool ClickConnect()
        {
            var wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(12), TimeSpan.FromMilliseconds(250));

            By[] locators =
            [
                By.CssSelector("button[aria-label*='connect' i]"),
                By.XPath("//button[@aria-label[contains(.,'connect')]]"),
                By.XPath("//button[.//span[normalize-space()='Connect']]"),
                By.CssSelector("button.artdeco-button--primary")
            ];

            IWebElement? btn = null;
            foreach (var by in locators)
            {
                try
                {
                    btn = wait.Until(d =>
                    {
                        var e = d.FindElements(by).FirstOrDefault(x => x.Displayed && x.Enabled);
                        return e;
                    });
                    if (btn != null) break;
                }
                catch (WebDriverTimeoutException)
                {
                    // try next locator
                }
            }

            if (btn is null)
            {
                _logger.LogInformation("Could not find 'Connect' button on the profile header.");
                return false;
            }

            try
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", btn);
            }
            catch { /* non-fatal */ }

            try
            {
                _logger.LogDebug("Clicking 'Connect' button…");
                btn.Click();
                return true;

            }
            catch (Exception)
            {
                _logger.LogDebug("Normal click failed. Retrying with JS click…");
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
                return false;
            }
        }

        /// <summary>
        /// Finds and clicks the "More actions" overflow button on a LinkedIn profile header.
        /// Waits until the menu is expanded. Returns true if clicked and expanded.
        /// </summary>
        private bool ClickMoreActions()
        {
            var wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(12), TimeSpan.FromMilliseconds(250));

            By[] locators =
            {
                By.CssSelector("button.artdeco-dropdown__trigger[aria-label='More actions']"),
                By.XPath("//button[@aria-label='More actions']"),
                By.XPath("//button[.//span[normalize-space()='More']]"),
                By.XPath("//button[starts-with(@id,'ember') and contains(@id,'profile-overflow-action')]")
            };

            IWebElement? btn = null;
            foreach (var by in locators)
            {
                try
                {
                    btn = wait.Until(d =>
                    {
                        var e = d.FindElements(by).FirstOrDefault(x => x.Displayed && x.Enabled);
                        return e;
                    });
                    if (btn != null) break;
                }
                catch (WebDriverTimeoutException)
                {
                    // try next locator
                }
            }

            if (btn is null)
            {
                _logger.LogInformation("Could not find 'More actions' button on the profile header.");
                return false;
            }

            try
            {
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", btn);
            }
            catch { /* non-fatal */ }

            try
            {
                _logger.LogDebug("Clicking 'More actions' button…");
                btn.Click();
                return true;
            }
            catch (Exception)
            {
                _logger.LogDebug("Normal click failed. Retrying with JS click…");
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", btn);
                return false;
            }
        }

        private bool ClickAddFreeNote(string messageText)
        {
            var wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(12), TimeSpan.FromMilliseconds(250));

            var modal = wait.Until(d =>
                d.FindElements(By.CssSelector("div.artdeco-modal.send-invite[role='dialog']"))
                 .FirstOrDefault(e => e.Displayed));

            if (modal is null)
            {
                _logger.LogWarning("Invite modal not visible; cannot click 'Add a free note'.");
                return false;
            }

            By[] candidates =
            [
                By.CssSelector("button[aria-label='Add a free note']"),
                By.XPath(".//button[.//span[normalize-space()='Add a free note']]"),
                By.XPath(".//button[contains(@class,'artdeco-button') and .//span[normalize-space()='Add a free note']]")
            ];

            IWebElement? addNoteBtn = null;
            foreach (var by in candidates)
            {
                addNoteBtn = modal.FindElements(by).FirstOrDefault(e => e.Displayed && e.Enabled);
                if (addNoteBtn != null) break;
            }

            if (addNoteBtn is null)
            {
                _logger.LogWarning("'Add a free note' button not found (UI/locale may differ).");
                return false;
            }

            try { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", addNoteBtn); } catch { /* ignore */ }

            try
            {
                _logger.LogDebug("Clicking 'Add a free note'…");
                addNoteBtn.Click();

                if (TrySetInvitationMessage(messageText))
                {
                    return true;
                }
            }
            catch (Exception)
            {
                _logger.LogDebug("Normal click failed. Retrying with JS click…");
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", addNoteBtn);
            }

            try
            {
                var noteFieldVisible = wait.Until(d =>
                    d.FindElements(By.CssSelector("textarea[name='message']")).Any(e => e.Displayed));
                if (noteFieldVisible) _logger.LogInformation("'Add a free note' clicked successfully (note field visible).");
                return noteFieldVisible;
            }
            catch (WebDriverTimeoutException)
            {
                _logger.LogWarning("Clicked 'Add a free note' but the note field did not appear in time.");
                return false;
            }
        }

        private bool TrySetInvitationMessage(string messageText)
        {
            var wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(250));

            try
            {
                var textArea = wait.Until(d =>
                    d.FindElements(By.CssSelector("textarea[name='message']"))
                     .FirstOrDefault(e => e.Displayed && e.Enabled));

                if (textArea is null)
                {
                    _logger.LogWarning("Invitation message textarea not found.");
                    return false;
                }

                textArea.Clear();

                var truncatedMessage = TruncateMessage(messageText, 200);
                textArea.SendKeys(truncatedMessage);
                TryClickSendInvitation();
                _logger.LogInformation("Invitation message set successfully.");
                return true;
            }
            catch (WebDriverTimeoutException)
            {
                _logger.LogWarning("Timed out waiting for invitation message textarea.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to set invitation message.");
                return false;
            }
        }

        public string TruncateMessage(string messageText, int maxLength)
        {
            if (string.IsNullOrEmpty(messageText))
            {
                _logger.LogWarning("TruncateMessage called with null or empty input.");
                return messageText ?? string.Empty;
            }

            if (messageText.Length <= maxLength)
            {
                _logger.LogDebug("TruncateMessage: No truncation needed. Length={Length}, Max={MaxLength}", messageText.Length, maxLength);
                return messageText;
            }

            string truncated = messageText.Substring(0, maxLength);
            _logger.LogInformation("TruncateMessage: Message truncated. OriginalLength={OriginalLength}, Max={MaxLength}", messageText.Length, maxLength);

            return truncated;
        }

        private bool TryClickSendInvitation()
        {
            var wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(250));

            try
            {
                By[] locators =
                [
                    By.CssSelector("button[aria-label='Send invitation']"),
                    By.XPath("//button[.//span[normalize-space()='Send']]")
                ];

                IWebElement? sendBtn = null;
                foreach (var by in locators)
                {
                    sendBtn = wait.Until(d =>
                        d.FindElements(by).FirstOrDefault(e => e.Displayed && e.Enabled));
                    if (sendBtn != null) break;
                }

                if (sendBtn is null)
                {
                    _logger.LogWarning("'Send invitation' button not found.");
                    return false;
                }

                try
                {
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", sendBtn);
                }
                catch { /* ignore */ }

                try
                {
                    _logger.LogDebug("Clicking 'Send invitation' button…");
                    sendBtn.Click();
                }
                catch (Exception)
                {
                    _logger.LogDebug("Normal click failed. Retrying with JS click…");
                    ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", sendBtn);
                }

                var closed = wait.Until(d =>
                    !d.FindElements(By.CssSelector("div.artdeco-modal.send-invite[role='dialog']"))
                      .Any(e => e.Displayed));

                if (closed)
                {
                    _logger.LogInformation("Invitation sent successfully (modal closed).");
                    return true;
                }

                return false;
            }
            catch (WebDriverTimeoutException)
            {
                _logger.LogWarning("Timed out waiting for 'Send invitation' action to complete.");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error clicking 'Send invitation' button.");
                return false;
            }
        }

        private bool ClickSendWithoutNote()
        {
            var wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(12), TimeSpan.FromMilliseconds(250));

            var modal = wait.Until(d =>
                d.FindElements(By.CssSelector("div.artdeco-modal.send-invite[role='dialog']"))
                 .FirstOrDefault(e => e.Displayed));

            if (modal is null)
            {
                _logger.LogWarning("Invite modal not visible; cannot click 'Send without a note'.");
                return false;
            }

            By[] candidates =
            [
                By.CssSelector("button[aria-label='Send without a note']"),
                By.XPath(".//button[.//span[normalize-space()='Send without a note']]"),
                By.XPath(".//button[contains(@class,'artdeco-button--primary') and .//span[normalize-space()='Send without a note']]")
            ];

            IWebElement? sendBtn = null;
            foreach (var by in candidates)
            {
                sendBtn = modal.FindElements(by).FirstOrDefault(e => e.Displayed && e.Enabled);
                if (sendBtn != null) break;
            }

            if (sendBtn is null)
            {
                _logger.LogWarning("'Send without a note' button not found (UI/locale may differ).");
                return false;
            }

            try { ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", sendBtn); } catch { /* ignore */ }

            try
            {
                _logger.LogDebug("Clicking 'Send without a note'…");
                sendBtn.Click();
            }
            catch (Exception)
            {
                _logger.LogDebug("Normal click failed. Retrying with JS click…");
                ((IJavaScriptExecutor)_driver).ExecuteScript("arguments[0].click();", sendBtn);
            }

            try
            {
                var closed = wait.Until(d =>
                    !d.FindElements(By.CssSelector("div.artdeco-modal.send-invite[role='dialog']"))
                      .Any(e => e.Displayed));
                if (closed) _logger.LogInformation("Invite sent without a note (modal closed).");
                return closed;
            }
            catch (WebDriverTimeoutException)
            {
                _logger.LogWarning("Clicked 'Send without a note' but the modal did not close in time.");
                return false;
            }
        }
    }
}
