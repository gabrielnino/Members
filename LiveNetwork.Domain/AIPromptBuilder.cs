using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace LiveNetwork.Domain
{
    public class AIPromptBuilder
    {
        public static string StepTag
        {
            get { return "###ResultPreviousStep##"; }
        }
        public string? Role { get; set; } = "assistant";
        public string? Task { get; set; }
        public string? Context { get; set; }
        public string? Format { get; set; }
        public string? Tone { get; set; } = "professional";
        public string? Style { get; set; } = "concise";
        public int? MaxLength { get; set; }
        public bool? IncludeSources { get; set; }
        public bool? StepByStep { get; set; }
        public int? Step
        {
            get
            {
                return Parent == null ? 1 : Parent.Step + 1; ;
            }
        }
        public List<string> Examples { get; } = [];
        public List<string> Constraints { get; } = [];
        public Dictionary<string, string> AdditionalParameters { get; } = [];
        public List<ChatMessage> ConversationHistory { get; } = [];
        public class ChatMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; }

            [JsonPropertyName("content")]
            public string Content { get; }

            public ChatMessage(string role, string content)
            {
                Role = role;
                Content = content;
            }
        }
        public AIPromptBuilder? Parent { get; private set; }

        public void SetParent(AIPromptBuilder parent)
        {
            Parent = parent;
        }
        private AIPromptBuilder? _nextTask;
        public AIPromptBuilder? NextTask
        {
            get => _nextTask;
            set
            {
                _nextTask = value;
                _nextTask?.SetParent(this);
            }
        }
        public void AddExample(string example) => Examples.Add(example);
        public void AddConstraint(string constraint) => Constraints.Add(constraint);
        public void AddParameter(string key, string value) => AdditionalParameters[key] = value;
        public void AddToConversationHistory(string role, string content) => ConversationHistory.Add(new ChatMessage(role, content));
        public void ClearConversationHistory() => ConversationHistory.Clear();
        public int? MinReviewCount { get; set; }
        public int? MaxReviewCount { get; set; }
        public DateTime? ReviewsPublishedAfter { get; set; }
        public DateTime? ReviewsPublishedBefore { get; set; }
        public string[]? ReviewDateRanges { get; set; }
        public int? MinimumReviewRating { get; set; }
        public int? MaximumReviewRating { get; set; }
        public string BuildPrompt()
        {
            var sb = new StringBuilder();
            AppendLineIfNotNull(sb, "Role", Role);
            AppendLineIfNotNull(sb, "Task", Task);
            AppendLineIfNotNull(sb, "Context", Context);
            AppendLineIfNotNull(sb, "Response Format", Format);
            AppendList(sb, "Examples", Examples);
            AppendList(sb, "Constraints", Constraints);

            // Review metadata section
            AppendReviewMetadata(sb);

            sb.AppendLine($"Tone: {Tone}");
            sb.AppendLine($"Style: {Style}");

            if (MaxLength.HasValue)
            {
                sb.AppendLine($"Maximum length: {MaxLength} words");
            }
            if (IncludeSources ?? false)
            {
                sb.AppendLine("Include relevant sources or references.");
            }
            if (StepByStep ?? false)
            {
                sb.AppendLine("Provide step-by-step response.");
            }
            if (AdditionalParameters.Count > 0)
            {
                sb.AppendLine("Additional Parameters:");
                foreach (var (key, value) in AdditionalParameters)
                {
                    sb.AppendLine($"{key}: {value}");
                }
            }
            return sb.ToString();
        }
        public Prompt BuildPromptObject(string? result = null)
        {
            var systemBuilder = new StringBuilder();
            AppendLineIfNotNull(systemBuilder, "Task", Task);
            AppendLineIfNotNull(systemBuilder, "Context", Context);
            AppendList(systemBuilder, "Constraints", Constraints);

            var userBuilder = new StringBuilder();
            AppendLineIfNotNull(userBuilder, "Role", Role);
            AppendLineIfNotNull(userBuilder, "Response Format", Format);
            AppendList(userBuilder, "Examples", Examples);

            // Add review metadata to user content
            AppendReviewMetadata(userBuilder);

            userBuilder.AppendLine($"Tone: {Tone}");
            userBuilder.AppendLine($"Style: {Style}");

            if (MaxLength.HasValue)
            {
                userBuilder.AppendLine($"Maximum length: {MaxLength} words");
            }
            if (IncludeSources ?? false)
            {
                userBuilder.AppendLine("Include relevant sources or references.");
            }
            if (StepByStep ?? false)
            {
                userBuilder.AppendLine("Provide step-by-step response.");
            }
            if (AdditionalParameters.Count > 0)
            {
                userBuilder.AppendLine("Additional Parameters:");
                foreach (var (key, value) in AdditionalParameters)
                {
                    userBuilder.AppendLine($"{key}: {value}");
                }
            }

            return new Prompt
            {
                SystemContent = systemBuilder.ToString().Trim().Replace(AIPromptBuilder.StepTag, result ?? string.Empty),
                UserContent = userBuilder.ToString().Trim(),
            };
        }
        private void AppendReviewMetadata(StringBuilder sb)
        {
            if (MinReviewCount.HasValue || MaxReviewCount.HasValue)
            {
                var countText = MinReviewCount.HasValue && MaxReviewCount.HasValue
                    ? $"{MinReviewCount}-{MaxReviewCount} reviews"
                    : MinReviewCount.HasValue
                        ? $"at least {MinReviewCount} reviews"
                        : $"up to {MaxReviewCount} reviews";
                sb.AppendLine($"Review Quantity: {countText}");
            }

            if (ReviewsPublishedAfter.HasValue || ReviewsPublishedBefore.HasValue)
            {
                var dateRange = string.Empty;
                if (ReviewsPublishedAfter.HasValue && ReviewsPublishedBefore.HasValue)
                {
                    dateRange = $"between {ReviewsPublishedAfter.Value:yyyy-MM-dd} and {ReviewsPublishedBefore.Value:yyyy-MM-dd}";
                }
                else if (ReviewsPublishedAfter.HasValue)
                {
                    dateRange = $"published after {ReviewsPublishedAfter.Value:yyyy-MM-dd}";
                }
                else if (ReviewsPublishedBefore.HasValue)
                {
                    dateRange = $"published before {ReviewsPublishedBefore.Value:yyyy-MM-dd}";
                }
                sb.AppendLine($"Review Dates: {dateRange}");
            }

            if (ReviewDateRanges?.Length > 0)
            {
                sb.AppendLine($"Review Date Ranges: {string.Join(", ", ReviewDateRanges)}");
            }

            if (MinimumReviewRating.HasValue || MaximumReviewRating.HasValue)
            {
                var ratingText = MinimumReviewRating.HasValue && MaximumReviewRating.HasValue
                    ? $"{MinimumReviewRating}-{MaximumReviewRating} stars"
                    : MinimumReviewRating.HasValue
                        ? $"at least {MinimumReviewRating} stars"
                        : $"up to {MaximumReviewRating} stars";
                sb.AppendLine($"Review Ratings: {ratingText}");
            }
        }
        public List<ChatMessage> GetApiMessages()
        {
            var messages = new List<ChatMessage>();
            var systemBuilder = new StringBuilder();
            AppendLineIfNotNull(systemBuilder, "Task", Task);
            AppendLineIfNotNull(systemBuilder, "Context", Context);
            AppendList(systemBuilder, "Constraints", Constraints);
            if (systemBuilder.Length > 0)
            {
                messages.Add(new ChatMessage("system", systemBuilder.ToString()));
            }
            if (Examples.Count > 0)
            {
                var examplesText = string.Join(Environment.NewLine, Examples.Select(e => $"- {e}"));
                messages.Add(new ChatMessage("user", $"Examples:\n{examplesText}"));
            }
            messages.Add(new ChatMessage("user", BuildPrompt()));
            messages.AddRange(ConversationHistory);
            return messages;
        }

        public string GetApiRequestJson(bool includeSystemMessage = true)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping // <- key line
            };
            return JsonSerializer.Serialize(new { messages = GetApiMessages() }, options);
        }

        public string ExportToJsonString(bool includeFormatting = true)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = includeFormatting,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            // Create a serializable DTO to avoid circular references
            var exportData = new AIPromptBuilderExportDto(this);

            return JsonSerializer.Serialize(exportData, options);
        }


        private static void AppendLineIfNotNull(StringBuilder sb, string label, string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                sb.AppendLine($"{label}: {value}");
            }
        }

        private static void AppendList(StringBuilder sb, string title, List<string> items)
        {
            if (items?.Count > 0)
            {
                sb.AppendLine($"{title}:");
                foreach (var item in items)
                    sb.AppendLine($"- {item}");
            }
        }

        private class AIPromptBuilderExportDto
        {
            public string? Role { get; set; }
            public string? Task { get; set; }
            public string? Context { get; set; }
            public string? Format { get; set; }
            public string? Tone { get; set; }
            public string? Style { get; set; }
            public int? MaxLength { get; set; }
            public bool? IncludeSources { get; set; }
            public bool? StepByStep { get; set; }
            public List<string> Examples { get; set; } = new();
            public List<string> Constraints { get; set; } = new();
            public Dictionary<string, string> AdditionalParameters { get; set; } = new();
            public List<ChatMessage> ConversationHistory { get; set; } = new();
            public AIPromptBuilderExportDto? NextTask { get; set; }

            public int? MinReviewCount { get; set; }
            public int? MaxReviewCount { get; set; }
            public DateTime? ReviewsPublishedAfter { get; set; }
            public DateTime? ReviewsPublishedBefore { get; set; }
            public string[]? ReviewDateRanges { get; set; }
            public int? MinimumReviewRating { get; set; }
            public int? MaximumReviewRating { get; set; }

            public AIPromptBuilderExportDto() { }

            public AIPromptBuilderExportDto(AIPromptBuilder builder)
            {
                Role = builder.Role;
                Task = builder.Task;
                Context = builder.Context;
                Format = builder.Format;
                Tone = builder.Tone;
                Style = builder.Style;
                MaxLength = builder.MaxLength;
                IncludeSources = builder.IncludeSources;
                StepByStep = builder.StepByStep;
                Examples = new List<string>(builder.Examples);
                Constraints = new List<string>(builder.Constraints);
                AdditionalParameters = new Dictionary<string, string>(builder.AdditionalParameters);
                ConversationHistory = new List<ChatMessage>(builder.ConversationHistory);
                MinReviewCount = builder.MinReviewCount;
                MaxReviewCount = builder.MaxReviewCount;
                ReviewsPublishedAfter = builder.ReviewsPublishedAfter;
                ReviewsPublishedBefore = builder.ReviewsPublishedBefore;
                ReviewDateRanges = builder.ReviewDateRanges?.ToArray();
                MinimumReviewRating = builder.MinimumReviewRating;
                MaximumReviewRating = builder.MaximumReviewRating;

                if (builder.NextTask != null)
                {
                    NextTask = new AIPromptBuilderExportDto(builder.NextTask);
                }
            }
        }
    }

    public static class PromptBuilderExtensions
    {
        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            WriteIndented = false,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        public static AIPromptBuilder WithJsonContext(this AIPromptBuilder builder, object contextObject)
        {
            builder.Context = JsonSerializer.Serialize(contextObject, _jsonOpts);
            return builder;
        }

        public static AIPromptBuilder WithDefaultReviewConstraints(this AIPromptBuilder builder)
        {
            builder.AddConstraint("Only use reviews that explicitly mention Canada, BC, Vancouver, or CRA.");
            builder.AddConstraint("Prefer reviews from small businesses, freelancers, independent consultants, or micro-agencies (2–10 people).");
            builder.AddConstraint("Extract pains tied to proposals→time→invoice, e-Transfer reconciliation, CRA taxes (GST/HST/PST/QST).");
            builder.AddConstraint("For each pain, include a realistic hypothetical quote and a clear business impact.");
            builder.AddConstraint("Avoid quoting long text verbatim; summarize faithfully.");
            return builder;
        }

        public static AIPromptBuilder WithExamplesSmallBiz(this AIPromptBuilder builder)
        {
            builder.AddExample("Example query: (\"quickbooks\" OR \"freshbooks\" OR \"xero\" OR \"wave\" OR \"zoho books\" OR \"toggl\" OR \"harvest\") AND (\"consultant\" OR \"freelancer\" OR \"independent\" OR \"small business\") AND (\"invoice\" OR \"timesheet\" OR \"reconciliation\" OR \"spreadsheet\" OR \"manual\") AND (\"canada\" OR \"bc\" OR \"vancouver\" OR \"cra\")");
            builder.AddExample("Pain framing: “I export hours from Toggl to CSV, then re-type in QuickBooks.” Impact: double entry, billing delays, cash-flow risk.");
            return builder;
        }

        public static AIPromptBuilder WithReviewQuantity(this AIPromptBuilder builder, int minCount, int? maxCount = null)
        {
            builder.MinReviewCount = minCount;
            builder.MaxReviewCount = maxCount;
            return builder;
        }

        public static AIPromptBuilder WithReviewDateRange(this AIPromptBuilder builder, DateTime? after = null, DateTime? before = null)
        {
            builder.ReviewsPublishedAfter = after;
            builder.ReviewsPublishedBefore = before;
            return builder;
        }

        public static AIPromptBuilder WithReviewDateRanges(this AIPromptBuilder builder, params string[] dateRanges)
        {
            builder.ReviewDateRanges = dateRanges;
            return builder;
        }

        public static AIPromptBuilder WithReviewRatingRange(this AIPromptBuilder builder, int? minRating = null, int? maxRating = null)
        {
            builder.MinimumReviewRating = minRating;
            builder.MaximumReviewRating = maxRating;
            return builder;
        }

        public static AIPromptBuilder WithRecentReviews(this AIPromptBuilder builder, int months = 12)
        {
            builder.ReviewsPublishedAfter = DateTime.Now.AddMonths(-months);
            builder.ReviewDateRanges = new[] { $"last-{months}-months" };
            return builder;
        }
    }

    public record ContextBundle(
        MetaSection Meta,
        SearchCriteriaSection SearchCriteria,
        FocusSection Focus,
        ReviewMetadataSection? ReviewMetadata = null); // Added optional ReviewMetadata

    public record ReviewMetadataSection(
    int? MinReviewCount = null,
    int? MaxReviewCount = null,
    DateTime? PublishedAfter = null,
    DateTime? PublishedBefore = null,
    string[]? DateRanges = null, // e.g., ["last-30-days", "last-6-months", "last-year"]
    int? MinimumRating = null,   // 1-5 stars
    int? MaximumRating = null
);

    public record MetaSection(
        string ReviewOrigin,      // <-- replaces Geography
        string Source,            // "Capterra"
        string Audience,          // "Small businesses & independent consultants"
        string Version);          // "v1.1"

    public record SearchCriteriaSection(
        string[] Software,
        string[] Features,
        string[] Segments,
        string[] Locations,
        string[] MustMentionAny);

    public record FocusSection(
        string ResearchGoal,
        string[] MustDeliver,
        string[] Exclusions);
}