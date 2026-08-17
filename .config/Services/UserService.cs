

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using QuickMover.Helper;
using MimeKit;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Security.Policy;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using WangenPizza.Helper;
using WangenPizza.Api_s.Models;
using WangenPizza.Helper.Response;
using MailKit;

namespace WangenPizza.Services
{
    public class UserService : IUserService
    {
		#region Ctor 

		private UserManager<ApplicationUser> _userManger;
		private IConfiguration _configuration;
        private readonly Interfaces.IMailService mailService;
        private readonly IEmailHtmlTemplateService emailHtmlTemplateService;

        public UserService(UserManager<ApplicationUser> userManager, IConfiguration configuration , Interfaces.IMailService mailService , IEmailHtmlTemplateService emailHtmlTemplateService)
		{
			_userManger = userManager;
			_configuration = configuration;
            this.mailService = mailService;
            this.emailHtmlTemplateService = emailHtmlTemplateService;
        }
		#endregion

		#region Register
		public async Task<UserManagerResponse> RegisterUserAsync(RegisterModel model)
		{
            if (model == null)
                throw new NullReferenceException("Reigster Model is null");


            var user = new ApplicationUser

            {
                UserName = model.Email,
				FullName = model.FullName,
                Email = model.Email,
                PhoneNumber = model.PhoneNumber,
                Salute = model.Salute,
                City = model.City,
                Street = model.Street,
                PostBox = model.PostBox,
				
            };

            var olduser = await _userManger.FindByEmailAsync(model.Email);

			if(olduser !=null)
			{
                return new UserManagerResponse
                {
                    Message = "Benutzer bereits registriert",
                    IsSuccess = false,
                    
                };
            }

            var result = await _userManger.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {



                if (result != null)
                {
                    IdentityResult roleresult = await _userManger.AddToRoleAsync(user, "User");
                    string body = await emailHtmlTemplateService.GetActvateEmailTemplate(user);
                    MailRequest mailRequest = new MailRequest
                    {
                        ToEmail = user.Email,
                        Subject = "Wangen Pizza",
                        Body = body,

                    };
                    await mailService.SendEmailAsync(mailRequest, default);

                }


                var claims = new[]
            {
                new Claim("Id", user.Id),
                new Claim("UserName", user.UserName),
                new Claim("Email", model.Email),
                new Claim("PhoneNumber",$"{user.PhoneNumber}"),
                new Claim("Salute",$"{user.Salute}"),
                new Claim("City",$"{user.City}"),
                new Claim("Street",$"{user.Street}"),
                new Claim("PostBox",$"{user.PostBox}"),




            };


                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

                var token = new JwtSecurityToken(
                    issuer: _configuration["JWT:Issuer"],
                    audience: _configuration["JWT:Audience"],
                     claims: claims,
                    expires: DateTime.Now.AddDays(30),
                    signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

                string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

                return new UserManagerResponse
                {
                    Message = "User created successfully!",
                    IsSuccess = true,
                    Token = tokenAsString
                };

            }

            return new UserManagerResponse
            {
                Message = "User did not create",
                IsSuccess = false,
                Errors = result.Errors.Select(e => e.Description)
            };
        }

            #endregion

        #region Confirm Email

            public async Task<UserManagerResponse> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManger.FindByIdAsync(userId);
            if (user == null)
                return new UserManagerResponse
                {
                    IsSuccess = false,
                    Message = "Benutzer nicht gefunden"
                };

            var decodedToken = WebEncoders.Base64UrlDecode(token);
            string normalToken = Encoding.UTF8.GetString(decodedToken);

            var result = await _userManger.ConfirmEmailAsync(user, normalToken);

            if (result.Succeeded)
                return new UserManagerResponse
                {
                    Message = "E-Mail erfolgreich bestätigt!",
                    IsSuccess = true,
                };

            return new UserManagerResponse
            {
                IsSuccess = false,
                Message = "E-Mail wurde nicht bestätigt",
                Errors = result.Errors.Select(e => e.Description)
            };
        }

        #endregion

        #region Login  

