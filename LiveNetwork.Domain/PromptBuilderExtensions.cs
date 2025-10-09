using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LiveNetwork.Domain
{
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
    }

}
