using HyperTyk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HyperTyk.Controllers.Auth
{
    public class AuthDependency : IDependencyResolver
    {
        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(LandingController))
            {
                return new LandingController(new AuthService(new Entities()));
            }
            return null;
        }   

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return Enumerable.Empty<object>();
        }
    }
}