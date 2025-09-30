using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Configuration;
using LiveNetwork.Application.Services;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace LiveNetwork.Infrastructure.Services
{
    public sealed class Scraper : IScraperService
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
            _driver = driverFactory.Create(); ;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            // Map products -> review URLs (extend as needed)
            _productUrls = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["quickbooks"] = "https://www.capterra.com/p/190778/QuickBooks-Online/reviews/",
                ["xero"] = "https://www.capterra.com/p/91822/Xero/reviews/",
                ["freshbooks"] = "https://www.capterra.com/p/72422/FreshBooks/reviews/",
                ["clio"] = "https://www.capterra.com/p/115737/Clio/reviews/",
                ["mycase"] = "https://www.capterra.com/p/115268/MyCase/reviews/",
                ["practicepanther"] = "https://www.capterra.com/p/147170/PracticePanther-Legal-Software/reviews/",
                ["toggl"] = "https://www.capterra.com/p/123373/Toggl-Track/reviews/",
                ["harvest"] = "https://www.capterra.com/p/100397/Harvest/reviews/"
            };
        }



        public async Task<int> ScrapeAsync()
        {
            var MaxReviews = 50;
            var rows = new List<ReviewRow>();
            foreach (var (product, url) in _productUrls)
            {


                _logger.LogInformation("Opening {Url}", url);
                _driver.Navigate().GoToUrl(url);
                var wait = new WebDriverWait(new SystemClock(), _driver, TimeSpan.FromSeconds(20), TimeSpan.FromMilliseconds(300));
                TryClick(wait, By.CssSelector("button#onetrust-accept-btn-handler, button[aria-label*='Accept']"));

                /*
                 int attemptsWithoutNew = 0;

                 while (rows.Count < 50 && attemptsWithoutNew < 3)
                 {
                     // Selectors are intentionally flexible; adjust if site markup changes.
                     var cards = _driver.FindElements(By.CssSelector("[data-qa='review-card'], article[class*='Review']"));
                     foreach (var card in cards)
                     {
                         if (rows.Count >= MaxReviews) break;

                         string reviewer = SafeText(card, ".Reviewer__name, [data-qa='reviewer-name'], [class*='reviewerName']");
                         string role = SafeText(card, "[data-qa='industry'], .Reviewer__role, [class*='industry']");
                         string date = SafeText(card, "[data-qa='review-date'], time, .Review__date");
                         string rating = SafeAttr(card, "div[aria-label*='rating'], [data-qa='star-rating']", "aria-label");
                         string pros = BlockText(card, "section:has(h3:contains('Pros')), [data-qa='review-pros'], .Pros");
                         string cons = BlockText(card, "section:has(h3:contains('Cons')), [data-qa='review-cons'], .Cons");

                         // Skip empty
                         if (string.IsNullOrWhiteSpace(pros) && string.IsNullOrWhiteSpace(cons)) continue;

                         rows.Add(new ReviewRow
                         {
                             Site = "capterra",
                             Product = product,
                             Reviewer = reviewer,
                             Role = role,
                             Date = date,
                             Rating = rating,
                             Pros = Normalize(pros),
                             Cons = Normalize(cons)
                         });
                     }

                     int before = rows.Count;
                     // Try to reveal more reviews:
                     bool clicked = TryClick(wait, By.CssSelector("button:has(span:contains('Load more')), button[aria-label*='Load more'], a[aria-label*='Next'], button:contains('Next')"));
                     if (!clicked)
                     {
                         // Fallback: scroll to bottom to trigger lazy loading
                         ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, document.body.scrollHeight);");
                         await Task.Delay(700);
                     }

                     await Task.Delay(800);
                     attemptsWithoutNew = (rows.Count > before) ? 0 : attemptsWithoutNew + 1;
                 }
                */
            }
            var OutputPath = Path.Combine(_executionOptions.ExecutionFolder, "capterra_reviews.csv");
            WriteCsv(_executionOptions.ExecutionFolder, rows);
            _logger.LogInformation("Saved {Count} reviews to {Path}", rows.Count, OutputPath);
            return rows.Count;
        }

        private static string Normalize(string? s) => (s ?? "").Replace("\r", " ").Replace("\n", " ").Trim();

        private static bool TryClick(WebDriverWait wait, By by)
        {
            try
            {
                var el = wait.Until(ExpectedConditions.ElementToBeClickable(by));
                el?.Click();
                return true;
            }
            catch { return false; }
        }

        private static string SafeText(ISearchContext ctx, string css)
        {
            try { return ctx.FindElement(By.CssSelector(css)).Text?.Trim() ?? ""; }
            catch { return ""; }
        }

        private static string SafeAttr(ISearchContext ctx, string css, string attr)
        {
            try { return ctx.FindElement(By.CssSelector(css)).GetAttribute(attr)?.Trim() ?? ""; }
            catch { return ""; }
        }

        // “:contains()” is not native in CSS; some drivers/polyfills support it; if not, fall back:
        private static string BlockText(IWebElement card, string primaryCss)
        {
            // Try direct
            try { return card.FindElement(By.CssSelector(primaryCss)).Text?.Trim() ?? ""; }
            catch { /* ignore */ }

            // Fallback: look for heading with text and read next sibling
            foreach (var headingSel in new[] { "h3", "h4", "strong" })
            {
                var headings = card.FindElements(By.CssSelector(headingSel));
                foreach (var h in headings)
                {
                    var t = h.Text?.Trim().ToLowerInvariant();
                    if (t == "cons" || t?.Contains("cons") == true) return h.FindElement(By.XPath("following-sibling::*")).Text.Trim();
                    if (t == "pros" || t?.Contains("pros") == true) return h.FindElement(By.XPath("following-sibling::*")).Text.Trim();
                }
            }
            return "";
        }

        private static void WriteCsv(string path, List<ReviewRow> rows)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? ".");
            var sb = new StringBuilder();
            sb.AppendLine("Site,Product,Reviewer,Role,Date,Rating,Pros,Cons");
            foreach (var r in rows)
            {
                sb.AppendLine(string.Join(",", new[]
                {
                    Csv(r.Site), Csv(r.Product), Csv(r.Reviewer), Csv(r.Role),
                    Csv(r.Date), Csv(r.Rating), Csv(r.Pros), Csv(r.Cons)
                }));
            }
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);

            static string Csv(string s) => $"\"{s.Replace("\"", "\"\"")}\"";
        }

        private sealed class ReviewRow
        {
            public string Site { get; set; } = "";
            public string Product { get; set; } = "";
            public string Reviewer { get; set; } = "";
            public string Role { get; set; } = "";
            public string Date { get; set; } = "";
            public string Rating { get; set; } = "";
            public string Pros { get; set; } = "";
            public string Cons { get; set; } = "";
        }
    }
}
