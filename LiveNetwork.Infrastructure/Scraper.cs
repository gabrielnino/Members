using System.Globalization;
using System.Text;
using Configuration;
using LiveNetwork.Application.Services;
using LiveNetwork.Domain;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace LiveNetwork.Infrastructure.Services
{
    public sealed class Scraper : ScraperBase, IScraperService
    {
        private readonly IWebDriver _driver;
        private readonly ILogger<Scraper> _logger;
        private readonly IDictionary<string, string> _productUrls;
        private readonly ExecutionTracker _executionOptions;

        public Scraper(
            ExecutionTracker executionOptions,
            IWebDriverFactory driverFactory,
            ILogger<Scraper> logger)
        {
            _executionOptions = executionOptions ?? throw new ArgumentNullException(nameof(executionOptions));
            _driver = driverFactory.Create();
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _logger.LogInformation("Initializing Scraper with execution folder: {ExecutionFolder}", _executionOptions.ExecutionFolder);

            _productUrls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["quickbooks"] = "https://www.capterra.com/p/190778/QuickBooks-Online/reviews/",
                //["xero"] = "https://www.capterra.com/p/120109/Xero/reviews/",
                ["freshbooks"] = "https://www.capterra.com/p/142390/FreshBooks/reviews/",
                ["clio"] = "https://www.capterra.com/p/105428/Clio/reviews/",
                ["mycase"] = "https://www.capterra.com/p/115613/MyCase/reviews/",
                ["practicepanther"] = "https://www.capterra.com/p/140231/PracticePanther-Legal-Software/reviews/",
                //["toggl"] = "https://www.capterra.com/p/167420/Toggl-Hire/reviews/",
                //["harvest"] = "https://www.capterra.com/p/75598/Harvest/reviews/"
            };

            _logger.LogInformation("Configured {Count} product URLs for scraping", _productUrls.Count);
        }

        public async Task<int> ScrapeAsync()
        {
            _logger.LogInformation("Starting scraping process...");

            var rows = new List<Review>();

            foreach (var (product, url) in _productUrls)
            {
                _logger.LogInformation("---- Starting iteration for {Product} ----", product);

                try
                {
                    _logger.LogInformation("Navigating to {Product} reviews page: {Url}", product, url);
                    _driver.Navigate().GoToUrl(url);

                    _logger.LogDebug("Applying filters for {Product}", product);
                    await SelectSortByAsync(_driver, "MOST_HELPFUL");
                    await SelectCompanySizeSelfEmployedAsync(_driver);
                    await SelectFrequencyDailyAsync(_driver);

                    _logger.LogDebug("Expanding all 'Continue Reading' sections for {Product}", product);
                    await ExpandAllContinueReadingAsync(_driver);

                    _logger.LogInformation("Extracting reviews for {Product}", product);
                    var reviews = ExtractReviews(_driver, TimeSpan.FromSeconds(12), msg => _logger.LogDebug(msg));
                    _logger.LogInformation("Extracted {Count} reviews for {Product}", reviews.Count, product);

                    rows.AddRange(reviews);

                    _logger.LogInformation("---- Finished iteration for {Product} ----", product);
                }
                catch (WebDriverTimeoutException tex)
                {
                    _logger.LogWarning(tex, "Timeout while scraping {Product} at {Url}. Skipping...", product, url);
                }
                catch (NoSuchElementException nse)
                {
                    _logger.LogWarning(nse, "Element not found while scraping {Product} at {Url}. Skipping...", product, url);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unexpected error scraping {Product} at {Url}. Continuing with next product...", product, url);
                }
            }

            var outputPath = Path.Combine(_executionOptions.ExecutionFolder, "capterra_reviews.csv");
            _logger.LogInformation("Writing {Count} reviews to {OutputPath}", rows.Count, outputPath);

            try
            {
                WriteCsv(outputPath, rows);
                _logger.LogInformation("Successfully wrote CSV file with {Count} reviews", rows.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write CSV file at {OutputPath}", outputPath);
                throw; // lo dejamos propagar para no ocultar un error crítico de salida
            }

            _logger.LogInformation("Scraping process finished. Total reviews collected: {Count}", rows.Count);
            return rows.Count;
        }


        private static void WriteCsv(string path, IEnumerable<Review> reviews)
        {
            if (path is null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            if (reviews is null)
            {
                throw new ArgumentNullException(nameof(reviews));
            }

            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrWhiteSpace(dir))
            {
                Directory.CreateDirectory(dir);
            }

            // UTF-8 con BOM para mejor compatibilidad con Excel
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            using var sw = new StreamWriter(fs, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));

            var header = new[]
            {
                "reviewer_name","reviewer_role","reviewer_industry","reviewer_used_for","reviewer_avatar_url",
                "title","review_date",
                "overall_rating","ease_of_use","customer_service","features","value_for_money",
                "likelihood_to_recommend_0_10",
                "pros","cons","free_text",
                "alternatives_considered","reason_for_choosing","switched_from","switch_reason"
            };
            sw.WriteLine(string.Join(",", header));

            foreach (var review in reviews)
            {
                var alternatives = GetAlternativesConsidered(review);
                var switchedFrom = GetSwitchedFrom(review);

                var fields = new[]
                {
                    Csv(review.Reviewer?.Name),
                    Csv(review.Reviewer?.Role),
                    Csv(review.Reviewer?.Industry),
                    Csv(review.Reviewer?.UsedFor),
                    Csv(review.Reviewer?.AvatarUrl),
                    Csv(review.Title),
                    Csv(review.ReviewDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                    Csv(review.OverallRating?.ToString(CultureInfo.InvariantCulture)),
                    Csv(review.Ratings?.EaseOfUse?.ToString(CultureInfo.InvariantCulture)),
                    Csv(review.Ratings?.CustomerService?.ToString(CultureInfo.InvariantCulture)),
                    Csv(review.Ratings?.Features?.ToString(CultureInfo.InvariantCulture)),
                    Csv(review.Ratings?.ValueForMoney?.ToString(CultureInfo.InvariantCulture)),
                    Csv(review.LikelihoodToRecommend10?.ToString(CultureInfo.InvariantCulture)),
                    Csv(review.Pros),
                    Csv(review.Cons),
                    Csv(review.FreeText),
                    Csv(alternatives),
                    Csv(review.ReasonForChoosing),
                    Csv(switchedFrom),
                    Csv(review.SwitchReason),
                };

                sw.WriteLine(string.Join(",", fields));
            }
        }

        private static string? GetSwitchedFrom(Review r) =>
            (r.SwitchedFrom != null && r.SwitchedFrom.Count != 0)
                ? string.Join(" | ", r.SwitchedFrom)
                : null;

        private static string? GetAlternativesConsidered(Review r) =>
            (r.AlternativesConsidered != null && r.AlternativesConsidered.Count != 0)
                ? string.Join(" | ", r.AlternativesConsidered)
                : null;

        private static string Csv(string? input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;

            var s = input.Replace("\r\n", "\n").Replace("\r", "\n");
            var containsSpecial = s.Contains(',') || s.Contains('"') || s.Contains('\n');

            if (s.Contains('"'))
                s = s.Replace("\"", "\"\"");

            return containsSpecial ? $"\"{s}\"" : s;
        }
    }
}
