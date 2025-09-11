using System.Diagnostics;
using System.Text;
using Configuration;
using LiveNetwork.Application.Services;
using LiveNetwork.Domain;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using Services.Interfaces;

namespace LiveNetwork.Infrastructure.Services
{
    public class ResumeDetailService : IResumeDetailService
    {
        private readonly IWebDriver _driver;
        private readonly AppConfig _config;
        private readonly ILogger<ResumeDetailService> _logger; // ✅ correct logger type
        private readonly ExecutionTracker _executionOptions;
        private readonly IDirectoryCheck _directoryCheck;
        private readonly ITrackingService _trackingService;
        private readonly ICaptureSnapshot _capture;
        private readonly IUtil _util;

        private const string FolderName = "Detail";
        private string FolderPath => Path.Combine(_executionOptions.ExecutionFolder, FolderName);

        public ResumeDetailService(
            IWebDriverFactory driverFactory,
            AppConfig config,
            ILogger<ResumeDetailService> logger,
            ICaptureSnapshot capture,
            ExecutionTracker executionOptions,
            IDirectoryCheck directoryCheck,
            ITrackingService trackingService,
            IUtil util)
        {
            _driver = driverFactory.Create();
            _config = config;
            _logger = logger;
            _executionOptions = executionOptions;

            _logger.LogInformation("📁 [Init/Step 1] Execution folder: {ExecutionFolder}", _executionOptions.ExecutionFolder);

            _directoryCheck = directoryCheck;
            _directoryCheck.EnsureDirectoryExists(FolderPath);
            _logger.LogInformation("📁 [Init/Step 2] Ensured detail folder exists: {FolderPath}", FolderPath);

            _trackingService = trackingService;
            _capture = capture;
            _util = util;
        }

        public async Task RunResumeDetailProcessAsync()
        {
            var swTotal = Stopwatch.StartNew();
            _logger.LogInformation("🚀 [Process/Step 1] Starting in-page profile extraction | Search=\"{SearchText}\"",
                _config.Search.SearchText);
            var outputFile = Path.Combine(_executionOptions.ExecutionFolder, _config.Paths.SearchUrlOutputFilePath);
            var connections = await _trackingService.LoadConnectionsFromSearchAsync(outputFile);
            _logger.LogInformation("📄 [Process/Step 2] Loaded {Count} profile URL(s) from {File}",
                connections.Count, _config.Paths.SearchUrlOutputFilePath);

            if (connections.Count == 0)
            {
                _logger.LogWarning("⚠️ [Process/Step 3] No profiles found. Nothing to process.");
                return;
            }

            // 🆕 Load already-processed URLs to support resume
            var processed = await _trackingService.LoadProcessedUrlsAsync(_config.Search.SearchText);
            if (processed.Count > 0)
            {
                _logger.LogInformation("🔁 [Process/Step 3b] Found {Count} previously processed URL(s). They will be skipped.", processed.Count);
            }

            try
            {
                var detailed = new List<LinkedInProfile>();

                detailed = await _trackingService.LoadDetailedProfilesAsync(_config.Paths.DetailedProfilesOutputFilePath);

                for (int i = 0; i < connections.Count; i++)
                {
                    var url = connections[i];
                    var urlStr = url.ToString();

                    // 🆕 Skip if already processed
                    if (processed.Contains(urlStr))
                    {
                        _logger.LogInformation("⏭️ [Profile {Index}/{Total}] Skipping already processed: {Url}", i + 1, connections.Count, urlStr);
                        continue;
                    }

                    var swItem = Stopwatch.StartNew();

                    _logger.LogInformation("🔗 [Profile {Index}/{Total}/Step 1] Navigating to: {Url}",
                        i + 1, connections.Count, urlStr);

                    _driver.Navigate().GoToUrl(url);

                    _logger.LogInformation("🌐 [Profile {Index}/{Total}/Step 2] Page requested. Waiting briefly for the DOM to stabilize...",
                        i + 1, connections.Count);

                    await Task.Delay(800); // brief stabilization

                    var profile = await ExtractLinkedInProfile(_driver, url);

                    _logger.LogInformation("🧾 [Profile {Index}/{Total}/Step 3] Extracted top-card data for: {Name}",
                        i + 1, connections.Count, profile.FullName ?? "(unknown)");

                    detailed.Add(profile);

                    await _trackingService.SaveLinkedInProfilesAsync(detailed, _config.Paths.DetailedProfilesOutputFilePath);

                    // 🆕 So this run also knows we've done it (no double work if list contains duplicates)
                    processed.Add(urlStr);

                    swItem.Stop();
                    _logger.LogInformation("⏱️ [Profile {Index}/{Total}/Done] Completed in {ElapsedMs} ms",
                        i + 1, connections.Count, swItem.ElapsedMilliseconds);
                }

                swTotal.Stop();
                _logger.LogInformation("🎯 [Process/Done] Finished extraction for {Count} profile(s) in {ElapsedMs} ms | Search=\"{SearchText}\"",
                    connections.Count, swTotal.ElapsedMilliseconds, _config.Search.SearchText);
            }
            catch (Exception ex)
            {
                swTotal.Stop();
                _logger.LogError(ex, "❌ [Process/Failed] Extraction failed after {ElapsedMs} ms | Search=\"{SearchText}\"",
                    swTotal.ElapsedMilliseconds, _config.Search.SearchText);
                throw;
            }
        }

