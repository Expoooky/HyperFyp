using HyperTyk.Controllers.Auth;
using HyperTyk.Controllers.Giveaway;
using HyperTyk.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HyperTyk.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        readonly Entities _database = new Entities();

        private async Task<account> FetchUserDataFromDatabase()
        {
            var userIsd = Session["UserId"] as string;
            // Fetch user data from the database based on userId
            var userData = await _database.accounts
                .Where(user => user.id == userIsd)
                .FirstOrDefaultAsync();

            return userData;
        }

        private async Task<ClientData> FetchClientDataFromDatabase(string userId)
        {
            // Fetch user data from the database based on userId
            var userData = await _database.accounts
                .Where(user => user.id == userId)
                .FirstOrDefaultAsync();

            var clientData = new ClientData
            {
                id = userData.id,
                username = userData.username,
                email = userData.email,
                avatar = userData.avatar,
                referralcode = userData.referralcode,
                coins = userData.coins,
                total_coin_earned = userData.total_coin_earned,
                total_coin_spent = userData.total_coin_spent,
                total_offers_completed = userData.total_offers_completed,
                total_users_referred = userData.total_users_referred,
                isverified = userData.isverified,
            };

            return clientData;
        }

        private async Task<List<auth>> FetchStaffDataFromDatabase()
        {
            var staffData = await _database.auths
                .Where(user => user.usertype == "Admin")
                .ToListAsync();

            return staffData;
        }

        private async Task<List<ClientAccountInformation>> GetExtendedUserDataAsync()
        {
            var userData = await _database.accounts
                .Where(user => _database.auths.Any(auth => auth.id == user.id && auth.usertype == "Normal"))
                .Select(user => new
                {
                    user.id,
                    user.username,
                    user.email,
                    user.avatar,
                    user.referralcode,
                    user.coins,
                    user.total_coin_earned,
                    user.total_coin_spent,
                    user.total_users_referred,
                    user.total_offers_completed,
                    user.isverified
                })
                .ToListAsync();

            var extendedData = userData.Select(user =>
            {
                var authInfo = _database.auths.FirstOrDefault(auth => auth.id == user.id);
                return new ClientAccountInformation
                {
                    Id = user.id,
                    Username = user.username,
                    Email = user.email,
                    Avatar = user.avatar,
                    Referralcode = user.referralcode,
                    Coins = user.coins,
                    Total_coin_earned = user.total_coin_earned,
                    Total_coin_spent = user.total_coin_spent,
                    Total_users_referred = user.total_users_referred,
                    Total_offers_completed = user.total_offers_completed,
                    Isverified = user.isverified,
                    Usertype = authInfo?.usertype, // Handle null case for authInfo
                    IsBanned = authInfo?.isbanned ?? 0, // Handle null case with default value (0)
                    DateCreated = authInfo?.datecreated ?? DateTime.MinValue // Handle null case with default value (MinValue)
                };
            }).ToList();

            return extendedData;
        }

        private async Task<List<promo_codes>> GetPromoCodesListAsync()
        {
            var promoCodes = await _database.promo_codes.ToListAsync();

            return promoCodes;
        }

        private async Task<List<social_service>> GetSocialServicesListAsync()
        {
            var socservicesModel = await _database.social_service.ToListAsync();

            return socservicesModel;
        }

        private async Task<List<social_orders>> GetSocialSOrdersListAsync(string userId)
        {
            var socorderModel = await _database.social_orders.Where(u => u.id == userId).ToListAsync();

            return socorderModel;
        }

        private bool IsValidPromoCode(string promocode)
        {
            string pattern = @"^[a-zA-Z0-9]+$";
            return Regex.IsMatch(promocode, pattern);
        }

        public class ClientAccountInformation
        {
            public string Id { get; set; }
            public string Username { get; set; }
            public string Email { get; set; }
            public byte[] Avatar { get; set; }
            public string Referralcode { get; set; }
            public Nullable<double> Coins { get; set; }
            public Nullable<double> Total_coin_earned { get; set; }
            public Nullable<double> Total_coin_spent { get; set; }
            public Nullable<int> Total_users_referred { get; set; }
            public Nullable<int> Total_offers_completed { get; set; }
            public Nullable<int> Isverified { get; set; }
            public string Usertype { get; set; }
            public int IsBanned { get; set; }
            public DateTime DateCreated { get; set; }
        }

        public async Task<ActionResult> Index()
        {
            var accountModels = await FetchUserDataFromDatabase();
            ViewBag.Data = accountModels;

            var extendedData = await GetExtendedUserDataAsync();

            return View(extendedData);
        }

        public async Task<ActionResult> Staff()
        {
            var accountModels = await FetchUserDataFromDatabase();
            ViewBag.Data = accountModels;

            var extendedData = await FetchStaffDataFromDatabase();

            return View(extendedData);
        }

        public async Task<ActionResult> Settings()
        {
            var accountModels = await FetchUserDataFromDatabase();
            ViewBag.Data = accountModels;

            return View();
        }

        public async Task<ActionResult> Promo_Codes()
        {
            var accountModels = await FetchUserDataFromDatabase();
            ViewBag.Data = accountModels;


            var PromoCodeModels = await GetPromoCodesListAsync();

            return View(PromoCodeModels);
        }

        public async Task<ActionResult> Social_Services()
        {
            var accountModels = await FetchUserDataFromDatabase();
            ViewBag.Data = accountModels;


            var SocialServicesModels = await GetSocialServicesListAsync();

            return View(SocialServicesModels);
        }
        public async Task<ActionResult> Giveaway()
        {
            var accountModels = await FetchUserDataFromDatabase();
            ViewBag.Data = accountModels;

            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Controllers", "Giveaway", "GiveawayDatabase.json");
            string json = System.IO.File.ReadAllText(jsonFilePath);

            var giveawayData = JsonConvert.DeserializeObject<GiveawayDatabase>(json)?.Giveaways;

            return View(giveawayData);
        }

        public async Task<ActionResult> Search(string userId)
        {
            var accountModels = await FetchUserDataFromDatabase();
            ViewBag.Data = accountModels;

            var userinfo = await FetchClientDataFromDatabase(userId);
            ViewBag.UserInfo = userinfo;

            var userorders = await GetSocialSOrdersListAsync(userId);

            return View(userorders);
        }

        [HttpPost]
        public async Task<ActionResult> Add_PromoCode(string promocode, int value, int user_limit, int usage_limit, DateTime expiration_date)
        {
            if (User.Identity.IsAuthenticated)
            {
                if (!IsValidPromoCode(promocode))
                {
                    TempData["ErrorMessage"] = "Special characters or spaces are not allowed in the promo code.";
                    return RedirectToAction("Promo_Codes");
                }

                var checkexistence = await _database.promo_codes.FirstOrDefaultAsync(u => u.promocode == promocode);
                if (checkexistence != null)
                {
                    TempData["ErrorMessage"] = "Promo code already exists.";
                    return RedirectToAction("Promo_Codes");
                }

                var promo = new promo_codes
                {
                    promocode = promocode,
                    discount_value = value,
                    discount_type = "fixed",
                    usage_limit = usage_limit,
                    user_limit = user_limit,
                    datecreated = DateTime.UtcNow,
                    expiration_date = expiration_date,
                    is_active = 1,
                    promocode_author = User.Identity.Name
                };

                _database.promo_codes.Add(promo);
                await _database.SaveChangesAsync();

                TempData["SuccessMessage"] = "Promo code added successfully.";
                return RedirectToAction("Promo_Codes");
            }

            TempData["ErrorMessage"] = "Unauthorized.";
            return RedirectToAction("Promo_Codes");
        }

        [HttpPost]
        public async Task<ActionResult> AddService(int service_id, string item, string item_desc, int rate, int interval, int fast_lane_interval, int fast_lane_interval_rate)
        {
            if (User.Identity.IsAuthenticated)
            {
                var checkexistence = await _database.social_service.FirstOrDefaultAsync(u => u.service_id == service_id);
                if (checkexistence != null)
                {
                    TempData["ErrorMessage"] = "Service already exists.";
                    return RedirectToAction("Social_Services");
                }

                var social = new social_service
                {
                   service_id = service_id,
                   item = item,
                   item_desc = item_desc,
                   rate = rate, 
                   interval = interval,
                   fast_lane_interval = fast_lane_interval,
                   fast_lane_interval_rate = fast_lane_interval_rate
                };

                _database.social_service.Add(social);
                await _database.SaveChangesAsync();

                TempData["SuccessMessage"] = "New service has been added successfully.";
                return RedirectToAction("Social_Services");
            }

            TempData["ErrorMessage"] = "Unauthorized.";
            return RedirectToAction("Social_Services");
        }

        [HttpPost]
        public async Task<ActionResult> Change_Password(string currentPassword, string newPassword)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = Session["UserId"] as string;
                var user = await _database.auths.FirstOrDefaultAsync(u => u.id == userId);

                if (user != null)
                {
                    if (AuthPasswordManager.VerifyPassword(currentPassword, user.password))
                    {
                        user.password = AuthPasswordManager.HashPassword(newPassword);

                        await _database.SaveChangesAsync();

                        TempData["SuccessMessage"] = "You have successfully set a new password.";
                        return RedirectToAction("Settings");
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid Current Password.";
                        return RedirectToAction("Settings");
                    }
                }

                // No user found, redirect to Settings
                TempData["ErrorMessage"] = "User not found.";
                return RedirectToAction("Settings");
            }

            TempData["ErrorMessage"] = "Unauthorized.";
            return RedirectToAction("Settings");
        }

        [HttpPost]
        public async Task<ActionResult> AddCoin(string id, int amount)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _database.accounts.FirstOrDefaultAsync(u => u.id == id);

                if (user != null)
                {
                    user.coins += amount;

                    await _database.SaveChangesAsync();

                    return Json(new { success = true, message = $"Successfully added {amount}. New Balance: {user.coins}" });
                }

                return Json(new { success = false, message = "User not found." });
            }

            return Json(new { success = false, message = "Unauthorized." });
        }

        [HttpPost]
        public async Task<ActionResult> DeductCoin(string id, int amount)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _database.accounts.FirstOrDefaultAsync(u => u.id == id);

                if (user != null)
                {
                    if (user.coins < amount)
                    {
                        return Json(new { success = true, message = $"User has insufficient balance, therefore, deduction cancelled." });
                    }

                    user.coins -= amount;

                    await _database.SaveChangesAsync();

                    return Json(new { success = true, message = $"Successfully deducted {amount}. New Balance: {user.coins}" });
                }

                return Json(new { success = false, message = "User not found." });
            }

            return Json(new { success = false, message = "Unauthorized." });
        }

        [HttpPost]
        public async Task<ActionResult> Ban(string id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _database.auths.FirstOrDefaultAsync(u => u.id == id);

                if (user != null)
                {
                    user.isbanned = 1;

                    await _database.SaveChangesAsync();

                    return Json(new { success = true, message = "User has been banned." });
                }

                return Json(new { success = false, message = "User not found." });
            }

            return Json(new { success = false, message = "Unauthorized." });
        }

        [HttpPost]
        public async Task<ActionResult> Unban(string id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _database.auths.FirstOrDefaultAsync(u => u.id == id);

                if (user != null)
                {
                    user.isbanned = 0;

                    await _database.SaveChangesAsync();

                    return Json(new { success = true, message = "User has been unbanned." });
                }

                return Json(new { success = false, message = "User not found." });
            }

            return Json(new { success = false, message = "Unauthorized." });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteCode(string code)
        {
            if (User.Identity.IsAuthenticated)
            {
                var promocode = await _database.promo_codes.FirstOrDefaultAsync(u => u.promocode == code);

                if (promocode != null)
                {
                    _database.promo_codes.Remove(promocode);
                    _database.SaveChanges();

                    return Json(new { success = true, message = "Promo Code has been removed." });
                }

                return Json(new { success = false, message = "Promo Code does not exist." });
            }

            return Json(new { success = false, message = "Unauthorized." });
        }

        [HttpPost]
        public async Task<ActionResult> DeleteService(int service)
        {
            if (User.Identity.IsAuthenticated)
            {
                var social = await _database.social_service.FirstOrDefaultAsync(u => u.service_id == service);

                if (social != null)
                {
                    _database.social_service.Remove(social);
                    _database.SaveChanges();

                    return Json(new { success = true, message = "Service has been removed." });
                }

                return Json(new { success = false, message = "Service does not exist." });
            }

            return Json(new { success = false, message = "Unauthorized." });
        }

        public string GenerateUniqueId()
        {
            // Using a combination of timestamp and a random number for uniqueness
            string uniqueId = DateTime.Now.Ticks.ToString() + new Random().Next(1000, 9999).ToString();
            return uniqueId;
        }

        [HttpPost]
        public ActionResult Create_Giveaway(GiveawayFormModel model)
        {
            if (User.Identity.IsAuthenticated)
            {
                try
                {
                    string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Controllers", "Giveaway", "GiveawayDatabase.json");
                    if (!System.IO.File.Exists(jsonFilePath))
                    {
                        TempData["ErrorMessage"] = "Giveaway database file not found.";
                        return RedirectToAction("Giveaway");
                    }

                    string json = System.IO.File.ReadAllText(jsonFilePath);
                    var giveawayData = JsonConvert.DeserializeObject<GiveawayDatabase>(json);

                        var newGiveaway = new Giveaway.Giveaway
                        {
                            id = GenerateUniqueId(),
                            title = model.title,
                            description = model.desc,
                            winnerCount = model.winnerCount,
                            created_at = DateTime.UtcNow.ToString("MMMM dd, yyyy hh:mm tt"),
                            expired_on = model.expiration_date.ToString("MMMM dd, yyyy hh:mm tt"),
                            requirements = new List<GiveawayRequirement>
                        {
                            new GiveawayRequirement
                            {
                                isVerified = model.isVerified ? 1 : (int?)null,
                                refCount = model.referralCount ? model.minReferralCount : (int?)null,
                                offCount = model.offerCount ? model.minOfferCount : (int?)null
                            }
                        },
                            participants = new List<string>(),
                            winners = new List<string>()
                        };

                    giveawayData.Giveaways.Add(newGiveaway);
                    string updatedJson = JsonConvert.SerializeObject(giveawayData, Formatting.Indented);
                    System.IO.File.WriteAllText(jsonFilePath, updatedJson);

                    TempData["SuccessMessage"] = "Giveaway created successfully.";
                    return RedirectToAction("Giveaway");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "An error occurred: " + ex.Message;
                    return RedirectToAction("Giveaway");
                }
            }
            TempData["ErrorMessage"] = "Unauthorized.";
            return RedirectToAction("Giveaway");
        }

        [HttpPost]
        public ActionResult DeleteGiveaway(string id)
        {
            try
            {
                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Controllers", "Giveaway", "GiveawayDatabase.json");
                string json = System.IO.File.ReadAllText(jsonFilePath);
                var giveawayData = JsonConvert.DeserializeObject<GiveawayDatabase>(json);

                var giveawayToRemove = giveawayData.Giveaways.FirstOrDefault(g => g.id == id);
                if (giveawayToRemove != null)
                {
                    giveawayData.Giveaways.Remove(giveawayToRemove);

                    string updatedJson = JsonConvert.SerializeObject(giveawayData, Formatting.Indented);
                    System.IO.File.WriteAllText(jsonFilePath, updatedJson);

                    return Json(new { success = true, message = "Giveaway removed successfully." });
                }
                else
                {
                    return Json(new { success = false, message = "Giveaway not found." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }
    }
}