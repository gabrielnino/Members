using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveNetwork.Domain
{

    public sealed class Ratings
    {
        public double? EaseOfUse { get; set; }
        public double? CustomerService { get; set; }
        public double? Features { get; set; }
        public double? ValueForMoney { get; set; }
    }
}
