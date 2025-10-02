using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveNetwork.Domain
{
    public sealed class Review
    {
        public Reviewer Reviewer { get; set; } = new();
        public string? Title { get; set; }
        public DateTime? ReviewDate { get; set; }
        public double? OverallRating { get; set; }
        public Ratings Ratings { get; set; } = new();
        public int? LikelihoodToRecommend10 { get; set; }  // 0-10
        public string? Pros { get; set; }
        public string? Cons { get; set; }
        public string? FreeText { get; set; }  // párrafo libre (si lo hay)
        public List<string> AlternativesConsidered { get; set; } = new();
        public string? ReasonForChoosing { get; set; }
        public List<string> SwitchedFrom { get; set; } = new();
        public string? SwitchReason { get; set; } // texto tras "Switched from" si aparece
    }
}
