using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HyperTyk.Controllers.Survey
{
    public class Offer
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string OfferUrl { get; set; }
        public int Reward { get; set; }
    }

    public class OfferResponse
    {
        public List<Offer> Offers { get; set; }
    }
}