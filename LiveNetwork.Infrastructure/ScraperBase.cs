using System.Globalization;
using LiveNetwork.Domain;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace LiveNetwork.Infrastructure.Services
{
    public class ScraperBase
    {
        public static IReadOnlyList<Review> ExtractReviews(
            IWebDriver driver,
            TimeSpan? waitTimeout = null,
            Action<string>? log = null)
        {
            log?.Invoke("[Capterra] => ExtractReviews: inicio");
            var results = new List<Review>();
            var wait = new WebDriverWait(driver, waitTimeout ?? TimeSpan.FromSeconds(10));

            try
            {
                log?.Invoke("[Capterra] Esperando contenedor principal de reviews…");
                var container = wait.Until(ExpectedConditions.ElementExists(
                    By.CssSelector("[data-test-id='review-cards-container']")));

                // Cada card es un hijo directo <div> dentro del contenedor
                var cards = container.FindElements(By.XPath("./div"));
                log?.Invoke($"[Capterra] Encontradas {cards.Count} tarjetas de review.");

                for (int i = 0; i < cards.Count; i++)
                {
                    var card = cards[i];
                    log?.Invoke($"[Capterra] -> Procesando tarjeta #{i + 1}/{cards.Count}");

                    try
                    {
                        var review = new Review();

                        // Reviewer
                        review.Reviewer.Name = TextOrNull(FindFirstOrDefault(card, By.CssSelector("span.typo-20.font-semibold")));
                        var metaBlock = FindFirstOrDefault(card, By.CssSelector(".typo-10.text-neutral-90"));
                        if (metaBlock is not null)
                        {
                            var lines = metaBlock.Text?.Split('\n')
                                .Select(t => t.Trim())
                                .Where(t => !string.IsNullOrWhiteSpace(t))
                                .ToArray() ?? Array.Empty<string>();

                            if (lines.Length > 0) review.Reviewer.Role = lines[0];
                            if (lines.Length > 1) review.Reviewer.Industry = lines[1];
                            if (lines.Length > 2) review.Reviewer.UsedFor = lines[2];
                        }

                        // Avatar
                       // var avatarImg = FindFirstOrDefault(card, By.CssSelector("img[data-testid='reviewer-profile-pic']"));
                        //review.Reviewer.AvatarUrl = avatarImg?.GetAttribute("src");

                        // Título y fecha
                        review.Title = TextOrNull(FindFirstOrDefault(card, By.CssSelector("h3.typo-20.font-semibold")));
                        var dateEl = FindFirstOrDefault(card, By.CssSelector(".typo-0.text-neutral-90"));
                        review.ReviewDate = ParseDateEn(TextOrNull(dateEl));

                        // Rating general
                        var overallBlock = FindFirstOrDefault(card, By.CssSelector("[data-testid='rating'] span.e1xzmg0z.sr2r3oj, [data-testid='rating'] span.sr2r3oj"));
                        review.OverallRating = ParseDouble(TextOrNull(overallBlock));

                        // Desglose ratings
                        review.Ratings.EaseOfUse = ExtractLabeledRating(card, "Ease of Use");
                        review.Ratings.CustomerService = ExtractLabeledRating(card, "Customer Service");
                        review.Ratings.Features = ExtractLabeledRating(card, "Features");
                        review.Ratings.ValueForMoney = ExtractLabeledRating(card, "Value for Money");

                        // Likelihood 0–10
                        //review.LikelihoodToRecommend10 = ExtractLikelihood10(card);

                        // Pros / Cons / Free text
                        review.Pros = ExtractLabeledParagraph(card, "Pros");
                        review.Cons = ExtractLabeledParagraph(card, "Cons");
                        review.FreeText = ExtractFreeTextIfAny(card);

                        // Alternatives / Reason / Switched from
                        //review.AlternativesConsidered = ExtractListUnderLabel(card, "Alternatives considered");
                        //review.ReasonForChoosing = ExtractFollowingParagraph(card, "Reason for choosing");
                        //review.SwitchedFrom = ExtractListUnderLabel(card, "Switched from");
                        //review.SwitchReason = ExtractParagraphAfterLabelBlock(card, "Switched from");

                        results.Add(review);
                        log?.Invoke($"[Capterra] <- Tarjeta #{i + 1}: OK");
                    }
                    catch (Exception exCard)
                    {
                        log?.Invoke($"[Capterra] ! Error parseando tarjeta #{i + 1}: {exCard.Message}");
                    }
                }
            }
            catch (WebDriverTimeoutException tex)
            {
                log?.Invoke($"[Capterra] ! Timeout esperando contenedor de reviews: {tex.Message}");
            }
            catch (NoSuchElementException nse)
            {
                log?.Invoke($"[Capterra] ! No se encontró el contenedor de reviews: {nse.Message}");
            }
            catch (Exception ex)
            {
                log?.Invoke($"[Capterra] ! Error inesperado en ExtractReviews: {ex.Message}");
            }

            log?.Invoke($"[Capterra] <= ExtractReviews: fin. Total recopiladas: {results.Count}");
            return results;
        }

        public static async Task SelectFrequencyDailyAsync(
            IWebDriver driver,
            bool deselectOthers = true,
            TimeSpan? timeout = null,
            CancellationToken ct = default,
            Action<string>? log = null)
        {
            timeout ??= TimeSpan.FromSeconds(12);
            var wait = new WebDriverWait(new SystemClock(), driver, timeout.Value, TimeSpan.FromMilliseconds(150));

            log?.Invoke("[Capterra] => SelectFrequencyDailyAsync: inicio");

            // 1) Abrir combobox
            try
            {
                log?.Invoke("[Capterra] Abriendo combobox 'Frequency of Use'…");
                var trigger = wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("[data-testid='filter-frequencyUsedProduct']")));
                SafeClick(driver, trigger);

                // 2) Esperar diálogo
                log?.Invoke("[Capterra] Esperando diálogo de frecuencia…");
                var dialog = wait.Until(drv =>
                {
                    var dlg = drv.FindElements(By.CssSelector("div[role='dialog']")).FirstOrDefault(e => e.Displayed);
                    return dlg ?? null;
                });

                // 3) Seleccionar Daily
                log?.Invoke("[Capterra] Seleccionando 'Daily'…");
                var daily = wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("[data-testid='filter-frequencyUsedProduct-Daily']")));
                EnsureSelected(driver, daily);
                log?.Invoke("[Capterra] 'Daily' seleccionado.");

                // 4) Deseleccionar otros (opcional)
                if (deselectOthers)
                {
                    log?.Invoke("[Capterra] Deseleccionando otras opciones de frecuencia…");
                    var others = new[]
                    {
                        "filter-frequencyUsedProduct-Weekly",
                        "filter-frequencyUsedProduct-Monthly",
                        "filter-frequencyUsedProduct-Other"
                    };

                    int toggledOff = 0;
                    foreach (var testId in others)
                    {
                        ct.ThrowIfCancellationRequested();
                        var opt = dialog.FindElements(By.CssSelector($"[data-testid='{testId}']")).FirstOrDefault();
                        if (opt != null && IsSelected(opt))
                        {
                            SafeClick(driver, opt);
                            toggledOff++;
                            await Task.Delay(75, ct);
                        }
                    }
                    log?.Invoke($"[Capterra] Otras opciones desactivadas: {toggledOff}");
                }

                // 5) Cerrar dropdown
                log?.Invoke("[Capterra] Cerrando dropdown de frecuencia…");
                try { SafeClick(driver, trigger); }
                catch
                {
                    try
                    {
                        ((IJavaScriptExecutor)driver)
                            .ExecuteScript("document.activeElement?.dispatchEvent(new KeyboardEvent('keydown',{key:'Escape'}));");
                    }
                    catch { /* ignore */ }
                }

                await Task.Delay(200, ct);
                log?.Invoke("[Capterra] <= SelectFrequencyDailyAsync: fin (OK)");
            }
            catch (Exception ex)
            {
                log?.Invoke($"[Capterra] ! Error en SelectFrequencyDailyAsync: {ex.Message}");
                throw;
            }
        }

        private static string? Between(string source, string start, string end)
        {
            if (string.IsNullOrEmpty(source)) return null;
            var i = source.IndexOf(start, StringComparison.OrdinalIgnoreCase);
            if (i < 0) return null;
            i += start.Length;
            var j = source.IndexOf(end, i, StringComparison.OrdinalIgnoreCase);
            if (j < 0) return null;
            return source.Substring(i, j - i);
        }

        private static void EnsureSelected(IWebDriver driver, IWebElement optionEl)
        {
            try
            {
                if (!IsSelected(optionEl))
                {
                    SafeClick(driver, optionEl);
                }
            }
            catch { /* logging de alto nivel ya lo cubre el caller */ }
        }

        private static string? ExtractFollowingParagraph(IWebElement card, string labelPrefix)
        {
            try
            {
                var span = card.FindElements(By.XPath($".//span[starts-with(normalize-space(),'{labelPrefix}')]")).FirstOrDefault();
                if (span is null) return null;

                var p = span.FindElements(By.XPath("./following::p[1]")).FirstOrDefault();
                return TextOrNull(p);
            }
            catch { return null; }
        }

        private static string? ExtractFreeTextIfAny(IWebElement card)
        {
            try
            {
                var block = card.FindElements(By.XPath(".//div[contains(@class,'!mt-4') or contains(@class,'mt-4')]//p")).FirstOrDefault();
                return TextOrNull(block);
            }
            catch { return null; }
        }

        private static string? ExtractLabeledParagraph(IWebElement card, string label)
        {
            try
            {
                var labelSpan = card.FindElements(By.XPath($".//span[normalize-space()='{label}']")).FirstOrDefault();
                if (labelSpan is null) return null;

                var p = labelSpan.FindElements(By.XPath("./ancestor::div[contains(@class,'space-y-2')][1]//p")).FirstOrDefault()
                        ?? labelSpan.FindElements(By.XPath("./following::p[1]")).FirstOrDefault();

                return TextOrNull(p);
            }
            catch { return null; }
        }

        private static double? ExtractLabeledRating(IWebElement card, string label)
        {
            try
            {
                var block = card.FindElements(By.XPath($".//div[.//span[normalize-space()='{label}']]")).FirstOrDefault();
                if (block is null) return null;

                var ratingBlock = FindFirstOrDefault(block, By.CssSelector($"[data-testid='{label}-rating'] span.sr2r3oj"));
                return ParseDouble(TextOrNull(ratingBlock));
            }
            catch { return null; }
        }

        private static int? ExtractLikelihood10(IWebElement card)
        {
            try
            {
                var row = card.FindElements(By.XPath(".//div[.//span[normalize-space()='Likelihood to Recommend']]")).FirstOrDefault();
                if (row is null) return null;

                var progress = FindFirstOrDefault(row, By.CssSelector("progress[max='10']"));
                if (progress != null)
                {
                    var valAttr = progress.GetAttribute("value");
                    if (int.TryParse(valAttr, out var val)) return val;
                }

                var text = row.Text ?? string.Empty;
                var token = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).LastOrDefault(t => t.Contains('/'));
                if (!string.IsNullOrEmpty(token))
                {
                    var left = token.Split('/')[0];
                    if (int.TryParse(left, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsed)) return parsed;
                }

                var bar = FindFirstOrDefault(row, By.CssSelector("[data-testid='Likelihood to Recommend-rating'] .bavdpqa"));
                var style = bar?.GetAttribute("style") ?? string.Empty;
                var pctStr = Between(style, "width:", "%");
                if (pctStr != null && double.TryParse(pctStr.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var pct))
                {
                    return (int)Math.Round(pct / 10.0, MidpointRounding.AwayFromZero);
                }

                return null;
            }
            catch { return null; }
        }

        private static List<string> ExtractListUnderLabel(IWebElement card, string label)
        {
            var items = new List<string>();
            try
            {
                var labelSpan = card.FindElements(By.XPath($".//span[contains(normalize-space(),'{label}')]")).FirstOrDefault();
                if (labelSpan is null) return items;

                var links = labelSpan.FindElements(By.XPath("./ancestor::span[1]/following-sibling::div[1]//a//span")).ToList();
                if (links.Count == 0)
                {
                    links = labelSpan.FindElements(By.XPath("./ancestor::span[1]/following-sibling::div[1]//a//span[contains(@class,'typo-10')]")).ToList();
                }

                foreach (var l in links)
                {
                    var name = TextOrNull(l);
                    if (!string.IsNullOrWhiteSpace(name)) items.Add(name!);
                }
            }
            catch { /* ignore */ }

            return items;
        }

        private static string? ExtractParagraphAfterLabelBlock(IWebElement card, string label)
        {
            try
            {
                var span = card.FindElements(By.XPath($".//span[normalize-space()='{label}']")).FirstOrDefault();
                if (span is null) return null;

                var p = span.FindElements(By.XPath("./ancestor::span[1]/following-sibling::p[1]")).FirstOrDefault()
                        ?? span.FindElements(By.XPath("./following::p[1]")).FirstOrDefault();

                return TextOrNull(p);
            }
            catch { return null; }
        }

        private static IWebElement? FindFirstOrDefault(ISearchContext ctx, By by)
        {
            try { return ctx.FindElements(by).FirstOrDefault(); }
            catch { return null; }
        }

        private static bool IsClickableVisible(IWebElement el)
        {
            try
            {
                if (!el.Displayed) return false;

                var cls = el.GetAttribute("class") ?? string.Empty;
                if (cls.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                       .Any(c => c.Equals("hidden", StringComparison.OrdinalIgnoreCase)))
                    return false;

                var size = el.Size;
                if (size is { Width: <= 0, Height: <= 0 }) return false;

                return true;
            }
            catch { return false; }
        }

        private static bool IsSelected(IWebElement optionEl)
        {
            try
            {
                var cls = optionEl.GetAttribute("class") ?? string.Empty;
                return cls.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                          .Any(c => string.Equals(c, "selected", StringComparison.OrdinalIgnoreCase));
            }
            catch { return false; }
        }

        private static DateTime? ParseDateEn(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;

            var formats = new[] { "MMMM d, yyyy", "MMM d, yyyy", "MMMM dd, yyyy", "MMM dd, yyyy" };
            if (DateTime.TryParseExact(s.Trim(), formats, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out var dt))
                return dt;

            if (DateTime.TryParse(s, CultureInfo.GetCultureInfo("en-US"), DateTimeStyles.None, out dt))
                return dt;

            return null;
        }

        private static double? ParseDouble(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return double.TryParse(s.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var v) ? v : null;
        }

        /// <summary>Clicks an element, falling back to JS click when Selenium's native click is intercepted.</summary>
        private static void SafeClick(IWebDriver driver, IWebElement el)
        {
            try
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView({block:'center'});", el);
                el.Click();
            }
            catch (WebDriverException)
            {
                ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].click();", el);
            }
        }

        private static IWebElement[] SafeFind(ISearchContext scope, By by)
        {
            try { return scope.FindElements(by)?.ToArray() ?? Array.Empty<IWebElement>(); }
            catch { return Array.Empty<IWebElement>(); }
        }

        private static void SafeScrollIntoView(IWebDriver driver, IWebElement el)
        {
            try
            {
                ((IJavaScriptExecutor)driver)
                    .ExecuteScript("arguments[0].scrollIntoView({block:'center', inline:'nearest'});", el);
            }
            catch { /* ignore */ }
        }

        private static string? TextOrNull(IWebElement? el)
        {
            var t = el?.Text?.Trim();
            return string.IsNullOrWhiteSpace(t) ? null : t;
        }

        public async Task<int> ExpandAllContinueReadingAsync(
            IWebDriver driver,
            ISearchContext? scope = null,
            int maxPasses = 5,
            int perClickDelayMs = 120,
            CancellationToken ct = default,
            Action<string>? log = null)
        {
            scope ??= driver;
            int totalClicked = 0;
            log?.Invoke("[Capterra] => ExpandAllContinueReadingAsync: inicio");

            for (int pass = 0; pass < maxPasses; pass++)
            {
                ct.ThrowIfCancellationRequested();
                log?.Invoke($"[Capterra] Pase {pass + 1}/{maxPasses}: buscando botones 'Continue Reading'…");

                var buttons = SafeFind(scope, By.CssSelector("[data-testid='continue-reading-button']"))
                    .Where(IsClickableVisible)
                    .ToList();

                log?.Invoke($"[Capterra] Botones visibles: {buttons.Count}");
                if (buttons.Count == 0) { log?.Invoke("[Capterra] No hay más botones por expandir. Fin anticipado."); break; }

                int clickedThisPass = 0;
                foreach (var btn in buttons)
                {
                    ct.ThrowIfCancellationRequested();

                    if (!IsClickableVisible(btn)) continue;

                    try
                    {
                        SafeScrollIntoView(driver, btn);
                        SafeClick(driver, btn);
                        clickedThisPass++;
                        totalClicked++;
                        await Task.Delay(perClickDelayMs, ct);
                    }
                    catch (Exception exBtn)
                    {
                        log?.Invoke($"[Capterra] ! Error al expandir un botón: {exBtn.Message}");
                    }
                }

                log?.Invoke($"[Capterra] Pase {pass + 1}: botones expandidos = {clickedThisPass}");
                await Task.Delay(200, ct);
            }

            log?.Invoke($"[Capterra] <= ExpandAllContinueReadingAsync: fin. Total expandidos = {totalClicked}");
            return totalClicked;
        }

        public async Task SelectCompanySizeSelfEmployedAsync(
            IWebDriver driver,
            bool deselectOthers = true,
            TimeSpan? timeout = null,
            CancellationToken ct = default,
            Action<string>? log = null)
        {
            timeout ??= TimeSpan.FromSeconds(12);
            var wait = new WebDriverWait(new SystemClock(), driver, timeout.Value, TimeSpan.FromMilliseconds(150));
            log?.Invoke("[Capterra] => SelectCompanySizeSelfEmployedAsync: inicio");

            try
            {
                // 1) Open the "Company Size" combobox
                log?.Invoke("[Capterra] Abriendo combobox 'Company Size'…");
                var trigger = wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("[data-testid='filter-companySize']")));
                SafeClick(driver, trigger);

                // 2) Wait for dialog
                log?.Invoke("[Capterra] Esperando diálogo de tamaños de empresa…");
                var dialog = wait.Until(drv =>
                {
                    var dlg = drv.FindElements(By.CssSelector("div[role='dialog']")).FirstOrDefault(e => e.Displayed);
                    return dlg ?? null;
                });

                // 3) Select "Self-employed"
                log?.Invoke("[Capterra] Seleccionando 'Self-employed'…");
                var selfEmp = wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("[data-testid='filter-companySize-A']")));
                EnsureSelected(driver, selfEmp);
                log?.Invoke("[Capterra] 'Self-employed' seleccionado.");

                // 4) Deselect others
                if (deselectOthers)
                {
                    log?.Invoke("[Capterra] Deseleccionando otros tamaños…");
                    var others = dialog.FindElements(By.CssSelector("[data-dropdown-item='true']"))
                                       .Where(e => !Equals(e.GetAttribute("data-testid"), "filter-companySize-A"))
                                       .ToList();

                    int toggledOff = 0;
                    foreach (var opt in others)
                    {
                        ct.ThrowIfCancellationRequested();
                        try
                        {
                            if (IsSelected(opt))
                            {
                                SafeClick(driver, opt);
                                toggledOff++;
                                await Task.Delay(75, ct);
                            }
                        }
                        catch (Exception exOpt)
                        {
                            log?.Invoke($"[Capterra] ! Error deseleccionando opción: {exOpt.Message}");
                        }
                    }
                    log?.Invoke($"[Capterra] Otras opciones desactivadas: {toggledOff}");
                }

                // 5) Close dropdown
                log?.Invoke("[Capterra] Cerrando dropdown 'Company Size'…");
                try { SafeClick(driver, trigger); }
                catch
                {
                    try
                    {
                        ((IJavaScriptExecutor)driver)
                            .ExecuteScript("document.activeElement?.dispatchEvent(new KeyboardEvent('keydown',{key:'Escape'}));");
                    }
                    catch { /* ignore */ }
                }

                await Task.Delay(200, ct);
                log?.Invoke("[Capterra] <= SelectCompanySizeSelfEmployedAsync: fin (OK)");
            }
            catch (Exception ex)
            {
                log?.Invoke($"[Capterra] ! Error en SelectCompanySizeSelfEmployedAsync: {ex.Message}");
                throw;
            }
        }

        public async Task SelectSortByAsync(
            IWebDriver driver,
            string sortKey = "MOST_HELPFUL",
            TimeSpan? timeout = null,
            CancellationToken ct = default,
            Action<string>? log = null)
        {
            timeout ??= TimeSpan.FromSeconds(12);
            var wait = new WebDriverWait(new SystemClock(), driver, timeout.Value, TimeSpan.FromMilliseconds(150));

            log?.Invoke($"[Capterra] => SelectSortByAsync: inicio (sortKey={sortKey})");

            try
            {
                // 1) Open the combobox
                log?.Invoke("[Capterra] Abriendo combobox 'Sort by'…");
                var display = wait.Until(ExpectedConditions.ElementToBeClickable(
                    By.CssSelector("[data-testid='filters-sort-by']")));
                SafeClick(driver, display);

                // 2) Wait for option
                var optionSelector = By.CssSelector($"[data-testid='filter-sort-{sortKey}']");
                log?.Invoke($"[Capterra] Esperando opción de ordenamiento '{sortKey}'…");
                var option = wait.Until(drv =>
                {
                    var elems = drv.FindElements(optionSelector);
                    return elems.Count > 0 && elems[0].Displayed ? elems[0] : null;
                });

                // 3) Click option
                log?.Invoke($"[Capterra] Seleccionando opción '{sortKey}'…");
                SafeClick(driver, option);

                // 4) Wait until display reflects choice
                var expectedText = sortKey switch
                {
                    "MOST_HELPFUL" => "Most Helpful",
                    "MOST_RECENT" => "Most Recent",
                    "HIGHEST_RATED" => "Highest Rating",
                    "LOWEST_RATED" => "Lowest Rating",
                    _ => "Most Helpful"
                };

                log?.Invoke($"[Capterra] Verificando etiqueta visible del combobox = '{expectedText}'…");
                wait.Until(drv =>
                {
                    try
                    {
                        var span = drv.FindElement(By.CssSelector("[data-testid='filters-sort-by'] span"));
                        return span.Displayed && string.Equals(span.Text?.Trim(), expectedText, StringComparison.OrdinalIgnoreCase);
                    }
                    catch { return false; }
                });

                await Task.Delay(200, ct);
                log?.Invoke("[Capterra] <= SelectSortByAsync: fin (OK)");
            }
            catch (Exception ex)
            {
                log?.Invoke($"[Capterra] ! Error en SelectSortByAsync: {ex.Message}");
                throw;
            }
        }
    }
}
