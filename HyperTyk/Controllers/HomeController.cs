using HyperTyk.Controllers.Auth;
using HyperTyk.Controllers.Giveaway;
using HyperTyk.Controllers.Survey;
using HyperTyk.Models;
using Microsoft.AspNet.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Mvc;
using System.Web.Security;
using static System.Net.WebRequestMethods;

namespace HyperTyk.Controllers
{
    [Authorize(Roles = "Normal")]
    public class HomeController : Controller
    {
        readonly Entities _database = new Entities();


        //private async Task<List<AccountDisplayable>> GetUserDataAsync()
        //{
        //    var userId = Session["UserId"] as string;
        //    var cachedData = HttpContext.Cache.Get($"UserData_{userId}") as List<AccountDisplayable>;

        //    if (cachedData == null)
        //    {
        //        cachedData = await Task.Run(() => FetchUserDataFromDatabase(userId));
        //        HttpContext.Cache.Insert($"UserData_{userId}", cachedData, null, DateTime.UtcNow.AddMinutes(10), TimeSpan.Zero);
        //    }

        //    return cachedData;
        //}

        // Method to fetch user data from the database
        private async Task<List<account>> FetchUserDataFromDatabase(string userId)
        {
            // Fetch user data from the database and project into an anonymous type
            var userData = await _database.accounts
            .Where(user => user.id == userId)
            .Select(user => new
            {
                user.coins,
                user.total_coin_earned,
                user.total_coin_spent,
                user.total_users_referred,
                user.total_offers_completed
            })
            .ToListAsync();

            // Transform the anonymous type into a list of 'account' objects
            var accountModel = userData.Select(user => new account
            {
                coins = user.coins,
                total_coin_earned = user.total_coin_earned,
                total_coin_spent = user.total_coin_spent,
                total_users_referred = user.total_users_referred,
                total_offers_completed = user.total_offers_completed
            }).ToList();

            return accountModel;
        }

        private async Task<List<social_service>> GetSocialServicesListAsync()
        {
            var socservicesModel = await _database.social_service.ToListAsync();

            return socservicesModel;
        }

        private async Task<List<social_orders>> GetSocialSOrdersListAsync(string userId)
        {
            var socorderModel = await _database.social_orders
                    .Where(user => user.id == userId)
                    .ToListAsync();

            return socorderModel;
        }
        // GET: Home
        public async Task<ActionResult> Index()
        {
            var userId = Session["UserId"] as string;
            var today = DateTime.UtcNow.Date;

            // Check if user has already received coins for today
            var userAuth = await _database.auths.FirstOrDefaultAsync(a => a.id == userId);
            if (userAuth != null)
            {
                if (userAuth.datecreated.Date != today.Date)
                {   
                    // Proceed with coins reward check only if the user's account was not created today
                    var hasReceivedCoinsToday = _database.accounts
                        .Any(a => a.id == userId && a.last_login_coins != null
                               && a.last_login_coins.Value.Year == today.Year
                               && a.last_login_coins.Value.Month == today.Month
                               && a.last_login_coins.Value.Day == today.Day);

                    if (!hasReceivedCoinsToday)
                    {
                        var user = await _database.accounts.FirstOrDefaultAsync(u => u.id == userId);
                        if (user != null)
                        {
                            user.coins += 10;
                            user.total_coin_earned += 10;
                            user.last_login_coins = today;
                            
                            await _database.SaveChangesAsync();

                            TempData["SuccessMessage"] = "You have been rewarded 10 coin(s) as your Daily Reward.";
                        }
                    }
                }
            }
            else
            {
                // Handle the case where user authentication data is not found
                TempData["ErrorMessage"] = "User authentication data not found. Please log in again!";
                Session.Clear();
                FormsAuthentication.SignOut();
                return View();
            }

            string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Controllers", "Giveaway", "GiveawayDatabase.json");
            string json = System.IO.File.ReadAllText(jsonFilePath);

            var giveawayData = JsonConvert.DeserializeObject<GiveawayDatabase>(json)?.Giveaways;

            var accountModels = await FetchUserDataFromDatabase(userId);
            var accountModel = accountModels.FirstOrDefault();

            ViewBag.Data = accountModel;

            return View(giveawayData);
        }

