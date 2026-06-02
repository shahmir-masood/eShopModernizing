using log4net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace eShopModernizedMVC.Controllers
{
    public class AccountController : Controller
    {
        private static readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        [ActionName("SignIn")]
        public IActionResult SignInUser()
        {
            _log.Info($"Now processing... AccountController.SignIn");
            if (!User.Identity.IsAuthenticated)
            {
                return Challenge(
                    new AuthenticationProperties { RedirectUri = "/" },
                    OpenIdConnectDefaults.AuthenticationScheme);
            }

            return RedirectToAction("Index", "Catalog");
        }

        [ActionName("SignOut")]
        public IActionResult SignOutUser()
        {
            _log.Info($"Now processing... AccountController.SignOut");
            return SignOut(
                new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme,
                OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult EndSession()
        {
            _log.Info($"Now processing... AccountController.EndSession");
            return SignOut(
                new AuthenticationProperties { RedirectUri = "/" },
                CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