        public async Task<List<ExperienceModel>> GetExperienceListAsync(IWebDriver driver)
        {
            _logger.LogInformation("🧭 [Experience/Step 1] Locating the Experience section...");

            var results = new List<ExperienceModel>();

            try
            {
                // 1) Find the Experience section by anchor id="experience"
                var section = driver.FindElements(By.XPath("//div[@id='experience']/ancestor::section")).FirstOrDefault();
                if (section == null)
                {
                    _logger.LogInformation("ℹ️ [Experience/Step 2] Experience section not found.");
                    return results;
                }

                // 2) Within the section, find top-level company blocks (profile-component-entity)
                var companyBlocks = section.FindElements(By.XPath(".//div[@data-view-name='profile-component-entity']"));
                _logger.LogInformation("📦 [Experience/Step 3] Found {Count} top-level experience block(s).", companyBlocks.Count);

                foreach (var companyBlock in companyBlocks)
                {
                    try
                    {
                        // Company name & URL
                        var companyLink = companyBlock.FindElements(By.XPath(".//a[contains(@class,'optional-action-target-wrapper') and @target='_self']"))
                                                      .FirstOrDefault();
                        var companyUrl = companyLink?.GetAttribute("href") ?? "";

                        var companyName = companyBlock.FindElements(By.XPath(".//div[contains(@class,'hoverable-link-text') and contains(@class,'t-bold')]/span"))
                                                      .FirstOrDefault()?.Text?.Trim() ?? "";

                        // Company logo
                        var logoImg = companyBlock.FindElements(By.XPath(".//img[contains(@class,'EntityPhoto') or contains(@class,'ivm-view-attr__img--centered')]"))
                                                  .FirstOrDefault();
                        var logoUrl = logoImg?.GetAttribute("src") ?? "";
                        var logoAlt = logoImg?.GetAttribute("alt") ?? "";

                        // Employment summary (e.g., "Full-time · 5 yrs")
                        var employmentSummary = companyBlock.FindElements(By.XPath(".//span[contains(@class,'t-14') and contains(@class,'t-normal')]/span"))
                                                            .FirstOrDefault()?.Text?.Trim() ?? "";

                        // Company/location line (e.g., "Herndon, Virginia, United States")
                        var location = companyBlock.FindElements(By.XPath(".//span[contains(@class,'t-14') and contains(@class,'t-black--light')]//span[contains(@class,'pvs-entity__caption-wrapper')]"))
                                                   .FirstOrDefault()?.Text?.Trim() ?? "";

                        var companyModel = new ExperienceModel
                        {
                            Company = companyName,
                            CompanyUrl = companyUrl,
                            CompanyLogoUrl = logoUrl,
                            CompanyLogoAlt = logoAlt,
                            EmploymentSummary = employmentSummary,
                            Location = location
                        };

                        // Nested roles inside ".pvs-entity__sub-components" (if present)
                        var roleBlocks = companyBlock.FindElements(By.XPath(".//div[contains(@class,'pvs-entity__sub-components')]//div[@data-view-name='profile-component-entity']"));
                        if (roleBlocks.Any())
                        {
                            _logger.LogDebug("🔎 [Experience] Company '{Company}' has {Count} nested role(s).", companyName, roleBlocks.Count);
                            foreach (var role in roleBlocks)
                            {
                                companyModel.Roles.Add(ParseRoleBlock(role));
                            }
                        }
                        else
                        {
                            // Single-position pattern (no nested subcomponents)
                            var singleRole = ParseRoleBlock(companyBlock);
                            if (!string.IsNullOrWhiteSpace(singleRole.Title) || !string.IsNullOrWhiteSpace(singleRole.DateRange))
                            {
                                companyModel.Roles.Add(singleRole);
                            }
                        }

                        // Contextual skills (e.g., "Information Technology")
                        var skills = companyBlock.FindElements(By.XPath(".//strong"))
                                                 .Select(s => s.Text?.Trim())
                                                 .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
                        if (!string.IsNullOrWhiteSpace(skills))
                        {
                            foreach (var r in companyModel.Roles.Where(r => string.IsNullOrEmpty(r.ContextualSkills)))
                                r.ContextualSkills = skills!;
                        }

                        results.Add(companyModel);
                        _logger.LogInformation("✅ [Experience/Add] Company '{Company}' with {Count} role(s).", companyModel.Company, companyModel.Roles.Count);
                    }
                    catch (Exception exCompany)
                    {
                        _logger.LogWarning(exCompany, "⚠️ [Experience] Failed to parse a top-level experience block.");
                    }
                }

                _logger.LogInformation("🎯 [Experience/Done] Parsed {Count} company experience item(s).", results.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [Experience/Failed] Unexpected error while parsing the Experience section.");
            }

            return results;
        }

        public async Task<List<EducationModel>> GetEducationListAsync(IWebDriver driver)
        {
            _logger.LogInformation("🧭 [Education/Step 1] Locating the Education section...");
            var list = new List<EducationModel>();

            try
            {
                // Try 1: canonical section with header "Education"
                var container = driver.FindElements(By.XPath(
                                    "//section[.//h2[contains(normalize-space(),'Education')]]"))
                                .FirstOrDefault();

                // Try 2: fallback — container that holds the footer link "Show all ... educations"
                if (container == null)
                {
                    container = driver.FindElements(By.XPath(
                        "//div[.//a[@id='navigation-index-see-all-education']]"))
                        .FirstOrDefault();
                }

                if (container == null)
                {
                    _logger.LogInformation("ℹ️ [Education/Step 2] Education section not found.");
                    return list;
                }

                // Each education item is a profile-component-entity
                var nodes = container.FindElements(By.XPath(".//div[@data-view-name='profile-component-entity']"));
                _logger.LogInformation("📦 [Education/Step 3] Found {Count} education item(s).", nodes.Count);

                foreach (var el in nodes)
                {
                    try
                    {
                        // School name (bold)
                        var school = el.FindElements(By.XPath(".//div[contains(@class,'hoverable-link-text') and contains(@class,'t-bold')]/span"))
                                       .FirstOrDefault()?.Text?.Trim() ?? "";

                        // School URL (anchor wrapping the row)
                        var schoolLink = el.FindElements(By.XPath(".//a[contains(@class,'optional-action-target-wrapper') and @target='_self']"))
                                           .FirstOrDefault()?.GetAttribute("href") ?? "";

                        // Logo
                        var logoImg = el.FindElements(By.XPath(".//img[contains(@class,'EntityPhoto') or contains(@class,'ivm-view-attr__img--centered')]"))
                                        .FirstOrDefault();
                        var logoUrl = logoImg?.GetAttribute("src") ?? "";
                        var logoAlt = logoImg?.GetAttribute("alt") ?? "";

                        // Degree + Field line
                        var degreeFieldRaw = el.FindElements(By.XPath(".//span[contains(@class,'t-14') and contains(@class,'t-normal')]/span"))
                                               .Select(x => x.Text?.Trim())
                                               .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t))
                                               ?? "";

                        string degree = "", field = "";
                        if (!string.IsNullOrWhiteSpace(degreeFieldRaw))
                        {
                            var parts = degreeFieldRaw.Split(',', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                            degree = parts.Length > 0 ? parts[0] : degreeFieldRaw;
                            field = parts.Length > 1 ? parts[1] : "";
                        }

                        // Date range (e.g., "Oct 2022" or "2011 - 2014")
                        var dateRange = el.FindElements(By.XPath(".//span[contains(@class,'t-14') and contains(@class,'t-black--light')]//span[contains(@class,'pvs-entity__caption-wrapper') or self::span]"))
                                          .Select(x => x.Text?.Trim())
                                          .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t))
                                          ?? "";

                        // Optional description/activities
                        var description = el.FindElements(By.XPath(".//div[contains(@class,'inline-show-more-text')]/span"))
                                            .Select(x => x.Text?.Trim())
                                            .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t))
                                            ?? "";

