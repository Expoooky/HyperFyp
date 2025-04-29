using HyperTyk.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.ApplicationServices;
using System.Web.Helpers;

namespace HyperTyk.Controllers.Auth
{
    public interface IAuthenticationService
    {
        auth AuthenticateUser(string email, string password);
        string GetEmailByUsername(string loginuser);
        auth GetUserByEmail(string email);
        auth GetUserByUsername(string username);
        string GetUserRole(string email);
    }
    public class AuthService : IAuthenticationService
    {
        private readonly Entities _database;

        public AuthService(Entities database)
        {
            _database = database;
        }
        public auth AuthenticateUser(string email, string password)
        {
            return _database.auths.FirstOrDefault(x => x.email == email && x.password == password);
        }

        public string GetEmailByUsername(string loginuser)
        {
            using (var context = new Entities())
            {
                return context.auths
                              .Where(u => u.username == loginuser)
                              .Select(u => u.email)              
                              .FirstOrDefault();                 
            }
        }

        public auth GetUserByEmail(string email)
        {
            using (var context = new Entities())
            {
                return context.auths.FirstOrDefault(u => u.email == email);
            }
        }

        public auth GetUserByUsername(string username)
        {
            using (var context = new Entities())
            {
               return context.auths.FirstOrDefault(u => u.username == username);
            }
        }

        public string GetUserRole(string email)
        {
            var user = _database.auths.FirstOrDefault(u => u.email == email);
            return user != null ? user.usertype : string.Empty;
        }
    }
}