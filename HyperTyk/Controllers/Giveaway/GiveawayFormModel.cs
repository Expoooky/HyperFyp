using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HyperTyk.Controllers.Giveaway
{
    public class GiveawayFormModel
    {
        public string title { get; set; }
        public string desc { get; set; }
        public int winnerCount { get; set; }
        public DateTime expiration_date { get; set; }
        public bool isVerified { get; set; }
        public bool referralCount { get; set; }
        public int minReferralCount { get; set; }
        public bool offerCount { get; set; }
        public int minOfferCount { get; set; }
    }
}