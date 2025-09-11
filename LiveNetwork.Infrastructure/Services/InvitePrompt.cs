using System.Text;
using System.Text.RegularExpressions;
using LiveNetwork.Domain;


namespace LiveNetwork.Infrastructure.Services
{
    public static class InvitePrompt
    {
        public const string Experiment = "linkedin_invite_content_reference_v1";
        private const string DefaultTone = "professional, respectful, thoughtful";
        private const string DefaultStyle = "concise, specific, genuine";
        private const int DefaultMaxChars = 195;
        private const string FallbackName = "there";

        /// <summary>
        /// Builds a content-referencing LinkedIn invitation prompt.
        /// </summary>
        /// <param name="profile">Parsed LinkedIn profile.</param>
        /// <param name="recentContent">
        /// Optional, recent content from the profile (post, article, etc.) to reference.
        /// </param>
        /// <param name="sharedChallenge">
        /// Optional, specific professional challenge to mention as common ground.
        /// </param>
        /// <param name="tone">Default: "professional, respectful, thoughtful".</param>
        /// <param name="style">Default: "concise, specific, genuine".</param>
        /// <param name="maxChars">Hard limit for the final invite (default: 300).</param>
        public static Prompt BuildPrompt(
            LinkedInProfile profile,
            string? recentContent = null,
            string? sharedChallenge = null,
            string tone = DefaultTone,
            string style = DefaultStyle,
            int maxChars = DefaultMaxChars)
        {
            ValidateParameters(profile, maxChars);

            var profileData = ExtractProfileData(profile, recentContent, sharedChallenge);
            var builder = new AIPromptBuilder();

            ConfigureAIPromptBuilder(builder, profileData, tone, style, maxChars);

            return builder.BuildPromptObject();
        }

        private static void ValidateParameters(LinkedInProfile profile, int maxChars)
        {
            if (profile == null)
            {
                throw new ArgumentNullException(nameof(profile));
            }

            if (profile.Url == null) throw new ArgumentException("Profile.Url is required.", nameof(profile));
            if (maxChars <= 0) throw new ArgumentException("Max characters must be positive.", nameof(maxChars));
        }

        private static ProfileData ExtractProfileData(LinkedInProfile profile, string? recentContent, string? sharedChallenge)
        {
            return new ProfileData
            {
                // CRITICAL FIX: Use only the provided FullName. Guessing a name is inauthentic and risky.
                Name = Safe(profile.FullName, FallbackName),
                IndustryArea = DeriveIndustryArea(profile),
                NotableAchievement = DeriveNotableAchievement(profile),
                Headline = Safe(profile.Headline),
                Location = Safe(profile.Location),
                CurrentCompany = Safe(profile.CurrentCompany),
                RecentContent = Safe(recentContent),
                SharedChallenge = Safe(sharedChallenge, "common technical challenges in our field"),
                ProfileUrl = profile.Url
            };
        }

        private static void ConfigureAIPromptBuilder(AIPromptBuilder builder, ProfileData profileData, string tone, string style, int maxChars)
        {
            builder.Role = "assistant";
            builder.Task = GetTaskDescription();
            builder.Context = profileData.BuildContextString(); // Delegate context building to the data object
            builder.Format = GetFormatInstructions(maxChars);
            builder.Tone = tone;
            builder.Style = style;
            builder.MaxLength = null; // enforced via constraints
            builder.IncludeSources = false;
            builder.StepByStep = false;

            AddConstraints(builder, maxChars);
            AddExamples(builder);
            AddParameters(builder, maxChars, profileData.RecentContent, profileData.SharedChallenge);
        }

        private static string GetTaskDescription()
        {
            return @"Write a thoughtful LinkedIn invite that references the recipient's content and establishes common ground around professional challenges:
- Reference specific content if available (post, article, etc.)
- Mention a genuine professional challenge that creates common ground
- Show respect for their expertise and time
- Give graceful permission to decline";
        }

