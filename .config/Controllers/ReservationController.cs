using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using QuickMover.Helper;
using WangenPizza.Dtos;
using WangenPizza.Helper;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using WangenPizza.Services;

namespace WangenPizza.Controllers
{
    

    public class ReservationController : Controller
    {
        private readonly IConfiguration configuration;

        #region Ctor

        private readonly IMapper mapper;
        private readonly IReservationService ReservationService;
        private readonly ITempReservationService tempReservationService;
        private readonly IEmailHtmlTemplateService emailHtmlTemplateService;
        private readonly IMailService mailService;
        private readonly IHubContext<NotificationHub> _hubContext;


        public ReservationController(IConfiguration configuration,IMapper mapper, IHubContext<NotificationHub> hubContext, IReservationService ReservationService, ITempReservationService tempReservationService , IEmailHtmlTemplateService emailHtmlTemplateService, IMailService mailService)
        {
            this.configuration = configuration;
            this.mapper = mapper;
            this.ReservationService = ReservationService;
            this.tempReservationService = tempReservationService;
            this.emailHtmlTemplateService = emailHtmlTemplateService;
            this.mailService = mailService;
            _hubContext = hubContext;

        }
        #endregion
        #region Index
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var data = await ReservationService.Get();
            var model = mapper.Map<IEnumerable<ReservationDto>>(data);
            return View(model);
        }
        #endregion

        #region Details
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Details(int id)
        {
            var data = await ReservationService.GetById(id);
            var model = mapper.Map<ReservationDto>(data);
            return View(model);
        }
        #endregion

        #region Delete

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var data = await ReservationService.GetById(id);
                ReservationService.Delete(data);

                if (data != null)
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                return View();

            }
            return View();


        }
        #endregion

        #region Verify Email


        public async Task<IActionResult> VerifyEmail(string token)
        {
            try
            {
                // Retrieve the reservation data using the token
                var reservation = await tempReservationService.GetReservationByTokenAsync(token);
                if (reservation != null)
                {
                    reservation.Verified = true;
                    await ReservationService.Create(reservation);

                    CustomResponse response = new CustomResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Reservation verified and created successfully!"
                    };
                   
                    string body = emailHtmlTemplateService.GetReservationTemplate(reservation);
                    MailRequest mailRequest = new MailRequest
                    {
                        ToEmail = reservation.Email,
                        Subject = "Wangen",
                        Body = body,

                    };
                    await mailService.SendEmailAsync(mailRequest, default);
                    await _hubContext.Clients.All.SendAsync("ReceiveNotification", $"Neue Bestellung {reservation.RDate} {reservation.RTime}  Name :{reservation.Name}  ", "alert");

                    return View();
                }

                return StatusCode(400, new CustomResponse { Code = "400", Message = "Invalid token or reservation not found." });
            }
            catch (Exception)
            {
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });
            }
        }
        #endregion
    }
}
