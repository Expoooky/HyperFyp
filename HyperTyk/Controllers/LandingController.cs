using HyperTyk.Controllers.Auth;
using HyperTyk.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using static System.Net.WebRequestMethods;

namespace HyperTyk.Controllers
{
    public class LandingController : Controller
    {
        private readonly IAuthenticationService _authenticationService;
        readonly Entities _database = new Entities();

        public LandingController(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        // GET: Landing Page
        public ActionResult Index()
        {
            return View();
        }

        // GET: Login Page
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                var userEmail = User.Identity.Name;

                using (var _database = new Entities()) // Assuming Entities is your DbContext
                {
                    var user = _database.auths.FirstOrDefault(u => u.email == userEmail);

                    if (user != null && user.isbanned == 1)
                    {
                        TempData["ErrorMessage"] = "Seems like your account has been banned. You may contact us or open a ticket!";
                        Session.Clear();
                        FormsAuthentication.SignOut();
                        return View("~/Views/Landing/Auth/Login.cshtml");
                    }

                    var roleName = Session["UserType"];

                    // Original redirection logic based on the role
                    switch (roleName)
                    {
                        case "Admin":
                            return RedirectToAction("Index", "Admin");
                        case "Normal":
                            return RedirectToAction("Index", "Home");
                        default:
                            TempData["ErrorMessage"] = "Your session has expired. Please refresh this page again.";
                            FormsAuthentication.SignOut();
                            Session.Clear();
                            return View("~/Views/Landing/Auth/Login.cshtml");
                    }
                }


                
            }
            return View("~/Views/Landing/Auth/Login.cshtml");
        }

