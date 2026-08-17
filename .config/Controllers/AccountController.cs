using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

using System;
using System.Security.Policy;
using System.Text;
using WangenPizza.Api_s.Models;
using WangenPizza.Dtos;
using WangenPizza.Helper;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using WangenPizza.Services;

namespace WangenPizza.Controllers
{
    /// <summary>
    /// Gesamter Account-Bereich muss anonym erreichbar sein — sonst /Account/Login → Challenge → /Account/Login (ERR_TOO_MANY_REDIRECTS).
    /// </summary>
    [AllowAnonymous]
	public class AccountController : Controller
	{
		#region Ctor

		private readonly UserManager<ApplicationUser> userManager;
		private readonly SignInManager<ApplicationUser> signInManager;
        private readonly IEmailHtmlTemplateService emailHtmlTemplateService;
        private readonly IUserService userService;
        private readonly IMailService mailService;
        private readonly IConfiguration configuration;
        private readonly ILogger<AccountController> logger;

        public AccountController(IUserService userService, IMailService mailService, IConfiguration configuration, UserManager<ApplicationUser> _UserManager, SignInManager<ApplicationUser> _SignInManager, IEmailHtmlTemplateService emailHtmlTemplateService, ILogger<AccountController> logger)
		{
            this.userService = userService;
            this.mailService = mailService;
            this.configuration = configuration;
            userManager = _UserManager;
			signInManager = _SignInManager;
            this.emailHtmlTemplateService = emailHtmlTemplateService;
            this.logger = logger;

        }
        #endregion




        #region Login
        [HttpGet]
        [AllowAnonymous]
		public async Task<IActionResult> Login(string? returnUrl = null)
		{
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Home");
                await signInManager.SignOutAsync();
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
		}
		[HttpPost]
        [AllowAnonymous]
		public async Task<IActionResult> Login(LoginDto model, string? returnUrl = null)
		{
			try
			{

				if (ModelState.IsValid)
				{

					var user = await userManager.FindByNameAsync(model.UserName);
                    if(user == null) 
                    {
                        TempData["InvalidEmail"] = "error";
                        ViewData["ReturnUrl"] = returnUrl;
                        return View(model);
                    }


                    var result = await signInManager.PasswordSignInAsync(user, model.Password, true, false);

					if (result.Succeeded)
					{
                        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                            return LocalRedirect(returnUrl);
                        return RedirectToAction("Index", "Home");
                    }
                    if (result.IsLockedOut)
                    {
                        TempData["LoginError"] = "Konto vorübergehend gesperrt. Bitte später erneut versuchen.";
                        ViewData["ReturnUrl"] = returnUrl;
                        return View(model);
                    }
                    if (result.IsNotAllowed)
                    {
                        TempData["LoginError"] = "Anmeldung nicht erlaubt (z. B. E-Mail nicht bestätigt). Bitte Support kontaktieren.";
                        ViewData["ReturnUrl"] = returnUrl;
                        return View(model);
                    }
                    TempData["InvalidUserNameOrPassword"] = "error";
                    ViewData["ReturnUrl"] = returnUrl;
                    return View(model);

                }
                TempData[key: "ErrorMessage"] = "error";
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);

			}
			catch (Exception ex)
			{
                logger.LogError(ex, "Account/Login fehlgeschlagen für Benutzer: {UserName}", model?.UserName);
                TempData["LoginError"] = "Anmeldung fehlgeschlagen (Server oder Datenbank). Bitte später erneut versuchen oder Support kontaktieren.";
                ViewData["ReturnUrl"] = returnUrl;
                return View(model);
			}
		}

        #endregion

        #region AccessDenied

        [HttpGet]
        [AllowAnonymous]
        public IActionResult AccessDenied()
        {
            return View();
        }

        #endregion

        #region Register
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto model)
        {
            try
            {

                if (ModelState.IsValid)
                {
                    var user = new ApplicationUser()
                    {
                        UserName = model.UserName,
                        Email = model.Email,
                    };
                    var userModel = await userManager.FindByEmailAsync(model.Email);

                    if (userModel != null)
                    {
                        TempData[key: "AlreadyRegisted"] = "error";
                        return View();
                    }
                  
                    var result = await userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        IdentityResult roleresult = await userManager.AddToRoleAsync(user, "Admin");
                        return View(model);
                    }
                    else
                    {

                        return View();

                    }

                }

                return View(model);

            }

            catch (Exception)
            {
                return View(model);

            }
        }

        #endregion



        #region Confirm email 

        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return NotFound();

            var result = await userService.ConfirmEmailAsync(userId, token);

            if (result.IsSuccess)
            {
                return RedirectToAction("EmailActivated");
            }
            return View();
        }
        #endregion

        #region Confirm email 

        public IActionResult EmailActivated()
        {

            return View();
        }
        #endregion



        #region LogOut (Sign Out)

        [HttpPost]
        public async Task<IActionResult> LogOut()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }


        #endregion

        #region Forget Password
        [HttpGet]
        public IActionResult ForgetPassword()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(string email)
        {

            var user = await userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResetLink = Url.Action("ResetPassword", "Account", new { Email = email, Token = token }, Request.Scheme);
                string body = emailHtmlTemplateService.GetResetPasswordTemplate(user.UserName, passwordResetLink);

                MailRequest mailRequest = new MailRequest
                {
                    ToEmail = user.Email,
                    Subject = "Wangen",
                    Body = body,

                };
                await mailService.SendEmailAsync(mailRequest, default);
                UserManagerResponse response = new UserManagerResponse()
                {
                    IsSuccess = true,
                    Message = "Der Link zum Zurücksetzen des Passworts wurde an Ihre E-Mail-Adresse gesendet."
                };
                TempData[key: "Succeeded"] = "done";
                return View();
            }
            TempData[key: "ErrorMessage"] = "error";
            return View();

        }

        #endregion

        #region Reset Password
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string Email, string Token)
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);

                if (user != null)
                {


                    if (model.Password != model.ConfirmPassword)
                    {
                        TempData[key: "notmatched"] = "error";
                        return View(model);
                    };


                    var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);

                    if (result.Succeeded)
                    {
                        TempData[key: "Succeeded"] = "done";
                        return View(model);
                    }
                        TempData[key: "ErrorMessage"] = "error";
                    return View(model);
                }

            }
            TempData[key: "ErrorMessage"] = "error";
            return View(model);
        }

        #endregion




    }
}
