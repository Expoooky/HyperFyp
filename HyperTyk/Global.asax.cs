using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using HyperTyk.Controllers.Auth;

namespace HyperTyk
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static GiveawayService _giveawayService;
        protected void Application_Start()
        {
            Exception exception = Server.GetLastError();
            DependencyResolver.SetResolver(new AuthDependency());
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            _giveawayService = new GiveawayService();
        }

        protected void Application_End()
        {
            _giveawayService?.Dispose();
        }
    }
}
