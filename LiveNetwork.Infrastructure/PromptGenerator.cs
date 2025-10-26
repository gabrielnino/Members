using LiveNetwork.Application.Services;
using LiveNetwork.Domain;

namespace LiveNetwork.Infrastructure.Services
{
    public class PromptGenerator() : IPromptGenerator
    {


        /// <summary>
        /// Generates up to MaxInvites first-time invites, with detailed step-by-step logging.
        /// </summary>
        public async Task GeneratPrompt() // kept for compatibility
        {
            var promptBuilder = GetAIPromptBuilder();
            // Example of conversation history (small context fragments)
            //promptBuilder.AddToConversationHistory("user", "Focus on e-Transfer reconciliation and late payments.");
            //promptBuilder.AddToConversationHistory("assistant", "Acknowledged. Will elevate cash-flow pains.");

            var json = promptBuilder.GetApiRequestJson();
        }

        private static AIPromptBuilder GetAIPromptBuilder()
        {
            var meta = GetMeta();
            var searchCriteria = GetSearchCriteria();
            var focus = GetFocus();
            var reviewMetadata = GetReviewMeta();
            var ctx = GetContext(meta, searchCriteria, focus, reviewMetadata);
            string OutputFormatSmallBizV2 = GetJsonFormat();

            var promptBuilder = new AIPromptBuilder
            {
                Role = GetRole(),
                Task = GetTask("Capterra", "", "pain-focused", "a concise JSON report emphasizing pros, cons, and comparison insights"),
                Tone = "professional",
                Style = "concise",
                IncludeSources = true,
                StepByStep = false,
                Format = OutputFormatSmallBizV2,

                // New review metadata properties
                MinReviewCount = 50,
                MaxReviewCount = 200,
                ReviewsPublishedAfter = new DateTime(2023, 1, 1),
                ReviewsPublishedBefore = DateTime.Now,
                ReviewDateRanges = ["last-24-months", "2022-2024"],
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

            promptBuilder.AddConstraint("Return a single valid JSON object exactly matching the OutputFormatSmallBizV2 structure.");
            promptBuilder.AddConstraint("Populate all sections dynamically from reviews that PASS filters_applied; do not invent sources.");
            promptBuilder.AddConstraint("All dates in ISO-8601; numbers as numbers; booleans as booleans.");
            promptBuilder.AddConstraint("If a section has no data after filtering, return an empty array for that section (do not omit the key).");
            promptBuilder.AddConstraint("Include diagnostics.searchQueries and any searchUrls used; include counts in totals derived from the filtered set only.");


            return promptBuilder;
        }

        private static string GetTask(string site, string keywords, string analysisFocus, string outputExpectation)
        {
            //return "Do a search for these terms on Capterra and deliver a pain-focused analysis with the four required sections (Top Product Matches, Most Common Pains & Complaints, Canadian-Specific Pains, Synthesized Pain Points Summary).";

            return
                $"Do a search for these terms on {site} — specifically: {keywords}. " +
                $"Deliver a {analysisFocus} analysis with the four required sections " +
                $"(Top Product Matches, Most Common Pains & Complaints, {site}-Specific Pains, Synthesized Pain Points Summary). " +
                $"The output must be {outputExpectation}.";
        }

        private static string GetRole()
        {
            return "You are a professional market research analyst specializing in software evaluation for small professional service businesses and independent consultants in Canada.";
        }

        private static string GetJsonFormat()
        {
            return @"
            {
              ""metadata"": {
                ""version"": ""v1.2"",
                ""generatedAtUtc"": ""<ISO-8601-UTC>"",
                ""source"": ""Capterra"",
                ""notes"": ""All sections are dynamic and must reflect only reviews that pass the filters_applied criteria.""
              },

              ""contextEcho"": {
                ""Meta"": {
                  ""ReviewOrigin"": ""Reviews written by users located in Vancouver, BC, Canada"",
                  ""Source"": ""Capterra"",
                  ""Audience"": ""Small businesses & independent consultants"",
                  ""Version"": ""v1.1""
                },
                ""SearchCriteria"": {
                  ""Software"": [""QuickBooks"", ""Xero"", ""FreshBooks"", ""Wave"", ""Sage"", ""Zoho Books"", ""Toggl"", ""Harvest""],
                  ""Features"": [""proposals/estimates"", ""invoicing"", ""timesheets"", ""spreadsheets/manual"", ""reconciliation"", ""e-Transfer"", ""receipt capture"", ""client intake"", ""billing"", ""timesheet"", ""spreadsheet"", ""manual billing"", ""manual entry""],
                  ""Segments"": [""independent consultants"", ""freelancers"", ""micro-agencies"", ""small firms (2–10)"", ""bookkeeping"", ""law firm"", ""accounting"", ""self-employee bookkeeping"", ""self-employee law firm"", ""self-employee accounting"", ""independent bookkeeping"", ""independent lawyer"", ""independent accounting""],
                  ""Locations"": [""Canada"", ""BC"", ""Vancouver"", ""CRA""],
                  ""MustMentionAny"": [""Canada"", ""BC"", ""Vancouver"", ""CRA""]
                },
                ""ReviewMetadata"": {
                  ""MinReviewCount"": 50,
                  ""MaxReviewCount"": 200,
                  ""PublishedAfter"": ""2023-01-01"",
                  ""PublishedBefore"": ""<ISO-8601-LOCAL-NOW>"",
                  ""DateRanges"": [""last-12-months"", ""2023-2024""],
                  ""MinimumRating"": 3,
                  ""MaximumRating"": 5
                }
              },

              ""filters_applied"": {
                ""reviewCount"": { ""min"": 50, ""max"": 200 },
                ""publishedRange"": { ""after"": ""2022-01-01"", ""before"": ""<ISO-8601-LOCAL-NOW>"", ""ranges"": [""last-24-months"", ""2023-2024""] },
                ""ratingRange"": { ""min"": 3, ""max"": 5 },
                ""locationsMustMentionAny"": [""Canada"", ""BC"", ""Vancouver"", ""CRA""]
              },

              ""totals"": {
                ""productsReviewed"": 0,
                ""reviewsConsidered"": 0,
                ""reviewsExcluded"": 0,
                ""exclusionsByReason"": [
                  { ""reason"": ""outside_date_range"", ""count"": 0 },
                  { ""reason"": ""rating_out_of_range"", ""count"": 0 },
                  { ""reason"": ""no_canada_signal"", ""count"": 0 }
                ],
                ""dataWindow"": {
                  ""earliestReviewDate"": ""<ISO-8601>"",
                  ""latestReviewDate"": ""<ISO-8601>""
                }
              },

              ""topProductMatches"": [
                {
                  ""name"": """",
                  ""category"": """",
                  ""primaryFunction"": """",
                  ""whyMatch"": """",
                  ""integrations"": [""QuickBooks"", ""Xero""],
                  ""reviewCounts"": { ""total"": 0, ""canadaTagged"": 0 },
                  ""avgRating"": { ""overall"": 0.0, ""canadaTagged"": 0.0 },
                  ""canadaSupport"": {
                    ""taxes"": [""GST"", ""HST"", ""PST"", ""QST""],
                    ""trustAccounting"": false,
                    ""eTransferHandling"": ""native|via_bank_feed|manual"",
                    ""evidence"": [
                      { ""type"": ""review"", ""date"": ""<ISO-8601>"", ""snippet"": """", ""url"": """" }
                    ]
                  },
                  ""links"": { ""homepage"": """", ""capterra"": """" }
                }
              ],

              ""mostCommonPainsAndComplaints"": [
                {
                  ""pain"": """",
                  ""severity"": 0.0,
                  ""frequency"": 0.0,
                  ""who"": [""freelancers"", ""micro-agencies"", ""law firms""],
                  ""products"": [""QuickBooks"", ""Clio""],
                  ""hypotheticalQuote"": """",
                  ""businessImpact"": """",
                  ""evidence"": [
                    { ""product"": """", ""date"": ""<ISO-8601>"", ""rating"": 0, ""snippet"": """", ""url"": """" }
                  ]
                }
              ],

              ""canadianSpecificPains"": [
                {
                  ""pain"": """",
                  ""relatedTo"": [""GST"", ""HST"", ""PST"", ""QST"", ""CRA"", ""e-Transfer""],
                  ""whyUniqueInCanada"": """",
                  ""products"": [""Xero"", ""FreshBooks""],
                  ""hypotheticalQuote"": """",
                  ""evidence"": [
                    { ""product"": """", ""date"": ""<ISO-8601>"", ""rating"": 0, ""snippet"": """", ""url"": """" }
                  ]
                }
              ],

              ""synthesizedPainPointsSummary"": [
                {
                  ""rank"": 1,
                  ""pain"": """",
                  ""rationale"": """",
                  ""opportunity"": """",
                  ""confidence"": 0.0
                }
              ],

              ""charts"": {
                ""marketCoverage"": {
                  ""description"": ""Bubble points derived from filtered data only."",
                  ""points"": [
                    { ""bucket"": ""Accounting/Invoicing"", ""x"": 0.0, ""y"": 0.0, ""r"": 0.0, ""label"": ""Xero"" }
                  ]
                },
                ""painPoints"": {
                  ""description"": ""Severity vs. frequency (0–10) for top pains after filters."",
                  ""labels"": [""Integration Issues (Time→Invoice)"", ""CRA Compliance"", ""e-Transfer Reconciliation""],
                  ""values"": [0.0, 0.0, 0.0]
                }
              },

              ""opportunities"": [
                {
                  ""title"": """",
                  ""level"": ""high|medium|low"",
                  ""description"": """",
                  ""stats"": { ""satisfactionGap"": 0.0, ""marketValueUsd"": 0, ""competition"": ""Low|Medium|High"" }
                }
              ],

              ""recommendation"": {
                ""title"": """",
                ""score"": 0.0,
                ""description"": """",
                ""features"": [
                  ""End-to-end proposals→invoice with one-click tax setup"",
                  ""Automated e-Transfer and bank-feed reconciliation""
                ]
              },

              ""diagnostics"": {
                ""searchQueries"": [""(quickbooks) AND (billing OR timesheet OR reconciliation) AND (law firm OR accounting OR bookkeeping) AND (Canada OR BC OR Vancouver OR CRA)""],
                ""searchUrls"": [""""],
                ""assumptions"": [""Severity/frequency scored from filtered sample only.""],
                ""warnings"": [""If filtered reviews < 50, mark low confidence.""]
              }
            }
            ";
        }

        private static ContextBundle GetContext(
            MetaSection meta, 
            SearchCriteriaSection searchCriteria, 
            FocusSection focus, 
            ReviewMetadataSection reviewMetadata)
        {
            return new ContextBundle(
                Meta: meta,
                SearchCriteria: searchCriteria,
                Focus: focus,
                ReviewMetadata: reviewMetadata
            );
        }

        private static ReviewMetadataSection GetReviewMeta()
        {
            return new ReviewMetadataSection(
                MinReviewCount: 150,
                MaxReviewCount: 350,
                PublishedAfter: new DateTime(2023, 1, 1),
                PublishedBefore: DateTime.Now,
                DateRanges: ["last-24-months", "2022-2024"],
                MinimumRating: 3,
                MaximumRating: 5
            );
        }

        private static FocusSection GetFocus()
        {
            return new FocusSection(
                ResearchGoal: "Identify pains & opportunities in billing/time/tax workflows for small professional services.",
                MustDeliver: ["Top Product Matches", "Most Common Pains & Complaints", "Canadian-Specific Pains", "Synthesized Pain Points Summary"],
                Exclusions: ["Exclude ERP/enterprise-only tools unless reviews clearly mention small teams"]);
        }

        private static SearchCriteriaSection GetSearchCriteria()
        {
            return new SearchCriteriaSection(
                Software:
                [
                    "QuickBooks",
                    "Xero",
                    "FreshBooks",
                    "Wave",
                    "Sage",
                    "Zoho Books",
                    "Toggl",
                    "Harvest"
                ],
                Features:
                [
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
                Segments:
                [
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
        }

        private static MetaSection GetMeta()
        {
            return new MetaSection(
                ReviewOrigin: "Reviews written by users located in Vancouver, BC, Canada",
                Source: "Capterra",
                Audience: "Small businesses & independent consultants",
                Version: "v1.1");
        }
    }
}
