using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveNetwork.Domain
{
    public sealed class Reviewer
    {
        public string? Name { get; set; }
        public string? Role { get; set; }
        public string? Industry { get; set; }
        public string? UsedFor { get; set; } // "Used the software for: 2+ years"
        public string? AvatarUrl { get; set; }
    }
}
