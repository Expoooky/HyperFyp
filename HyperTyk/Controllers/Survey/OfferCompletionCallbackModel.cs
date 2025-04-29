using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HyperTyk.Controllers.Survey
{
    public class OfferCompletionCallbackModel
    {
        public string UserId { get; set; }
        public string OfferId { get; set; }
        public int Reward { get; set; }
        public string Signature { get; set; }
    }
}