                        list.Add(new EducationModel
                        {
                            School = school,
                            SchoolUrl = schoolLink,
                            LogoUrl = logoUrl,
                            LogoAlt = logoAlt,
                            Degree = degree,
                            Field = field,
                            DateRange = dateRange,
                            Description = NormalizeWhitespace(description)
                        });

                        _logger.LogInformation("✅ [Education/Add] '{School}' | {Degree}{FieldSep}{Field} | {Date}",
                            school, degree, string.IsNullOrEmpty(field) ? "" : ", ", field, dateRange);
                    }
                    catch (Exception exItem)
                    {
                        _logger.LogWarning(exItem, "⚠️ [Education] Failed to parse an education item.");
                    }
                }

                _logger.LogInformation("🎯 [Education/Done] Parsed {Count} education record(s).", list.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [Education/Failed] Unexpected error while parsing the Education section.");
            }

            return list;
        }

        private ExperienceRoleModel ParseRoleBlock(IWebElement roleBlock)
        {
            var role = new ExperienceRoleModel();

            try
            {
                // Title (bold span within the role block)
                role.Title = roleBlock.FindElements(By.XPath(".//div[contains(@class,'hoverable-link-text') and contains(@class,'t-bold')]/span"))
                                      .FirstOrDefault()?.Text?.Trim() ?? "";

                // Date range (e.g., "Sep 2020 - Present · 5 yrs")
                role.DateRange = roleBlock.FindElements(By.XPath(".//span[contains(@class,'t-14') and contains(@class,'t-black--light')]//span[contains(@class,'pvs-entity__caption-wrapper') or self::span]"))
                                          .Select(e => e.Text?.Trim())
                                          .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)) ?? "";

                // Work arrangement (On-site / Remote / Hybrid)
                var workArrangement = roleBlock.FindElements(By.XPath(".//span[contains(@class,'t-14') and contains(@class,'t-black--light')]/span"))
                                               .Select(e => e.Text?.Trim())
                                               .FirstOrDefault(t => t is "On-site" or "Remote" or "Hybrid");
                role.WorkArrangement = workArrangement ?? "";

                // Description (collapsed bullets)
                var desc = roleBlock.FindElements(By.XPath(".//div[contains(@class,'inline-show-more-text')]/span"))
                                    .Select(e => e.Text?.Trim())
                                    .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t)) ?? "";
                role.Description = NormalizeWhitespace(desc);

                // Contextual skills (e.g., "Information Technology")
                var contextual = roleBlock.FindElements(By.XPath(".//strong"))
                                          .Select(e => e.Text?.Trim())
                                          .FirstOrDefault(t => !string.IsNullOrWhiteSpace(t));
                if (!string.IsNullOrWhiteSpace(contextual))
                    role.ContextualSkills = contextual!;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ [Experience] Failed to parse a role block.");
            }

            return role;
        }

        private static string NormalizeWhitespace(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;
            var sb = new StringBuilder(input.Length);
            bool ws = false;
            foreach (var ch in input)
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!ws) { sb.Append(' '); ws = true; }
                }
                else
                {
                    sb.Append(ch);
                    ws = false;
                }
            }
            return sb.ToString().Trim();
        }

        public async Task<LinkedInProfile> ExtractLinkedInProfile(IWebDriver driver, Uri url)
        {
            _logger.LogInformation("🧭 [Extract/Step 1] Extracting top-card fields...");
            var sw = Stopwatch.StartNew();

            var profile = new LinkedInProfile
            {
                FullName = TryGetText(By.CssSelector("h1.inline.t-24"), driver),
                Headline = TryGetText(By.CssSelector("div.text-body-medium.break-words"), driver),
                Location = TryGetText(By.CssSelector("span.text-body-small.inline.t-black--light.break-words"), driver),
                CurrentCompany = TryGetText(By.XPath("//button[contains(@aria-label, 'Current company:')]//div[@dir='ltr']"), driver),
                ProfileImageUrl = TryGetAttribute(By.CssSelector("img.pv-top-card-profile-picture__image--show"), "src", driver),
                BackgroundImageUrl = TryGetAttribute(By.CssSelector("img.profile-background-image__image"), "src", driver),
                ConnectionDegree = TryGetText(By.CssSelector("span.dist-value"), driver),
                Connections = TryGetText(By.XPath("//span[@class='t-bold' and contains(text(), '+')]"), driver),
                Followers = TryGetText(By.XPath("//li[.//text()[contains(., 'followers')]]//span[@class='t-bold']"), driver),
                AboutText = GetAboutSection(driver),
                Url = url,
                Experiences = await GetExperienceListAsync(driver),
                Educations = await GetEducationListAsync(driver),
            };

            sw.Stop();
            _logger.LogInformation("✅ [Extract/Done] Extracted profile '{Name}' in {ElapsedMs} ms",
                profile.FullName ?? "(unknown)", sw.ElapsedMilliseconds);

            return profile;
        }

        private static string? TryGetText(By by, IWebDriver driver)
        {
            try
            {
                var el = driver.FindElement(by);
                return el?.Text?.Trim();
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        private static string? TryGetAttribute(By by, string attribute, IWebDriver driver)
        {
            try
            {
                var el = driver.FindElement(by);
                return el?.GetAttribute(attribute);
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        }

        public string GetAboutSection(IWebDriver driver)
        {
            _logger.LogInformation("📄 [About/Step 1] Reading About section...");
            try
            {
                var aboutElement = driver.FindElements(By.XPath(
                        "//section[contains(@class, 'pv-profile-card')]//div[contains(@class, 'inline-show-more-text')]" +
                        "//span[contains(@class, 'visually-hidden')]"))
                    .FirstOrDefault(el => el.Displayed);

                if (aboutElement == null)
                {
                    _logger.LogInformation("ℹ️ [About/Done] About section not found.");
                    return string.Empty;
                }

                var text = aboutElement.Text.Trim();
                _logger.LogInformation("✅ [About/Done] Extracted {Length} character(s).", text.Length);
                return text;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [About/Failed] Failed to extract the About section.");
                return string.Empty;
            }
        }

        public async Task<List<ProjectModel>> GetProjectListAsync(IWebDriver driver)
        {
            _logger.LogInformation("🧭 [Projects/Step 1] Locating the Projects section...");
            var projects = new List<ProjectModel>();

            try
            {
                var section = driver.FindElements(By.XPath("//section[@data-view-name='profile-card']//div[@id='projects']/ancestor::section"))
                                    .FirstOrDefault();

                if (section == null)
                {
                    _logger.LogWarning("⚠️ [Projects/Step 2] Projects section not found.");
                    return projects;
                }

                var nodes = section.FindElements(By.XPath(".//div[@data-view-name='profile-component-entity']"));
                _logger.LogInformation("📦 [Projects/Step 3] Found {Count} project item(s).", nodes.Count);

                foreach (var el in nodes)
                {
                    try
                    {
                        var title = el.FindElement(By.XPath(".//div[contains(@class, 't-bold')]/span")).Text.Trim();
                        var durationEl = el.FindElements(By.XPath(".//span[contains(@class,'t-14')]//span")).FirstOrDefault();
                        var duration = durationEl?.Text?.Trim() ?? string.Empty;

                        var descriptionEl = el.FindElements(By.XPath(".//div[contains(@class,'inline-show-more-text')]/span")).FirstOrDefault();
                        var description = descriptionEl?.Text?.Trim() ?? string.Empty;

                        var linkEl = el.FindElements(By.XPath(".//a[contains(@class, 'artdeco-button')]")).FirstOrDefault();
                        var url = linkEl?.GetAttribute("href") ?? string.Empty;

                        projects.Add(new ProjectModel
                        {
                            Title = title,
                            Duration = duration,
                            Description = description,
                            Url = url
                        });

                        _logger.LogInformation("✅ [Projects/Add] '{Title}' | {Duration} | {Url}", title, duration, url);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "⚠️ [Projects] Failed to parse a project item.");
                    }
                }

                _logger.LogInformation("🎯 [Projects/Done] Parsed {Count} project(s).", projects.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ [Projects/Failed] Error while locating or parsing the Projects section.");
            }

            return projects;
        }
    }
}
