using WangenPizza.Dtos;
using WangenPizza.Helper;
using WangenPizza.Models;

namespace WangenPizza.Interfaces
{
    public interface IEmailHtmlTemplateService
    {
        public string GetContactUsTemplate(string email);
        public string GetResetPasswordTemplate(string email, string url);
        public string GetContactUsAdminTemplate(ContactDto dto);
        public string GetReservationTemplate(Reservation dto);
        public string GetOrderTemplate(Order dto);
        Task<string> GetActvateEmailTemplate(ApplicationUser user);
        public string GetReservationTemplate(ReservationDto dto, string verificationUrl);
        public string GetOrderTemplate(OrderDto dto, string verificationUrl);



    }
}
