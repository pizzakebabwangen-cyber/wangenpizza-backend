using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using MimeKit;

using System.Globalization;
using System.Text;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Services
{
    public class EmailHtmlTemplateService : IEmailHtmlTemplateService
    {
        #region Ctor
      
        private readonly IConfiguration _configuration;
        private readonly IWebHostEnvironment _environment;
        private readonly UserManager<ApplicationUser> userManager;

        public EmailHtmlTemplateService(IConfiguration configuration, IWebHostEnvironment environment ,UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _environment = environment;
            this.userManager = userManager;
        }


        #endregion



        #region Contact Us Template
        public string GetContactUsTemplate(string name)
        {

            var pathToFile = $"{_environment.WebRootPath}\\Templates\\ContactUs.html";
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
            {

                builder.HtmlBody = SourceReader.ReadToEnd();

            }
            string messageBody = string.Format(builder.HtmlBody, name);
            return messageBody;

        }
        #endregion



        #region Contact Us Admin Template
        public string GetContactUsAdminTemplate(ContactDto dto)
        {

            var pathToFile = $"{_environment.WebRootPath}\\Templates\\ContactUsAdmin.html";
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
            {

                builder.HtmlBody = SourceReader.ReadToEnd();

            }
            string messageBody = string.Format(builder.HtmlBody, dto.Name , dto.Phone , dto.Email , dto.Message);
            return messageBody;

        }
        #endregion

        #region GetReservationTemplate
        public string GetReservationTemplate(Reservation dto)
        {

            var pathToFile = $"{_environment.WebRootPath}\\Templates\\Reservation.html";
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
            {

                builder.HtmlBody = SourceReader.ReadToEnd();

            }
            string messageBody = string.Format(builder.HtmlBody, dto.Salute, dto.Name,dto.RDate, dto.RTime);
            return messageBody;

        }
        #endregion

        #region GetOrderTemplate
        public string GetOrderTemplate(Order dto)
        {

            var pathToFile = $"{_environment.WebRootPath}\\Templates\\Order.html";
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
            {

                builder.HtmlBody = SourceReader.ReadToEnd();

            }
            string messageBody = string.Format(builder.HtmlBody, dto.Salute, dto.Name);
            return messageBody;

        }
        #endregion

        #region Get ResetPassword Us Template
        public string GetResetPasswordTemplate(string name, string url)
        {

            var pathToFile = $"{_environment.WebRootPath}\\Templates\\ResetPassword.html";
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
            {

                builder.HtmlBody = SourceReader.ReadToEnd();

            }
            string messageBody = string.Format(builder.HtmlBody, name , url);
            return messageBody;

        }
        #endregion

        #region Actvate Email Template
        public async Task<string> GetActvateEmailTemplate(ApplicationUser user)
        {
            var moveOfferEmailImagesConfig = _configuration.GetSection("EmailImages");

            var headerImage = moveOfferEmailImagesConfig["companyLogo"];
            var confirmEmailToken = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedEmailToken = Encoding.UTF8.GetBytes(confirmEmailToken);
            var validEmailToken = WebEncoders.Base64UrlEncode(encodedEmailToken);
            string url = $"{_configuration["AppUrl"]}/Account/ConfirmEmail?userid={user.Id}&token={validEmailToken}";


            var pathToFile = $"{_environment.WebRootPath}\\Templates\\ActivateEmail.html";
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
            {

                builder.HtmlBody = SourceReader.ReadToEnd();

            }
            string messageBody = string.Format(builder.HtmlBody, user.Email, url);
            return messageBody;

        }
        #endregion


        #region GetReservationTemplate
        public string GetReservationTemplate(ReservationDto dto, string verificationUrl)
        {

            var pathToFile = $"{_environment.WebRootPath}\\Templates\\ReservasionEmail.html";
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
            {

                builder.HtmlBody = SourceReader.ReadToEnd();

            }
            string messageBody = string.Format(builder.HtmlBody, dto.Name, verificationUrl);
            return messageBody;

        }
        #endregion

        #region GetOrderTemplate
        public string GetOrderTemplate(OrderDto dto, string verificationUrl)
        {

            var pathToFile = $"{_environment.WebRootPath}\\Templates\\OrderEmail.html";
            var builder = new BodyBuilder();
            using (StreamReader SourceReader = System.IO.File.OpenText(pathToFile))
            {

                builder.HtmlBody = SourceReader.ReadToEnd();

            }
            string messageBody = string.Format(builder.HtmlBody, dto.Name, verificationUrl);
            return messageBody;

        }

     
        #endregion



    }

}
