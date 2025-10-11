using Configuration;
using LiveNetwork.Application.Services;
using LiveNetwork.Domain;
using Microsoft.Extensions.Logging;
using Services.Interfaces;

namespace LiveNetwork.Infrastructure.Services
{
    public class PromptGenerator(
        IOpenAIClient openAIClient,
        ILogger<PromptGenerator> logger,
        ITrackingService trackingService,
        AppConfig config) : IPromptGenerator
    {
        private readonly IOpenAIClient _openAIClient = openAIClient ?? throw new ArgumentNullException(nameof(openAIClient));
        private readonly ILogger<PromptGenerator> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly ITrackingService _trackingService = trackingService ?? throw new ArgumentNullException(nameof(trackingService));
        private readonly AppConfig _config = config ?? throw new ArgumentNullException(nameof(config));

        private static readonly Random _random = new();
        private readonly int MaxInvites = 100; // Limit to 100 invites per run

        /// <summary>
        /// Generates up to MaxInvites first-time invites, with detailed step-by-step logging.
        /// </summary>
        public async Task GeneratPrompt() // kept for compatibility
        {
            var meta = new MetaSection(
                ReviewOrigin: "Reviews written by users located in Vancouver, BC, Canada",
                Source: "Capterra",
                Audience: "Small businesses & independent consultants",
                Version: "v1.1");

            var searchCriteria = new SearchCriteriaSection(
                Software: [
                                "QuickBooks", 
                                "Xero", 
                                "FreshBooks", 
                                "Wave", 
                                "Sage", 
                                "Zoho Books", 
                                "Toggl", 
                                "Harvest"
                          ],
                Features: [
                                "proposals/estimates", 
                                "invoicing", 
                                "timesheets", 
                                "spreadsheets/manual", 
                                "reconciliation", 
                                "e-Transfer", 
                                "receipt capture",
                                "client intake", 
                                "billing", 
                                "timesheet", 
                                "spreadsheet",
                                "manual billing",
                                "manual entry",
                         ],
                Segments: [
                                "independent consultants", 
                                "freelancers", 
                                "micro-agencies", 
                                "small firms (2–10)",
                                "bookkeeping",
                                "law firm",
                                "accounting",
                                "self-employee bookkeeping",
                                "self-employee law firm",
                                "self-employee accounting",
                                "independent bookkeeping",
                                "independent lawyer",
                                "independent accounting",
                          ],
                Locations: 
                          [
                                "Canada", 
                                "BC", 
                                "Vancouver", 
                                "CRA"
                          ],
                MustMentionAny: 
                          [
                                "Canada", 
                                "BC", 
                                "Vancouver", 
                                "CRA"
                          ]);

            var focus = new FocusSection(
                ResearchGoal: "Identify pains & opportunities in billing/time/tax workflows for small professional services.",
                MustDeliver: ["Top Product Matches", "Most Common Pains & Complaints", "Canadian-Specific Pains", "Synthesized Pain Points Summary"],
                Exclusions: ["Exclude ERP/enterprise-only tools unless reviews clearly mention small teams"]);

            var reviewMetadata = new ReviewMetadataSection(
       MinReviewCount: 50,
       MaxReviewCount: 200,
       PublishedAfter: new DateTime(2023, 1, 1),
       PublishedBefore: DateTime.Now,
       DateRanges: new[] { "last-12-months", "2023-2024" },
       MinimumRating: 3,
       MaximumRating: 5
   );


            var ctx = new ContextBundle(
    Meta: meta,
    SearchCriteria: searchCriteria,
    Focus: focus,
    ReviewMetadata: reviewMetadata);

            const string OutputFormatSmallBiz = @"{
              ""searchCriteria"": {
                ""softwareFeatures"": [
                  ""Proposals & Estimates"",
                  ""Invoicing & Timesheets"",
                  ""Manual Entry / Spreadsheets"",
                  ""Reconciliation & Payments""
                ],
                ""segments"": {
                  ""SmallBusinesses"": [""Small Professional Services"", ""Micro-Agencies"", ""Small Firms (2–10)""],
                  ""IndependentConsultants"": [""Independent Consultants"", ""Freelancers"", ""Solo Practitioners""]
                },
                ""locations"": [""Canada"", ""BC / Vancouver"", ""CRA References""]
              },
              ""topProductMatches"": [],
              ""mostCommonPainsAndComplaints"": [],
              ""canadianSpecificPains"": [],
              ""synthesizedPainPointsSummary"": [],
              ""charts"": {
                ""marketCoverage"": {
                  ""description"": ""This chart shows how well different software categories serve the market. The upper right quadrant represents the best opportunities."",
                  ""datasets"": [
                    { ""label"": ""Proposal/Project Mgmt"", ""data"": [ { ""x"": 35, ""y"": 60, ""r"": 14 }, { ""x"": 62, ""y"": 48, ""r"": 12 }, { ""x"": 72, ""y"": 68, ""r"": 18 } ],
                      ""backgroundColor"": ""rgba(52, 152, 219, 0.7)"", ""borderColor"": ""rgba(52, 152, 219, 1)"" },
                    { ""label"": ""Accounting/Invoicing"", ""data"": [ { ""x"": 68, ""y"": 75, ""r"": 18 }, { ""x"": 84, ""y"": 58, ""r"": 15 }, { ""x"": 70, ""y"": 50, ""r"": 12 } ],
                      ""backgroundColor"": ""rgba(46, 204, 113, 0.7)"", ""borderColor"": ""rgba(46, 204, 113, 1)"" },
                    { ""label"": ""Opportunity Areas"", ""data"": [ { ""x"": 28, ""y"": 82, ""r"": 24 }, { ""x"": 32, ""y"": 86, ""r"": 20 } ],
                      ""backgroundColor"": ""rgba(231, 76, 60, 0.7)"", ""borderColor"": ""rgba(231, 76, 60, 1)"" }
                  ]
                },
                ""painPoints"": {
                  ""description"": ""Areas with high pain severity and frequency represent the best opportunities for new solutions."",
                  ""labels"": [ ""Integration Issues (Time→Invoice)"", ""CRA Compliance (GST/HST/PST/QST)"", ""Late Payments & Cash Flow"", ""Billing Complexity"", ""Learning Curve"", ""e-Transfer Reconciliation"" ],
                  ""data"": [ 8.6, 9.0, 8.4, 7.6, 7.1, 8.2 ],
                  ""backgroundColors"": [ ""rgba(231, 76, 60, 0.7)"", ""rgba(231, 76, 60, 0.7)"", ""rgba(243, 156, 18, 0.7)"", ""rgba(243, 156, 18, 0.7)"", ""rgba(52, 152, 219, 0.7)"", ""rgba(231, 76, 60, 0.7)"" ],
                  ""borderColors"": [ ""rgba(231, 76, 60, 1)"", ""rgba(231, 76, 60, 1)"", ""rgba(243, 156, 18, 1)"", ""rgba(243, 156, 18, 1)"", ""rgba(52, 152, 219, 1)"", ""rgba(231, 76, 60, 1)"" ]
                }
              },
              ""opportunities"": [
                { ""title"": ""Canadian Freelancer Billing Suite"", ""level"": ""high"", ""description"": ""Simple proposals→time→invoice flow with built-in GST/HST/PST handling, automated reminders, and e-Transfer reconciliation."",
                  ""stats"": { ""satisfactionGap"": ""81%"", ""marketValue"": ""$14M"", ""competition"": ""Low"" } },
                { ""title"": ""Proposal-to-Payment Automation (Canada)"", ""level"": ""high"", ""description"": ""Unified proposal acceptance, scope→tasks, time capture, and instant invoicing with CRA-ready taxes and deposits."",
                  ""stats"": { ""satisfactionGap"": ""77%"", ""marketValue"": ""$16M"", ""competition"": ""Medium"" } },
                { ""title"": ""Cash Flow & Receivables Coach"", ""level"": ""medium"", ""description"": ""Predictive cash flow + automated dunning, partial payments, and bank feed/e-Transfer matching for micro-firms."",
                  ""stats"": { ""satisfactionGap"": ""70%"", ""marketValue"": ""$8M"", ""competition"": ""Medium"" } }
              ],
              ""recommendation"": {
                ""title"": ""Canadian Freelancer Billing Suite"",
                ""score"": ""9.1/10"",
                ""description"": ""Highest pain concentration around tax compliance, reconciliation, and late payments for freelancers and micro-agencies; low specialization among incumbents creates a strong entry point."",
                ""features"": [
                  ""End-to-end proposals→invoice with one-click tax setup"",
                  ""Automated e-Transfer and bank-feed reconciliation"",
                  ""Cash-flow reminders and deposits/retainers"",
                  ""Tight time-tracking→invoicing integrations"",
                  ""CRA/QBO/Xero-friendly exports and reports""
                ]
              }
            }";

            var promptBuilder = new AIPromptBuilder
            {
                Role = "You are a professional market research analyst specializing in software evaluation for small professional service businesses and independent consultants in Canada.",
                Task = "Do a search for these terms on Capterra and deliver a pain-focused analysis with the four required sections (Top Product Matches, Most Common Pains & Complaints, Canadian-Specific Pains, Synthesized Pain Points Summary).",
                Tone = "professional",
                Style = "concise",
                IncludeSources = true,
                StepByStep = false,
                Format = OutputFormatSmallBiz,

                // New review metadata properties
                MinReviewCount = 50,
                MaxReviewCount = 200,
                ReviewsPublishedAfter = new DateTime(2023, 1, 1),
                ReviewsPublishedBefore = DateTime.Now,
                ReviewDateRanges = ["last-12-months", "2023-2024"],
                MinimumReviewRating = 3,
                MaximumReviewRating = 5
            }
            .WithJsonContext(ctx)
            .WithDefaultReviewConstraints()
            .WithExamplesSmallBiz()
            // Using the new extension methods
            .WithReviewQuantity(50, 200)
            .WithRecentReviews(12)
            .WithReviewRatingRange(3, 5);

            // Optional parameters to log or tune behavior
            promptBuilder.AddParameter("scoring.hi_severity_weight", "0.6");
            promptBuilder.AddParameter("scoring.hi_frequency_weight", "0.4");
            promptBuilder.AddParameter("output.language", "en");

            // Example of conversation history (small context fragments)
            promptBuilder.AddToConversationHistory("user", "Focus on e-Transfer reconciliation and late payments.");
            promptBuilder.AddToConversationHistory("assistant", "Acknowledged. Will elevate cash-flow pains.");

            var json = promptBuilder.GetApiRequestJson();
        }
    }
}
