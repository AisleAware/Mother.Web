using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mother.Web.ViewModels;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace EmployeeManagementAspNetCore.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
        }

        [HttpPost("account/logout")]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("index", "home");
        }

        [HttpGet("account/register")]
        [AllowAnonymous]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("account/register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email
                };
                var result = await userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("report", "home");
                }

                foreach (var error in result.Errors)
                {
                    // Add error to the ModelState object to be later used by the asp-validation-summary tag helper in the Register.cshtml view
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View();
        }

        [HttpGet("account/login")]
        [AllowAnonymous]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("account/login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    if (!String.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        //Important!  Use LocalRedirect() instead of Redirect() to avoid open redirect attacks through malicious external web sites
                        //return LocalRedirect(returnUrl);    // Creates an exception when a non-local redirect is attempted

                        return Redirect(returnUrl);         // Avoids the exception when paired with the Url.IsLocalUrl check above
                    }
                    else
                    {
                        return RedirectToAction("report", "home");
                    }
                }

                // Add error to the ModelState object to be later used by the asp-validation-summary tag helper in the Register.cshtml view
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt");
            }

            return View(model);
        }
    }
}
