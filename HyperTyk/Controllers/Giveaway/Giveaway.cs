using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HyperTyk.Controllers.Giveaway
{
    public class Giveaway
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public int winnerCount { get; set; }
        public string created_at { get; set; }
        public string expired_on { get; set; }
        public List<GiveawayRequirement> requirements { get; set; }
        public List<string> participants { get; set; }
        public List<string> winners { get; set; }

        public string WinnersString => winners != null ? string.Join(", ", winners) : "No winners yet";
    }

    
}