        public async Task<ActionResult> Settings()
        {
            var userId = Session["UserId"] as string;
            var accountModels = await FetchUserDataFromDatabase(userId);
            var accountModel = accountModels.FirstOrDefault();
            ViewBag.Data = accountModel;

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> Change_Password(string currentPassword, string newPassword)
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
        //[HttpPost]
        //public async Task<IdentityResult> Verify_Email()
        //{
        //    var userid = Session["UserId"] as string;
        //    var user = _database.auths.FirstOrDefault(u => u.id == userid);


        //    if (user != null)
        //    {

        //        AuthMailer emailService = new AuthMailer();
        //        emailService.SendEmailConfirmation(user.email, user.username, link);

        //        TempData["SuccessMessage"] = "An email has been sent to your email address.";
        //        return RedirectToAction("Settings");
        //    }
        //    return RedirectToAction("Settings");
        //}     
        public async Task<ActionResult> Redeem()
        {
            var userId = Session["UserId"] as string;
            var accountModels = await FetchUserDataFromDatabase(userId);
            var accountModel = accountModels.FirstOrDefault();
            ViewBag.Data = accountModel;

            return View();
        }
        [HttpPost]
        public async Task<ActionResult> RedeemCode(string promocode)
        {
            if (User.Identity.IsAuthenticated)
            {
                var userId = Session["UserId"] as string;

                var checkexistence = await _database.promo_codes.FirstOrDefaultAsync(u => u.promocode == promocode);
                if (checkexistence != null)
                {
                    // Check if the promo code has reached its overall usage limit
                    var usageRedemption = await _database.promo_redemption_logs.CountAsync(r => r.promocode == promocode);
                    if (usageRedemption < checkexistence.usage_limit)
                    {
                        if (checkexistence.expiration_date.Date >= DateTime.Today)
                        {
                            // Check if the user has reached the usage limit for the promo code
                            var userRedemptions = await _database.promo_redemption_logs.CountAsync(r => r.id == userId && r.promocode == promocode);
                            if (userRedemptions < checkexistence.user_limit)
                            {
                                // Add redemption log
                                AuthOTPManager otpManager = new AuthOTPManager();
                                long currenttime = otpManager.GetCurrentEpochTime();

                                var proremlog = new promo_redemption_logs
                                {
                                    id = userId,
                                    promocode = promocode,
                                    redemption_date_epoch = currenttime
                                };

                                _database.promo_redemption_logs.Add(proremlog);

                                // Update user's coins
                                var user = await _database.accounts.FirstOrDefaultAsync(u => u.id == userId);
                                user.coins += checkexistence.discount_value;
                                user.total_coin_earned += checkexistence.discount_value;

                                await _database.SaveChangesAsync();

                                TempData["SuccessMessage"] = "Successfully redeemed " + checkexistence.discount_value + " coins!";
                                return RedirectToAction("Redeem");
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "You have reached the usage limit for this promo code.";
                                return RedirectToAction("Redeem");
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Promo code has expired.";
                            return RedirectToAction("Redeem");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Promo code has reached its overall usage limit.";
                        return RedirectToAction("Redeem");
                    }
                }
                else
                {
                    TempData["ErrorMessage"] = "Promo code does not exist.";
                    return RedirectToAction("Redeem");
                }
            }

            TempData["ErrorMessage"] = "Unauthorized.";
            return RedirectToAction("Redeem");
        }
        public async Task<ActionResult> Socials()
        {
            var userId = Session["UserId"] as string;
            var accountModels = await FetchUserDataFromDatabase(userId);
            var accountModel = accountModels.FirstOrDefault();
            ViewBag.Data = accountModel;

            var socialServicesList = await GetSocialServicesListAsync();
            ViewBag.SocialServicesList = socialServicesList;

            var socialOrderList = await GetSocialSOrdersListAsync(userId);

            return View(socialOrderList);
        }

        private readonly string apiKey = "9876f206e48fce338cdbd4c2b0720a6d";

        public async Task<ActionResult> CreateOrder(int service_id, string link, int amount, float total, bool fastlane = false)
        {
            
            if (User.Identity.IsAuthenticated)
            {
                string userId = Session["UserId"] as string;
                var user = await _database.accounts.FirstOrDefaultAsync(u => u.id == userId);
                var userorder = await _database.social_orders.FirstOrDefaultAsync(u => u.id == userId);
                var order = await _database.social_service.FirstOrDefaultAsync(u => u.service_id == service_id);

                if (user != null)
                {
                    if (user.coins < total)
                    {
                        TempData["ErrorMessage"] = "You don't have enough coins to make an order.";
                        return RedirectToAction("Socials");
                    }

                    string apiUrl = "https://addsmm.com/api/v2";
                    string parameters = $"?action=add&service={service_id}&link={link}&quantity={amount}&key={apiKey}";
                    string url = apiUrl + parameters;
                    int interval;

                    if (fastlane)
                    {
                        interval = 0;
                    }
                    else
                    {
                        int normalInterval = (int)await _database.social_service
                                                .Where(u => u.service_id == service_id)
                                                .Select(u => u.interval)
                                                .FirstOrDefaultAsync();

                        interval = normalInterval * 1000;
                    }

                    using (var httpClient = new HttpClient())
                    {
                        try
                        {
                            HttpResponseMessage response = await httpClient.PostAsync(url, null);

                            if (response.IsSuccessStatusCode)
                            {
                                string content = await response.Content.ReadAsStringAsync();

                                if (content.Contains("\"order\":"))
                                {
                                    string orderString = JsonConvert.DeserializeObject<dynamic>(content).order.ToString();
                                    long orderId = long.Parse(orderString);

                                    var socialorder = new social_orders
                                    {
                                        order_id = orderId,
                                        service_id = service_id,
                                        id = userId,
                                        item = order.item,
                                        amount = amount,
                                        amount_in_coins = total,
                                        isFastLane = fastlane ? 1 : 0
                                    };

                                    user.coins -= total;
                                    user.total_coin_spent += total;

                                    _database.social_orders.Add(socialorder);

                                    await _database.SaveChangesAsync();

                                    TempData["SuccessMessage"] = $"Success! Your order has been queued!";
                                    return RedirectToAction("Socials");
                                }
                                else if (content.Contains("\"error\":"))
                                {
                                    string errorMessage = JsonConvert.DeserializeObject<dynamic>(content).error.ToString();

                                    TempData["ErrorMessage"] = errorMessage;
                                    return RedirectToAction("Socials");
                                }
                                else
                                {
                                    TempData["ErrorMessage"] = "Unexpected response from the API.";
                                    return RedirectToAction("Socials");
                                }
                            }
                            else
                            {
                                TempData["ErrorMessage"] = "Failed to add to SMM service. Please try again later.";
                                return RedirectToAction("Socials");
                            }
                        }
                        catch (HttpRequestException)
                        {
                            TempData["ErrorMessage"] = "An error occurred while processing your request. Please try again later.";
                            return RedirectToAction("Socials");
                        }
                    }
                }
            }

            TempData["ErrorMessage"] = "Unauthorized.";
            return RedirectToAction("Socials");
        }

        [HttpPost]
        public async Task<ActionResult> Verify()
        {
            var email = Session["UserEmail"] as string;
            var user = _database.auths.FirstOrDefault(u => u.email == email);

            if (user != null)
            {
                AuthOTPManager otpManager = new AuthOTPManager();
                AuthMailer emailService = new AuthMailer();
                string token = otpManager.GenerateTokenWithExpiration(user.id, 5, HttpContext);
                long epochTimestamp = otpManager.GetCurrentEpochTime();
                

                var verificationEntry = await _database.auth_req_verify.FirstOrDefaultAsync(u => u.id == user.id);
                if (verificationEntry == null)
                {
                    var newreqVer = new auth_req_verify
                    {
                        id = user.id,
                        email = user.email,
                        token = token,
                        epoch_timestamp = epochTimestamp,
                        total_req_attempts = 1,
                        req_attempts = 1,
                        last_attempt_timestamp = DateTime.Today,
                        is_expired = 0
                    };

                    _database.auth_req_verify.Add(newreqVer);
                    _database.SaveChanges();

                    string link = "https://hyperfyp.com/Verified?token=" + token;
                    emailService.SendEmailConfirmation(user.email, user.username, link);

                    TempData["SuccessMessage"] = "You have been sent an email for your account verification.";
                    return RedirectToAction("Settings");
                } else
                {
                    if (verificationEntry.last_attempt_timestamp.Date == DateTime.Today && verificationEntry.req_attempts >= 3)
                    {
                        TempData["ErrorMessage"] = "You have reached the maximum account verification attempts for today. Please try again later!";
                        return RedirectToAction("Settings");
                    }
                    else if (verificationEntry.last_attempt_timestamp.Date != DateTime.Today)
                    {
                        verificationEntry.last_attempt_timestamp = DateTime.Today;
                        verificationEntry.req_attempts = 0;
                    }

                    verificationEntry.token = token;
                    verificationEntry.epoch_timestamp = epochTimestamp;
                    verificationEntry.total_req_attempts++;
                    verificationEntry.req_attempts++;
                    verificationEntry.is_expired++;

                    _database.SaveChanges();

                    string link = "https://hyperfyp.com/Verified?token=" + token;

                    emailService.SendEmailConfirmation(user.email, user.username, link);

                    TempData["SuccessMessage"] = "You have been sent an email for your account verification.";
                    return RedirectToAction("Settings");
                }
                
            }

            TempData["ErrorMessage"] = "Error! Failed to fetch user data from the database.";
            return RedirectToAction("Settings");
        }

        private readonly OfferService _offerService = new OfferService();

        public async Task<ActionResult> Surveys()
        {
            var userId = Session["UserId"] as string;
            var accountModels = await FetchUserDataFromDatabase(userId);
            var accountModel = accountModels.FirstOrDefault();
            //var offers = await _offerService.GetOffersAsync(userId);
            ViewBag.Data = accountModel;

            return View(/*offers*/);
        }

        //[HttpPost]
        //public ActionResult Callback(OfferCompletionCallbackModel model)
        //{
        //    // Validate the signature (optional but recommended)
        //    var isValid = ValidateSignature(model);
        //    if (!isValid)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }

        //    // Process the callback data (e.g., credit the user with the reward)
        //    // Update user points or rewards based on the offer completion

        //    return new HttpStatusCodeResult(HttpStatusCode.OK);
        //}

        //private bool ValidateSignature(OfferCompletionCallbackModel model)
        //{
        //    // Implement signature validation logic based on BitLabs documentation
        //    // This usually involves hashing the data with a secret key and comparing it with the provided signature
        //    return true;
        //}

        public async Task<ActionResult> Giveaway()
        {
            var userId = Session["UserId"] as string;
            var accountModels = await FetchUserDataFromDatabase(userId);
            var accountModel = accountModels.FirstOrDefault();
            ViewBag.Data = accountModel;

            return View();
        }

        public async Task<ActionResult> Blogs()
        {
            var userId = Session["UserId"] as string;
            var accountModels = await FetchUserDataFromDatabase(userId);
            var accountModel = accountModels.FirstOrDefault();
            ViewBag.Data = accountModel;

            return View();
        }
        [HttpPost]
        public JsonResult AddCoins(string username)
        {
            if (!string.IsNullOrEmpty(username))
            {
                // 1. Check if the session username matches the account username
                if (Session["UserName"] as string != username)
                {
                    // Handle mismatch or unauthorized request if needed
                    return Json(new { coinsAdded = 0 });
                }

                var user = _database.accounts.FirstOrDefault(u => u.username == username);
                if (user != null)
                {
                    user.total_coin_earned += 15;
                    user.coins += 15;
                    _database.SaveChanges();
                    return Json(new { coinsAdded = 15 });
                }
            }

            // If something goes wrong or username is null
            return Json(new { coinsAdded = 0 });
        }

        [HttpPost]
        public async Task<ActionResult> JoinGiveaway(string id)
        {
            try
            {
                string userId = Session["UserId"] as string;
                var user = await _database.accounts.FirstOrDefaultAsync(u => u.id == userId);

                string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Controllers", "Giveaway", "GiveawayDatabase.json");
                string json = System.IO.File.ReadAllText(jsonFilePath);
                var giveawayData = JsonConvert.DeserializeObject<GiveawayDatabase>(json);

                var giveaway = giveawayData.Giveaways.FirstOrDefault(g => g.id == id);

                if (giveaway == null)
                {
                    return Json(new { success = false, message = "Giveaway not found." });
                }

                // 2. Check requirements
                var requirements = giveaway.requirements.FirstOrDefault();
                if (requirements == null)
                {
                    return Json(new { success = false, message = "Giveaway requirements not found." });
                }

                if (requirements.isVerified.HasValue && requirements.isVerified.Value == 1 && user.isverified != 1)
                {
                    return Json(new { success = false, message = "You must be verified to join." });
                }

                if (requirements.refCount.HasValue && user.total_users_referred < requirements.refCount.Value)
                {
                    return Json(new { success = false, message = $"You need at least {requirements.refCount} referrals to join." });
                }

                if (requirements.offCount.HasValue && user.total_offers_completed < requirements.offCount.Value)
                {
                    return Json(new { success = false, message = $"You need to complete at least {requirements.offCount} offers to join." });
                }

                if (giveaway.participants == null)
                    giveaway.participants = new List<string>();

                if (!giveaway.participants.Contains(user.username))
                {
                    giveaway.participants.Add(user.username);
                    string updatedJson = JsonConvert.SerializeObject(giveawayData, Formatting.Indented);
                    System.IO.File.WriteAllText(jsonFilePath, updatedJson);
                }
                else
                {
                    return Json(new { success = false, message = "You already joined this giveaway." });
                }

                return Json(new { success = true, message = "You have successfully joined the giveaway!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "An error occurred: " + ex.Message });
            }
        }

        
    }
}