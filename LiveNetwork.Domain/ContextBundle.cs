using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveNetwork.Domain
{
    public record ContextBundle(
        MetaSection Meta,
        SearchCriteriaSection SearchCriteria,
        FocusSection Focus);

    public record MetaSection(
        string Geography,
        string Source,           // "Capterra"
        string Audience,         // "Small businesses & independent consultants"
        string Version);         // e.g., "v1.0"

    public record SearchCriteriaSection(
        string[] Software,       // e.g., QuickBooks, Xero, FreshBooks, Wave, Zoho Books, Toggl, Harvest
        string[] Features,       // proposals/estimates, invoicing, timesheets, spreadsheets/manual, reconciliation, e-Transfer, receipts
        string[] Segments,       // independent consultants, freelancers, micro-agencies, small firms (2–10)
        string[] Locations,      // Canada, BC, Vancouver, CRA
        string[] MustMentionAny  // términos que deben aparecer en la reseña
    );

    public record FocusSection(
        string ResearchGoal,     // “Identify pains & opportunities…”
        string[] MustDeliver,    // Top Product Matches, Pains, Canadian-Specific, Synthesis
        string[] Exclusions      // opcional: “exclude ERP/enterprise-only”, etc.
    );

}
