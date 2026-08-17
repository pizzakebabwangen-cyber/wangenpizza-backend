using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using WangenPizza.Helper;
using WangenPizza.Api_s.Models;
using WangenPizza.Helper.Response;
using WangenPizza.Services;

namespace QuickMover.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        #region Ctor
        private readonly IUserService _userService;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMailService mailService;
        private readonly IEmailHtmlTemplateService emailHtmlTemplateService;
        private IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public AuthController(IUserService userService, IConfiguration configuration  , UserManager<ApplicationUser> _userManager, RoleManager<IdentityRole> roleManager, IMailService mailService ,IEmailHtmlTemplateService emailHtmlTemplateService )
        {

            _userService = userService;

            _configuration = configuration;
            this._userManager = _userManager;
            _roleManager = roleManager;
            this.mailService = mailService;
            this.emailHtmlTemplateService = emailHtmlTemplateService;
        }
        #endregion


        #region Register 
        [HttpPost("Register")]
        public async Task<IActionResult> RegisterUserAsync(RegisterModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .Where(e => !string.IsNullOrWhiteSpace(e))
                    .ToArray();

                return BadRequest(new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = errors.Length > 0 ? string.Join(" ", errors) : "Einige Eingaben sind ungültig",
                    Errors = errors
                });
            }

            try
            {
                var result = await _userService.RegisterUserAsync(model);


                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            catch
            {
                return StatusCode(500, new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Registrierung konnte nicht abgeschlossen werden. Bitte später erneut versuchen."
                });
            }
        }

        #endregion

        /// <summary>
        /// E-Mail-Aktivierung per Link (GET). Leitet nach erfolgreicher Bestätigung auf die Kunden-Website weiter.
        /// </summary>
        [AllowAnonymous]
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmailFromLink([FromQuery] string userId, [FromQuery] string token)
        {
            var frontend = (_configuration["FrontendAppUrl"] ?? "https://pizzawangen.ch").TrimEnd('/');
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
                return Redirect($"{frontend}/login?emailConfirmed=0");

            var result = await _userService.ConfirmEmailAsync(userId, token);
            if (result.IsSuccess)
                return Redirect($"{frontend}/login?emailConfirmed=1");

            var msg = Uri.EscapeDataString(result.Message ?? "E-Mail wurde nicht bestätigt");
            return Redirect($"{frontend}/login?emailConfirmed=0&emailConfirmError={msg}");
        }

        #region Login
        [HttpPost("Login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginModel model)
        {

            if (ModelState.IsValid)
            {
                var result = await _userService.LoginUserAsync(model);


                if (result.IsSuccess)
                {
                    return Ok(result);
                }

                return BadRequest(result);
            }
            return BadRequest("Some properties are not valid");
        }

        private string CreateToken(ClaimsPrincipal user)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Forget Password
        [HttpPost("ForgetPassword/{email}")]
        public async Task<IActionResult> ForgetPassword( string email ) {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);

                if (user != null)
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                    var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
                    var frontend = (_configuration["FrontendAppUrl"] ?? "https://www.pizzawangen.ch").TrimEnd('/');
                    var encodedEmail = Uri.EscapeDataString(email);
                    var passwordResetLink = $"{frontend}/resetPassword?email={encodedEmail}&token={encodedToken}";
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
                        Message = "Reset password URL has been sent to the email successfully!"
                    };
                    return Ok(response);

                }
                return BadRequest(new { isSuccess = false, message = "E-Mail nicht gefunden." });
            }
            catch
            {
                return StatusCode(500, new { isSuccess = false, message = "E-Mail konnte nicht gesendet werden. Bitte später erneut versuchen." });
            }
        }

        #endregion

        #region Reset Password
        // api/auth/resetpassword
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _userService.ResetPasswordAsync(model);

                if (result.IsSuccess)
                    return Ok(result);

                return BadRequest(result);
            }

            return BadRequest("Some properties are not valid");
        }

        #endregion

        #region Get Account Data
        [HttpPost("GetAccountData/{id}")]
        public async Task<IActionResult> GetUser(string id)
        {
            var data = _userService.GetAccount(id);

            if (data != null)
            {
                UserAccountCustomResponse Cusotm = new UserAccountCustomResponse()

                {
                    Code = "200",
                     Status = "Done",
                    Message = "Account data returned",
                    Record = await data


                };
                return Ok(Cusotm);

            }
            CustomResponse customResponse = new CustomResponse()
            {
                Code = "400",
                Message = "There Is No User With This Id",
                Status = "Faild"
            };
            return Ok(customResponse);

        }

        #endregion

        #region Edit Profile

        [HttpPost("EditAccount/{id}")]
        public async Task<IActionResult> EditAccount(string id, [FromForm] EditProfileModel model)
        {

            if (ModelState.IsValid)
            {
                model.Id = id;
                var data = await _userService.EditProfile(model);

                if (data.IsSuccess)

                {
                    return Ok(data);
                }

                return BadRequest(data);
            }

            return BadRequest("Some properties are not valid");


        }
        #endregion

        #region Edit Password
        [HttpPost("EditPassword")]
        public async Task<IActionResult> EditPassword( [FromBody] EditPassword model)
        {

            if (ModelState.IsValid)
            {
                var data = await _userService.EditPassword(model);

                if (data.IsSuccess)

                {
                    return Ok(data);
                }

                return BadRequest(data);
            }

            return BadRequest("Some properties are not valid");


        }

        #endregion




    }



}
