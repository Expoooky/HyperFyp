using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HyperTyk.Models
{
    public class ClientData
    {
        public string id { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public byte[] avatar { get; set; }
        public string referralcode { get; set; }
        public Nullable<double> coins { get; set; }
        public Nullable<double> total_coin_earned { get; set; }
        public Nullable<double> total_coin_spent { get; set; }
        public Nullable<int> total_users_referred { get; set; }
        public Nullable<int> total_offers_completed { get; set; }
        public Nullable<int> isverified { get; set; }
        public Nullable<System.DateTime> last_login_coins { get; set; }
    }
}