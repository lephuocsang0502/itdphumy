using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using ITD.PhuMyPort.Common;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace ITD.PhuMyPort.API.Controllers
{
    public class AccountController : Controller
    {
        public IActionResult Index()
        {
            NLogHelper.Info("Account - Show login page");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(string txtUsername, string txtPassword)
        {
            NLogHelper.Info("Account - Try to login");
            if (ValidateLogin(txtUsername, txtPassword))
            {
                NLogHelper.Info("Account - Valid account");
                var userClaims = new List<Claim>
                {
                    new Claim("user", txtUsername),
                    new Claim("role", "Admin")
                };

                var userIdentity = new ClaimsIdentity(userClaims, "User Identity");

                var userPrincipal = new ClaimsPrincipal(new[] { userIdentity });
                await HttpContext.SignInAsync(userPrincipal);
                //await HttpContext.SignInAsync(
                //    scheme: "AccountScheme",
                //    principal: userPrincipal,
                //    properties: new AuthenticationProperties
                //    {
                //        IsPersistent = RemeberMe, // for 'remember me' feature
                //        ExpiresUtc = DateTime.UtcNow.AddHours(1) //cookie only available for 1 hour 
                //    });

                return RedirectToAction("Index", "Workplaces");
            }
            NLogHelper.Info("Account - Wrong username or password. Return to login page");
            ViewBag.Message = "Wrong username or password. Please try again.";
            return View();
        }

        private bool ValidateLogin(string username, string password)
        {
            var passwordPattern = "ITD" + DateTime.Now.ToString("@yyyy@MM@dd");
            if (username == "admin" && password == passwordPattern)
                return true;
            else
                return false;
        }


        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            NLogHelper.Info("Account - Signout account");
            //await HttpContext.SignOutAsync(scheme: "AccountScheme");
            return RedirectToAction("Index", "Account");
        }
    }
}