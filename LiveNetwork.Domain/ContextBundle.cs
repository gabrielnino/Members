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