        public async Task<UserManagerResponse> LoginUserAsync(LoginModel model)
		{
			var user = await _userManger.FindByEmailAsync(model.Email);

			if (user == null)
			{
				return new UserManagerResponse
				{
					Message = "Es gibt keinen Benutzer mit dieser E-Mail-Adresse",
					IsSuccess = false,
				};
			}
            if (!user.EmailConfirmed)
            {
                return new UserManagerResponse
                {
                    Message = "E-Mail nicht bestätigt",
                    IsSuccess = false,
                };
            }
            var result = await _userManger.CheckPasswordAsync(user, model.Password);

			if (!result)
				return new UserManagerResponse
				{
					Message = "Ungültiges Passwort",
					IsSuccess = false,
				};

			var claims = new[]
			{
				new Claim("Id", user.Id),
				new Claim("UserName", user.UserName),
				new Claim("FullName", user.FullName),
				new Claim("Email", model.Email),
				new Claim("PhoneNumber",$"{user.PhoneNumber}"),
				new Claim("Salute",$"{user.Salute}"),
				new Claim("City",$"{user.City}"),
				new Claim("Street",$"{user.Street}"),
				new Claim("PostBox",$"{user.PostBox}"),



                 //new Claim("Role",$"{user.role}")

                //new Claim("IsAgree",$"{user.IsAgree}"),

            };


			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

			var token = new JwtSecurityToken(
				issuer: _configuration["JWT:Issuer"],
				audience: _configuration["JWT:Audience"],
				 claims: claims,
				expires: DateTime.Now.AddDays(30),
				signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

			string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);

			return new UserManagerResponse
			{
				Message = "User Login successfully!",
				IsSuccess = true,
				Token = tokenAsString
			};
		}

		#endregion

		#region   ForgetPassword
		public async Task<UserManagerResponse> ForgetPasswordAsync(string email)
		{
			var user = await _userManger.FindByEmailAsync(email);
			if (user == null)
				return new UserManagerResponse
				{
					IsSuccess = false,
					Message = "No user associated with email",
				};

            var token = await _userManger.GeneratePasswordResetTokenAsync(user);
            string url = $"{_configuration["AppUrl"]}/Account/ResetPassword?email={email}&token={token}";

            MailRequest mailRequest = new MailRequest
            {
                ToEmail = email,
                Subject = "Wangen",
                Body = url,

            };
            await mailService.SendEmailAsync(mailRequest, default);

            return new UserManagerResponse
			{
				IsSuccess = true,
				Message = "Die URL zum Zurücksetzen des Passworts wurde erfolgreich an die E-Mail-Adresse gesendet!"
            };
		}


		#endregion

		#region   ResetPassword
		public async Task<UserManagerResponse> ResetPasswordAsync(ResetPasswordModel model)
		{
			var user = await _userManger.FindByEmailAsync(model.Email);
			
			var decodedToken = WebEncoders.Base64UrlDecode(model.Token);
			string normalToken = Encoding.UTF8.GetString(decodedToken);

			var result = await _userManger.ResetPasswordAsync(user, normalToken, model.Password);

			if (result.Succeeded)
				return new UserManagerResponse
				{
					Message = "Password has been reset successfully!",
					IsSuccess = true,
				};

			return new UserManagerResponse
			{
				Message = "Something went wrong",
				IsSuccess = false,
				Errors = result.Errors.Select(e => e.Description),
			};
		}


		#endregion