        // GET: Login Page
        public ActionResult Register(string r)
        {
            if (User.Identity.IsAuthenticated)
            {
                var roleName = Session["UserType"];

                switch (roleName)
                {
                    case "Admin":
                        return RedirectToAction("Index", "Admin");
                    case "Normal":
                        return RedirectToAction("Index", "Home");
                    default:
                        return View("~/Views/Landing/Auth/Register.cshtml");
                }

            }
            ViewData["RegistrationCode"] = r;
            return View("~/Views/Landing/Auth/Register.cshtml");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(auth userdatabase, string ReturnUrl)
        {
            var user = await _database.auths.FirstOrDefaultAsync(u => u.username == userdatabase.loginuser || u.email == userdatabase.loginuser);

            if (user != null)
            {
                var account = await _database.accounts
                    .Where(a => a.id == user.id)
                    .Select(a => new
                    {
                        a.id,
                        a.avatar,
                        a.referralcode,
                        a.isverified
                    })
                    .FirstOrDefaultAsync();

                if (account != null)
                {
                    if (AuthPasswordManager.VerifyPassword(userdatabase.password, user.password))
                    {
                        FormsAuthentication.SetAuthCookie(user.email, true);

                        Session["UserId"] = user.id;
                        Session["UserEmail"] = user.email;
                        Session["UserName"] = user.username;
                        Session["UserType"] = user.usertype;
                        Session["AccountAvatar"] = account.avatar;
                        Session["isVerified"] = account.isverified;

                        if (user.isbanned == 1)
                        {
                            TempData["ErrorMessage"] = "Seems like your account has been banned. You may contact us or open a ticket!";
                            Session.Clear();
                            FormsAuthentication.SignOut();
                            return View("~/Views/Landing/Auth/Login.cshtml");
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(ReturnUrl) && Url.IsLocalUrl(ReturnUrl))
                            {
                                return Redirect(ReturnUrl);
                            }
                            else
                            {
                                switch (user.usertype)
                                {
                                    case "Admin":
                                        return RedirectToAction("Index", "Admin");
                                    case "Normal":
                                        Session["AccountRefCode"] = account.referralcode;
                                        return RedirectToAction("Index", "Home");
                                    default:
                                        TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                                        FormsAuthentication.SignOut();
                                        Session.Clear();
                                        return View("~/Views/Landing/Auth/Login.cshtml");
                                }
                            }
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Invalid Email or Password. Please Try Again!";
                        return View("~/Views/Landing/Auth/Login.cshtml");
                    }
                }
            }

            TempData["ErrorMessage"] = "Invalid Email or Password. Please Try Again!";
            return View("~/Views/Landing/Auth/Login.cshtml");
        }

        public byte[] ConvertImageToByteArray(string imagePath)
        {
            string resolvedImagePath = Server.MapPath(imagePath); // Resolve server-side path

            if (!System.IO.File.Exists(resolvedImagePath))
            {
                // Handle file not found (log an error, return null or a default value)
                return null;
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(resolvedImagePath);
            return imageBytes;
        }

        // POST: Sign Up
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(string username, string email, string password, string referralcode)
        {
            if (_database.auths.Any(a => a.username == username))
            {
                TempData["ErrorMessage"] = "Username already exists.";
                return View("~/Views/Landing/Auth/Register.cshtml");
            }
            else if (username.Length > 12)
            {
                TempData["ErrorMessage"] = "Username cannot exceed 12 characters.";
                return View("~/Views/Landing/Auth/Register.cshtml");
            }

            if (_database.auths.Any(a => a.email == email))
            {
                TempData["ErrorMessage"] = "Email already exists.";
                return View("~/Views/Landing/Auth/Register.cshtml");
            }
            else if (_database.auths.Any(a => a.email.Replace(".", "") == email.Replace(".", "")))
            {
                TempData["ErrorMessage"] = "Email already exists.";
                return View("~/Views/Landing/Auth/Register.cshtml");
            }

            if (ModelState.IsValid)
            {
                string hashedPassword = AuthPasswordManager.HashPassword(password);
                string refcode = AuthReferralCodeManager.GenerateReferralCode();
                byte[] imageBytes = ConvertImageToByteArray("~/Items/9440461.jpg");
                string uuid = AuthUUIDManager.GenerateUUID(username);
                AuthOTPManager otpManager = new AuthOTPManager();
                double coinreward = 0;

                if (!string.IsNullOrEmpty(referralcode))
                {
                    var referredAccount = _database.accounts.FirstOrDefault(a => a.referralcode == referralcode);

                    if (referredAccount != null)
                    {
                        referredAccount.total_users_referred++;

                        if (referredAccount.isverified == 1)
                        {
                            referredAccount.coins += 5;
                            referredAccount.total_coin_earned += 5;

                            coinreward = 2.5;

                            TempData["SuccessMessage"] = $"You have received 2.5 coins as a referral bonus.";
                        }

                        var tableRefLog = new referral_log()
                        {
                            referrer_id = referredAccount.id,
                            referred_id = uuid,
                            referred_email = email,
                            referred_username = username,
                            epochtime = otpManager.GetCurrentEpochTime()
                        };

                        var referrer = new account_referrer()
                        {
                            id = uuid,
                            referrer_id = referredAccount.id,
                            referrer_code = referralcode
                        };

                        

                        _database.referral_log.Add(tableRefLog);
                        _database.account_referrer.Add(referrer);
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Referral Code does not exist.";
                        return View("~/Views/Landing/Auth/Register.cshtml");
                    }
                }

                var tableAuth = new auth()
                {
                    id = uuid,
                    username = username,
                    email = email,
                    password = hashedPassword,
                    usertype = "Normal",
                    datecreated = DateTime.UtcNow,
                    isbanned = 0
                };

                var tableAccount = new account()
                {
                    id = uuid,
                    username = username,
                    email = email,
                    avatar = imageBytes,
                    referralcode = refcode,
                    coins = coinreward,
                    total_coin_earned = coinreward,
                    total_coin_spent = 0,
                    total_users_referred = 0,
                    total_offers_completed = 0,
                    isverified = 0
                };

                var tableReqReset = new auth_req_forgot()
                {
                    id = uuid,
                    email = email,
                    pin = null,
                    token = null,
                    epoch_timestamp = null,
                    total_req_attempts = 0,
                    req_attempts = 0,
                    last_attempt_timestamp = null,
                };

                var tableReqPassword = new auth_chng_password()
                {
                    id = uuid,
                    email = email,
                    token = null,
                    success_request_attempts = 0
                };

                _database.auths.Add(tableAuth);
                _database.accounts.Add(tableAccount);
                _database.auth_req_forgot.Add(tableReqReset);
                _database.auth_chng_password.Add(tableReqPassword);
                _database.SaveChanges();

                Session["UserId"] = uuid;
                Session["UserEmail"] = email;
                Session["UserName"] = username;
                Session["UserType"] = "Normal";
                Session["AccountRefCode"] = refcode;
                Session["AccountAvatar"] = imageBytes;
                Session["isVerified"] = 0;

                FormsAuthentication.SetAuthCookie(email, true);
                return RedirectToAction("login");
            }
            return View("~/Views/Landing/Auth/Register.cshtml");
        }

        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            Session.Clear();

            var data = new { success = true };

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Forgot()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Forgot(auth userdatabase)
        {
            var user = _authenticationService.GetUserByUsername(userdatabase.loginuser) ??
                       _authenticationService.GetUserByEmail(userdatabase.loginuser);

            if (user != null)
            {
                var resetEntry = _database.auth_req_forgot.FirstOrDefault(r => r.id == user.id);

                if (resetEntry.last_attempt_timestamp?.Date == DateTime.Today && resetEntry.req_attempts >= 3)
                {
                    // Max attempts for today reached
                    TempData["ErrorMessage"] = "You have reached the maximum password reset attempts for today. Please try again later!";
                    return View("~/Views/Landing/Forgot.cshtml");
                }
                else if (resetEntry.last_attempt_timestamp?.Date != DateTime.Today)
                {
                    // Reset attempts since it's a new day
                    resetEntry.last_attempt_timestamp = DateTime.Today;
                    resetEntry.req_attempts = 0;
                    _database.SaveChanges();
                }

                AuthOTPManager otpManager = new AuthOTPManager();
                string otp = otpManager.GenerateOTP();
                string token = otpManager.GenerateTokenWithExpiration(user.id, 5, HttpContext);
                long epochTimestamp = otpManager.GetCurrentEpochTime();

                AuthMailer emailService = new AuthMailer();
                emailService.SendOTP(user.email, user.username, otp);

                resetEntry.pin = int.Parse(otp);
                resetEntry.token = token;
                resetEntry.epoch_timestamp = epochTimestamp;
                resetEntry.total_req_attempts++;
                resetEntry.req_attempts++;
                resetEntry.is_expired = 0;

                _database.SaveChanges();

                TempData["SuccessMessage"] = "Please check your email for a six-digit confirmation code.";
                return RedirectToAction("OTP", new { token });
            }

            TempData["ErrorMessage"] = "Invalid Email or Username.";
            return View();
        }
        public ActionResult OTP(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AuthOTPManager otpManager = new AuthOTPManager();
            string userAgentHash = otpManager.GetUserAgentHash(HttpContext);

            if (!otpManager.IsTokenValid(token, userAgentHash))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var resetEntry = _database.auth_req_forgot.FirstOrDefault(r => r.token == token);
            if (resetEntry == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            } else if (resetEntry.is_expired == 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Session["UserId"] = otpManager.GetUserIdfromToken(token);
            Session["Token"] = token; 

            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OTP(int? enteredOTP)
        {
            if (enteredOTP == null)
            {
                string token = Session["Token"] as string;
                TempData["ErrorMessage"] = "Invalid OTP!";
                return RedirectToAction("OTP", new { token });
            }

            var userId = Session["UserId"] as string;

            var user = _database.auth_req_forgot.FirstOrDefault(u => u.id == userId);

            if (user != null)
            {
                if (user.pin == enteredOTP)
                {
                    AuthOTPManager otpManager = new AuthOTPManager();
                    var token = otpManager.GenerateTokenWithExpiration(user.id, 5, HttpContext);

                    user.is_expired = 1;

                    var changePasswordEntry = _database.auth_chng_password.FirstOrDefault(c => c.id == userId);

                    if (changePasswordEntry != null)
                    {
                        changePasswordEntry.token = token;
                        changePasswordEntry.success_request_attempts++;
                        changePasswordEntry.is_expired = 0;
                    }

                    _database.SaveChanges();

                    Session["UserId"] = otpManager.GetUserIdfromToken(token);

                    TempData["SuccessMessage"] = "Success! You can now choose a new password.";
                    return RedirectToAction("Reset_Password", new { token });
                }
                else
                {
                    string token = Session["Token"] as string;
                    TempData["ErrorMessage"] = "Incorrect OTP entered.";
                    return RedirectToAction("OTP", new { token });
                }
            }

            Session.Clear();
            TempData["ErrorMessage"] = "Invalid Token!";
            return RedirectToAction("Login");
        }

        public ActionResult Reset_Password(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            AuthOTPManager otpManager = new AuthOTPManager();
            string userAgentHash = otpManager.GetUserAgentHash(HttpContext);

            if (!otpManager.IsTokenValid(token, userAgentHash))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var resetPassEntry = _database.auth_chng_password.FirstOrDefault(r => r.token == token);
            if (resetPassEntry == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            } else if (resetPassEntry.is_expired == 1)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            return View();
        }

        [HttpPost]
        public ActionResult New_Password(string newPassword)
        {
            var userId = Session["UserId"] as string;

            var user = _database.auths.FirstOrDefault(u => u.id == userId);

            if (user != null)
            {
                string hashedPassword = AuthPasswordManager.HashPassword(newPassword);

                user.password = hashedPassword;
            }

            var resetreq = _database.auth_chng_password.FirstOrDefault(u => u.id == userId);

            resetreq.is_expired = 1;

            _database.SaveChanges();

            TempData["SuccessMessage"] = "You have successfully set a new password.";
            return RedirectToAction("Login");
        }

        public ActionResult Privacy() { return View(); }
        public ActionResult Terms() { return View(); }
        public ActionResult Punishment() { return View(); }


        [Authorize]
        public ActionResult Verified(string token = null)
        {
            if (string.IsNullOrEmpty(token))
            {
                TempData["ErrorMessage"] = "How dare you enter this room 😡.";
                return RedirectToAction("Index", "Home");
            }

            AuthOTPManager otpManager = new AuthOTPManager();
            string userAgentHash = otpManager.GetUserAgentHash(HttpContext);

            if (!otpManager.IsTokenValid(token, userAgentHash))
            {
                TempData["ErrorMessage"] = "Invalid or expired verification token.";
                return RedirectToAction("Index", "Home");
            }

            // Since the token is valid, find the user and update their status
            var verificationEntry = _database.auth_req_verify.FirstOrDefault(u => u.token == token);
            if (verificationEntry == null)
            {
                TempData["ErrorMessage"] = "Verification failed. Please try again.";
                return RedirectToAction("Index", "Home");
            }

            var user = _authenticationService.GetUserByEmail(verificationEntry.email);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Verification failed. User not found.";
                return RedirectToAction("Index", "Home");
            }

            var account = _database.accounts.FirstOrDefault(u => u.id == user.id);

            account.isverified = 1;

            _database.SaveChanges();

            Session["isVerified"] = account.isverified;
            
            return View("~/Views/Home/Verified.cshtml");
        }

    }
}