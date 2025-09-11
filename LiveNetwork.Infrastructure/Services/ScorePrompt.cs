using System.Text;
using LiveNetwork.Domain;


namespace LiveNetwork.Infrastructure.Services
{
    /// <summary>
    /// Builds a Sandler-style *review* prompt to analyze a user's LinkedIn invitation.
    /// Output format is strictly specified to ensure consistent, actionable feedback.
    /// </summary>
    public static class ScorePrompt
    {
        public const string Experiment = "linkedin_invite_sandler_review_v1";

        /// <summary>
        /// Creates a prompt instructing the model to critique a LinkedIn invitation using Sandler principles,
        /// and to return a structured analysis with a rewritten version and alternatives.
        /// </summary>
        /// <param name="invitationText">The user's invitation text to be analyzed.</param>
        /// <param name="targetProfile">
        /// Optional: target LinkedIn profile (if you want the model to consider light context like name, role, company).
        /// Only lightweight metadata is passed; do NOT rely on it being present.
        /// </param>
        public static Prompt BuildSandlerReviewPrompt(string invitationText, LinkedInProfile? targetProfile = null)
        {
            if (string.IsNullOrWhiteSpace(invitationText))
                throw new ArgumentException("Invitation text must not be empty.", nameof(invitationText));

            // Build the full context (lightweight – safe, optional fields only).
            var ctx = new StringBuilder();
            ctx.AppendLine("Operating principles (apply to all replies):");
            ctx.AppendLine("From now on, don’t just agree with my ideas or take my conclusions for granted.");
            ctx.AppendLine("When I propose an idea, do the following:");
            ctx.AppendLine("• Question my assumptions.");
            ctx.AppendLine("• Take a skeptical stance.");
            ctx.AppendLine("• Verify my reasoning.");
            ctx.AppendLine("• Propose other approaches.");
            ctx.AppendLine("• Prioritize accuracy over approval.");
            ctx.AppendLine("• Be constructive yet rigorous.");
            ctx.AppendLine("If you notice biases or unfounded beliefs, say it clearly. The goal is to refine our conclusions and our process.");
            ctx.AppendLine();

            if (targetProfile != null)
            {
                ctx.AppendLine("Target Profile (optional, for light context only):");
                if (!string.IsNullOrWhiteSpace(targetProfile.FullName)) ctx.AppendLine($"• Name: {targetProfile.FullName}");
                if (!string.IsNullOrWhiteSpace(targetProfile.Headline)) ctx.AppendLine($"• Headline: {targetProfile.Headline}");
                if (!string.IsNullOrWhiteSpace(targetProfile.CurrentCompany)) ctx.AppendLine($"• Company: {targetProfile.CurrentCompany}");
                if (!string.IsNullOrWhiteSpace(targetProfile.Location)) ctx.AppendLine($"• Location: {targetProfile.Location}");
                if (targetProfile.Url != null) ctx.AppendLine($"• URL: {targetProfile.Url}");
                ctx.AppendLine();
            }

            // Build the task text.
            var task = new StringBuilder();
            task.AppendLine("Role:");
            task.AppendLine("You are a seasoned sales coach and an expert in the Sandler Sales Method.");
            task.AppendLine("Your specialty is using this methodology to build genuine, professional relationships on LinkedIn,");
            task.AppendLine("moving away from pushy sales tactics and toward creating trust and value.");
            task.AppendLine();
            task.AppendLine("Context:");
            task.AppendLine("The user needs to evaluate a LinkedIn connection invitation. The goal is not just to get a connection");
            task.AppendLine("but to start a meaningful professional relationship that could lead to a conversation.");
            task.AppendLine("The Sandler method emphasizes being upfront, mutually respectful, and avoiding \"free consulting\"");
            task.AppendLine("or overly eager pitches. A good invitation should feel like a peer-to-peer conversation, not a cold call.");
            task.AppendLine();
            task.AppendLine("Task:");
            task.AppendLine("Analyze the provided LinkedIn invitation based on the following Sandler-inspired criteria:");
            task.AppendLine("Naturalness & Tone: Does it sound like a genuine human message, or like a canned sales pitch?");
            task.AppendLine("Is the tone peer-to-peer and respectful?");
            task.AppendLine("Effectiveness & Sandler Alignment:");
            task.AppendLine("- Up-Front Contract: Does it set a clear, low-pressure expectation for the connection (e.g., \"to share ideas,\" \"to expand my network\")?");
            task.AppendLine("- Pain: Does it hint at a possible shared challenge or area of interest without being presumptuous or negative?");
            task.AppendLine("- No Pitching: Does it avoid offering unsolicited advice, solutions, or a sales pitch immediately?");
            task.AppendLine("- Permission-Based: Does it feel like it asks for permission to connect in a respectful way?");
            task.AppendLine("Improvement Points: Provide a specific rewrite of the invitation.");
            task.AppendLine("Explain why your changes make it more effective according to the Sandler principles.");
            task.AppendLine("Offer 2–3 alternative phrasings for key sentences.");
            task.AppendLine();

            // Build the required output format.
            var format = new StringBuilder();
            format.AppendLine("Format:");
            format.AppendLine("Provide your analysis in the following structured format (plain text, in English):");
            format.AppendLine("Overall Score: (X/10)");
            format.AppendLine("Naturalness Analysis: [2–3 sentences]");
            format.AppendLine("Effectiveness & Sandler Analysis:");
            format.AppendLine("- Up-Front Contract: [...]");
            format.AppendLine("- Pain: [...]");
            format.AppendLine("- No Pitching: [...]");
            format.AppendLine("- Permission-Based: [...]");
            format.AppendLine("Improved Invitation:");
            format.AppendLine("Rewritten Message: [Full rewritten text]");
            format.AppendLine("Rationale for Changes: [Explain the Sandler reasoning behind key changes]");
            format.AppendLine("Alternative Phrases:");
            format.AppendLine("- [Short standalone phrase 1]");
            format.AppendLine("- [Short standalone phrase 2]");
            format.AppendLine("- [Short standalone phrase 3]");
            format.AppendLine();
            format.AppendLine("Example:");
            format.AppendLine("User's Invitation: \"Hi, I see you work in SaaS. My company helps businesses like yours scale revenue by 30%. Let's connect so I can tell you more!\"");
            format.AppendLine("Your Analysis (Example):");
            format.AppendLine("Overall Score: 3/10");
            format.AppendLine("Naturalness Analysis: Sounds like a generic sales pitch. The tone is vendor-to-prospect, not peer-to-peer.");
            format.AppendLine("Effectiveness & Sandler Analysis:");
            format.AppendLine("- Up-Front Contract: Unclear; the connection request implies a sales conversation, which adds pressure.");
            format.AppendLine("- Pain: Tries to inject a solution (\"scale revenue by 30%\") without context; feels presumptuous.");
            format.AppendLine("- No Pitching: Violated; it pitches a solution immediately.");
            format.AppendLine("- Permission-Based: Weak; it pushes for a follow-up rather than offering a low-pressure connection.");
            format.AppendLine("Improved Invitation:");
            format.AppendLine("Rewritten Message: \"Hi [Name], I’m exploring how SaaS teams approach sustainable growth and found your background relevant. I prefer low-pressure, peer connections to share perspectives. If that’s useful, I’d be glad to connect—no problem if not.\"");
            format.AppendLine("Rationale for Changes: Sets a clear, low-pressure purpose (up-front contract), removes pitching, and requests permission.");
            format.AppendLine("Alternative Phrases:");
            format.AppendLine("- \"Open to a peer connection to exchange perspectives?\"");
            format.AppendLine("- \"Happy to connect if it’s useful—no pressure either way.\"");
            format.AppendLine("- \"Exploring how peers approach this—would a light connection help?\"");



            // Assemble with your existing AIPromptBuilder abstraction.
            var builder = new AIPromptBuilder
            {
                Role = "assistant",
                Task = task.ToString(),
                Context = ctx.ToString(),
                Format = format.ToString(),
                Tone = "analytical, respectful, constructive",
                Style = "clear, specific, actionable",
                MaxLength = null,
                IncludeSources = false,
                StepByStep = false
            };

            // Constraints help keep responses consistent.
            builder.AddConstraint("Write in English.");
            builder.AddConstraint("Follow the exact output headings and order specified in the Format section.");
            builder.AddConstraint("Do not add extra sections or code fences.");
            builder.AddConstraint("Keep the rewritten message concise (<= 350 characters if possible).");
            builder.AddConstraint("Avoid any sales pitch or implied consulting in the rewritten message.");
            builder.AddConstraint("Use a peer-to-peer, permission-based tone.");

            // Pass the invitation as a parameter (if your builder supports parameter substitution) and as user content.
            builder.AddParameter("InvitationText", invitationText.Trim());

            // Attach the user's invitation as the primary content to analyze.
            return builder.BuildPromptObject();
        }
    }
}

