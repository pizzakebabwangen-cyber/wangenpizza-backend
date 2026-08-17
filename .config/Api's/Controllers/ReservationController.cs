using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WangenPizza.Dtos;
using WangenPizza.Helper;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using WangenPizza.Services;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationController : ControllerBase
    {
        #region Ctor

        private readonly IMapper mapper;
        private readonly IReservationService ReservationService;
        private readonly IEmailHtmlTemplateService emailHtmlTemplateService;
        private readonly IMailService mailService;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IConfiguration configuration;
        private readonly ITempReservationService tempReservationService;

        public ReservationController(IConfiguration configuration,ITempReservationService tempReservationService, IHubContext<NotificationHub> hubContext, IMapper mapper, IReservationService ReservationService , IEmailHtmlTemplateService emailHtmlTemplateService , IMailService mailService)
        {
            this.configuration = configuration;
            this.tempReservationService = tempReservationService;
            this.mapper = mapper;
            this.ReservationService = ReservationService;
            this.emailHtmlTemplateService = emailHtmlTemplateService;
            this.mailService = mailService;
            _hubContext = hubContext;

        }
        #endregion

        //#region Create Reservation
        //[HttpPost("Reservation")]
        //public async Task<IActionResult> Create([FromBody] ReservationDto dto)
        //{
        //   try
        //    {
        //        var data = mapper.Map<Reservation>(dto);
        //        await ReservationService.Create(data);
        //        if (data != null)
        //        {
        //            CustomResponse response = new CustomResponse
        //            {
        //                Code = "200",
        //                Status = "Success",
        //                Message = "Reservation request created successfully !"
                       
                       
        //            };
        //            //string Adminbody =  emailHtmlTemplateService.GetContactUsAdminTemplate(dto);
        //            //var  EmailSection = configuration.GetSection("MailSettings");
        //            //string AdminEmail = EmailSection["Mail"];
        //            //MailRequest AdminMailRequest = new MailRequest
        //            //{
        //            //    ToEmail = AdminEmail,
        //            //    Subject = "Wangen",
        //            //    Body = Adminbody,

        //            //};
        //            //await mailService.SendEmailAsync(AdminMailRequest, default);
        //            string body = emailHtmlTemplateService.GetReservationTemplate(dto);
        //            MailRequest mailRequest = new MailRequest
        //            {
        //                ToEmail = dto.Email,
        //                Subject = "Wangen",
        //                Body = body,

        //            };
        //            await mailService.SendEmailAsync(mailRequest, default);
        //            await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Neue Bestellung {dto.RDate} {dto.RTime}  Name :{dto.Name}  ", "alert");

        //            return Ok(response);
        //        }
        //        return StatusCode(400,value:  new CustomResponse { Code = "400", Message = "Error" });
        //    }
        //    catch(Exception)
        //    {
        //        return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });

        //    }

        //}
        //#endregion

        [HttpPost("Reservation")]
        public async Task<IActionResult> Create([FromBody] ReservationDto dto)
        {
            try
            {
                var data = mapper.Map<Reservation>(dto);
                data.Verified = false; // Set verified to false initially

                // Generate a unique token
                var token = Guid.NewGuid().ToString();

                // Store the token and reservation data temporarily (e.g., in-memory cache or a temporary table)
                await tempReservationService.StoreReservationAsync(token, data);

                string verificationUrl = Url.Action("VerifyEmail", "Reservation", new { token = token }, Request.Scheme);
                string body = emailHtmlTemplateService.GetReservationTemplate(dto, verificationUrl);

                MailRequest mailRequest = new MailRequest
                {
                    ToEmail = dto.Email,
                    Subject = "Verify your reservation",
                    Body = body,
                };
                await mailService.SendEmailAsync(mailRequest, default);

                CustomResponse response = new CustomResponse
                {
                    Code = "200",
                    Status = "Success",
                    Message = "Verification email sent successfully! Please check your email to verify your reservation."
                };

                return Ok(response);
            }
            catch (Exception)
            {
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });
            }
        }





    }
}
