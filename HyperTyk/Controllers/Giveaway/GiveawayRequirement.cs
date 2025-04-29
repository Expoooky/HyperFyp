using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HyperTyk.Controllers.Giveaway
{
    public class GiveawayRequirement
    {
        public int? isVerified { get; set; } // 1 if required, 0 if not, null if not applicable
        public int? refCount { get; set; }  // Minimum referral count, null if not applicable
        public int? offCount { get; set; } // Minimum offer count, null if not applicable
    }
}