        private static string GetFormatInstructions(int maxChars)
        {
            return $@"Write 2–3 concise sentences (≤ {maxChars} characters).
Start with the recipient's first name (e.g., ""Hi Paula,"").
Structure:
1) Reference their content or work (specific and genuine)
2) Connect it to your own work/experience
3) Mention a specific professional challenge as common ground
4) Express desire to connect for that reason with permission to decline
Output only the invite text (no explanations).";
        }

        private static void AddConstraints(AIPromptBuilder builder, int maxChars)
        {
            builder.AddConstraint($"Strict Hard limit: <= {maxChars} characters in total.");
            builder.AddConstraint("Start with the recipient's first name (e.g., \"Hi Paula,\").");
            builder.AddConstraint("Reference specific content if available, otherwise reference their general area of expertise.");
            builder.AddConstraint("Mention a genuine, non-presumptuous professional challenge.");
            builder.AddConstraint("Include explicit permission to decline (e.g., \"if not, I understand completely\").");
            builder.AddConstraint("Be specific but not overly technical - keep it accessible.");
            builder.AddConstraint("No flattery or excessive compliments - keep it professional.");
            builder.AddConstraint("No hard selling or pitching.");
            builder.AddConstraint("Write in English.");
            builder.AddConstraint("Return only the message text (no prefixes like 'Draft:' or code fences).");
        }

        private static void AddExamples(AIPromptBuilder builder)
        {
            // EXAMPLE 1: Good - Content Reference with Neutral Common Ground
            builder.AddExample(
                "GOOD: \"Hi Paula, your post on system architecture resonated with me. I also work in software dev and face similar challenges. Would you be open to connect? No worries if not.\"");

            // EXAMPLE 2: Good - Article Reference with a Shared Focus Area
            builder.AddExample(
                "GOOD: \"\"Hi Mark, your article on data pipeline optimization was insightful. I also work on data engineering challenges. Would you be open to connect? No problem if not.\"");

            // EXAMPLE 3: Good - Handles Missing Name Gracefully (Uses Fallback)
            builder.AddExample(
                "GOOD: \"Hi there, I saw your profile and your work in product management at TechCorp stood out. I also focus on engineering + strategy. Open to connect? No pressure if not.\"");

            // EXAMPLE 4: BAD - What to Avoid
            builder.AddExample(
                "BAD: \"Dear Sir/Madam, I would like to add you to my professional network on LinkedIn to explore potential synergies for my B2B SaaS solution.\"");

            builder.AddExample(
                "BAD: \"Hi Anna, we should connect! I see you're a developer. My company helps developers like you reduce technical debt by 50%. Let's hop on a quick call this week?\"");

            builder.AddExample(
                "BAD: \"Hi Cloud, your profile is amazing! Let's connect and change the world together. I'd love to pick your brain over coffee.\"");
        }

        private static void AddParameters(AIPromptBuilder builder, int maxChars, string? recentContent, string? sharedChallenge)
        {
            builder.AddParameter("TargetMaxChars", maxChars.ToString());
            builder.AddParameter("Approach", "Content-referencing with shared challenges");
            if (!string.IsNullOrWhiteSpace(recentContent))
                builder.AddParameter("RecentContent", recentContent);
            if (!string.IsNullOrWhiteSpace(sharedChallenge))
                builder.AddParameter("SharedChallenge", sharedChallenge);
        }

        // ---------- Heuristics for optional context synthesis ----------

        private static string DeriveIndustryArea(LinkedInProfile profile)
        {
            var contextual = profile.Experiences?
                .SelectMany(e => e.Roles ?? Enumerable.Empty<ExperienceRoleModel>())
                .Select(r => r.ContextualSkills)
                .FirstOrDefault(s => !string.IsNullOrWhiteSpace(s));

            if (!string.IsNullOrWhiteSpace(contextual))
                return TrimToPhrase(contextual, 80);

            if (!string.IsNullOrWhiteSpace(profile.Headline))
                return TrimToPhrase(ExtractPrimaryFromHeadline(profile.Headline), 80);

            if (!string.IsNullOrWhiteSpace(profile.CurrentCompany))
                return TrimToPhrase(profile.CurrentCompany, 80);

            // FIX: Changed from a presumptuous "technology" to a neutral placeholder.
            return "their field";
        }

        private static string DeriveNotableAchievement(LinkedInProfile profile)
        {
            var about = FirstSentence(profile.AboutText);
            if (!string.IsNullOrWhiteSpace(about))
                return TrimToPhrase(about, 120);

            var current = profile.Experiences?
                .FirstOrDefault(e => StringEquals(e.Company, profile.CurrentCompany))
                ?? profile.Experiences?.FirstOrDefault();

            var currentRole = current?.Roles?.FirstOrDefault()?.Title;
            if (!string.IsNullOrWhiteSpace(currentRole) && !string.IsNullOrWhiteSpace(current?.Company))
                return $"{currentRole} at {current.Company}";

            var anyDesc = current?.Roles?.FirstOrDefault()?.Description;
            if (!string.IsNullOrWhiteSpace(anyDesc))
                return TrimToPhrase(FirstSentence(anyDesc), 120);

            return "recent leadership and impact";
        }

        private static string ExtractPrimaryFromHeadline(string headline)
        {
            var separators = new[] { " | ", "|", " • ", " •", "·", " – ", "-", "," };
            var chunk = headline.Split(separators, StringSplitOptions.RemoveEmptyEntries)
                                .FirstOrDefault();
            return chunk?.Trim() ?? headline;
        }

        private static string FirstSentence(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return string.Empty;

            var match = Regex.Match(text.Trim(), @"^(.+?[\.!\?])\s");
            if (match.Success) return match.Groups[1].Value.Trim();

            return TrimToPhrase(text.Trim(), 120);
        }

        private static string TrimToPhrase(string? value, int max)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            value = Regex.Replace(value.Trim(), @"\s+", " ");
            return value.Length <= max ? value : value[..max].TrimEnd() + "…";
        }

        private static string Safe(string? value, string fallback = "")
            => string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();

        private static bool StringEquals(string? a, string? b)
            => string.Equals(a?.Trim(), b?.Trim(), StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// Private class responsible for holding and formatting the profile data for the AI context.
        /// This encapsulation ensures the context formatting logic is coupled with the data it uses.
        /// </summary>
        private class ProfileData
        {
            public string Name { get; set; } = string.Empty;
            public string IndustryArea { get; set; } = string.Empty;
            public string NotableAchievement { get; set; } = string.Empty;
            public string Headline { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public string CurrentCompany { get; set; } = string.Empty;
            public string RecentContent { get; set; } = string.Empty;
            public string SharedChallenge { get; set; } = string.Empty;
            public Uri? ProfileUrl { get; set; }

            /// <summary>
            /// Builds the context string for the AI prompt. This method encapsulates the formatting logic,
            /// aligning with the Tell-Don't-Ask principle and improving cohesion.
            /// </summary>
            public string BuildContextString()
            {
                var sb = new StringBuilder();
                sb.AppendLine("You have the following profile details of the person:");
                sb.AppendLine($"Name: {Name}");
                sb.AppendLine($"Industry/Area: {IndustryArea}");
                sb.AppendLine($"Notable achievement or experience: {NotableAchievement}");
                sb.AppendLine($"Headline: {Headline}");
                sb.AppendLine($"Location: {Location}");
                sb.AppendLine($"Current company: {CurrentCompany}");
                sb.AppendLine($"Profile URL: {ProfileUrl}");

                if (!string.IsNullOrWhiteSpace(RecentContent))
                {
                    sb.AppendLine();
                    sb.AppendLine("Recent content to reference:");
                    sb.AppendLine(RecentContent);
                }

                sb.AppendLine();
                sb.AppendLine("Shared professional challenge to mention:");
                sb.AppendLine(SharedChallenge);

                return sb.ToString();
            }
        }
    }
}