		#region   EditProfile
		public async Task<EditAccountCustomResponse> EditProfile(EditProfileModel model)
		{
			var user = await _userManger.FindByIdAsync(model.Id);
			if (model.Email == null)
			{
				model.Email = user.Email;
				user.PhoneNumber = model.PhoneNumber;
				user.UserName = model.UserName;
				user.Salute = model.Salute;
				user.City = model.City;
				user.Street = model.Street;
				user.PostBox = model.PostBox;


			}
			//chek user exist
			if (user == null)
			{
				EditAccountCustomResponse custom = new EditAccountCustomResponse()
				{
					Code = "400",
					Message = "User Not Found",
					Status = "Faild",
					IsSuccess = false,
				};
				return (custom);
			}

			//check user name and Email 
			var userWithSameEmail = await _userManger.FindByEmailAsync(model.Email);
			if (userWithSameEmail != null && userWithSameEmail.Id != model.Id)
			{

				EditAccountCustomResponse custom = new EditAccountCustomResponse()
				{
					Code = "400",
					Message = "Email is Used ",
                    Status = "Faild",
					IsSuccess = false,
				};
				return (custom);
			}

				model.Email = user.Email;
				user.PhoneNumber = model.PhoneNumber;
				user.UserName = model.UserName;
				user.Salute = model.Salute;
				user.Street = model.Street;
				user.City = model.City;
				user.PostBox = model.PostBox;




			var result = await _userManger.UpdateAsync(user);

			if (result.Succeeded)
			{
				#region Token
				var claims = new[]
			  {
                new Claim("Id", user.Id),
                new Claim("UserName", user.UserName),
                new Claim("Email", model.Email),
                new Claim("PhoneNumber",$"{user.PhoneNumber}"),
                new Claim("Salute",$"{user.Salute}"),
                new Claim("City",$"{user.City}"),
                new Claim("Street",$"{user.Street}"),
                new Claim("PostBox",$"{user.PostBox}"),



                };


				var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

				var token = new JwtSecurityToken(
					issuer: _configuration["JWT:Issuer"],
					audience: _configuration["JWT:Audience"],
					 claims: claims,
					expires: DateTime.Now.AddDays(30),
					signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));

				string tokenAsString = new JwtSecurityTokenHandler().WriteToken(token);
				#endregion//Token

				EditAccountCustomResponse custom = new EditAccountCustomResponse()
				{
					Message = "Account Updated Successfully",
					Code = "200",
                    Status = "succeed",
					IsSuccess = true,
					Token = tokenAsString,


				};
				return (custom);
			}
			else
			{
				EditAccountCustomResponse custom = new EditAccountCustomResponse()
				{
					Code = "400",
					Message = "Something is wrong ",
                    Status = "Faild",
					IsSuccess = false,
				};
				return (custom);
			}


		}


		#endregion

		#region   EditPassword
		public async Task<UserManagerResponse> EditPassword(EditPassword model)
		{
			var user = await _userManger.FindByEmailAsync(model.Email);

			if (user == null)
				return new UserManagerResponse
				{
					IsSuccess = false,
					Message = "No user associated with email",
				};

			//Check old Password
			var oldpasword = await _userManger.CheckPasswordAsync(user, model.OldPaassword);
			if (!oldpasword)
				return new UserManagerResponse
				{
					Message = "Invalid Old password",
					IsSuccess = false,
				};
			//Check Password Confirmation
			if (model.NewPassword != model.ConfirmNewPassword)
				return new UserManagerResponse
				{
					IsSuccess = false,
					Message = "Password doesn't match its confirmation",
				};


			//Generate User Token
			var token = await _userManger.GeneratePasswordResetTokenAsync(user);

			//Edite Password

			var result = await _userManger.ResetPasswordAsync(user, token, model.NewPassword);
			if (result.Succeeded)
				return new UserManagerResponse
				{
					Message = "Password has been Edited successfully!",
					IsSuccess = true,
				};

			return new UserManagerResponse
			{
				Message = "Something went wrong",
				IsSuccess = false,
				Errors = result.Errors.Select(e => e.Description),
			};
		}


		#endregion

		#region GetAccount

		public async Task<UserModel> GetAccount(string id)
		{
			var data = await _userManger.FindByIdAsync(id);

			UserModel obj = new UserModel()
			{
				Id = data.Id,
				UserName = data.UserName,
				PhoneNumber = data.PhoneNumber,
				Email = data.Email,
				Salute =data.Salute,
				City=	data.City,
				Street=	data.Street,
				PostBox=	data.PostBox,






			};
			return obj;
		}

        public async Task<ApplicationUser> GetUserToEdit(string name)
        {
            var data = await _userManger.FindByEmailAsync(name);
       
            return (data);
        }

        #endregion








    }

}
