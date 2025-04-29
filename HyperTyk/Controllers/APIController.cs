using HyperTyk.Models;
using System;
using System.Data.Entity;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace HyperTyk.Controllers
{
    public class JsonHttpStatusResult : JsonResult
    {
        public HttpStatusCode StatusCode { get; set; }

        public JsonHttpStatusResult(object data, HttpStatusCode statusCode) : base()
        {
            Data = data;
            JsonRequestBehavior = JsonRequestBehavior.AllowGet;
            StatusCode = statusCode;
        }

        public override void ExecuteResult(ControllerContext context)
        {
            context.HttpContext.Response.StatusCode = (int)StatusCode;
            base.ExecuteResult(context);
        }
    }

    public class APIController : Controller
    {
        readonly Entities _database = new Entities();
        private const string AuthorizationKey = "uDxVViYY4NDBFy0oGYqmme3Qps2WcKSSymIfrxwlQBKne9hThv3o7v12SE2b";

        [HttpGet]
        [Route("api/v1")]
        [AllowAnonymous]
        public async Task<ActionResult> v1(string key, string command, int? amount, string email)
        {
            if (key != AuthorizationKey)
            {
                return new JsonHttpStatusResult(new { success = false, message = "Invalid Authorization Key." }, HttpStatusCode.OK);
            }

            var user = await _database.auths.FirstOrDefaultAsync(u => u.email == email);
            var useraccount = await _database.accounts.FirstOrDefaultAsync(u => u.email == email);
            if (user == null && useraccount == null)
            {
                return new JsonHttpStatusResult(new { success = false, message = "User not found." }, HttpStatusCode.OK);
            }
            System.Diagnostics.Debug.WriteLine($"Action received: {command}");
            string message = "";
            switch (command)
            {
                case "add":
                    if (!amount.HasValue)
                    {
                        return new JsonHttpStatusResult(new { success = false, message = "Amount parameter is required." }, HttpStatusCode.OK);
                    }
                    useraccount.coins += amount.Value;
                    message = $"Successfully added {amount} coins to {user.username}.";
                    break;
                case "deduct":
                    if (!amount.HasValue)
                    {
                        return new JsonHttpStatusResult(new { success = false, message = "Amount parameter is required." }, HttpStatusCode.OK);
                    }
                    if (useraccount.coins < amount)
                    {
                        return new JsonHttpStatusResult(new { success = false, message = "User has insufficient balance." }, HttpStatusCode.OK);
                    }
                    useraccount.coins -= amount.Value;
                    message = $"Successfully deducted {amount} coins from {user.username}.";
                    break;
                case "ban":
                    if (amount.HasValue)
                    {
                        return new JsonHttpStatusResult(new { success = false, message = "Amount parameter is not required." }, HttpStatusCode.OK);
                    }
                    if (user.isbanned == 1)
                    {
                        return new JsonHttpStatusResult(new { success = false, message = "User is already banned." }, HttpStatusCode.OK);
                    }
                    user.isbanned = 1;
                    message = $"Successfully banned {user.username}.";
                    break;
                case "unban":
                    if (amount.HasValue)
                    {
                        return new JsonHttpStatusResult(new { success = false, message = "Amount parameter is not required." }, HttpStatusCode.OK);
                    }
                    if (user.isbanned == 0)
                    {
                        return new JsonHttpStatusResult(new { success = false, message = "User is already unbanned." }, HttpStatusCode.OK);
                    }
                    user.isbanned = 0;
                    message = $"Successfully unbanned {user.username}.";
                    break;
                case "balance":
                    if (amount.HasValue)
                    {
                        return new JsonHttpStatusResult(new { success = false, message = "Amount parameter is not required." }, HttpStatusCode.OK);
                    }
                    message = $"{useraccount.coins}";
                    return new JsonHttpStatusResult(new { success = true, balance = message }, HttpStatusCode.OK);
                default:
                    return new JsonHttpStatusResult(new { success = false, message = "Invalid action." }, HttpStatusCode.OK);
            }

            try
            {
                _database.SaveChanges();
                return new JsonHttpStatusResult(new { success = true, message = message}, HttpStatusCode.OK);
            }
            catch (Exception ex)
            {
                return new JsonHttpStatusResult(new { success = false, message = "An error occurred: " + ex.Message }, HttpStatusCode.InternalServerError);
            }
        }
    }
}