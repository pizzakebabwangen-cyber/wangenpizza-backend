using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using WangenPizza.Dtos;
using WangenPizza.Helper;
using WangenPizza.Helper.Response;
using WangenPizza.Interfaces;
using WangenPizza.Services;

namespace WangenPizza.Api_s.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        #region Ctor

        private readonly IMapper mapper;
        private readonly IHubContext<NotificationHub> hubContext;
        private readonly ICompanyService companyService;

        public CompanyController(IMapper mapper, IHubContext<NotificationHub> hubContext, ICompanyService companyService)
        {
            this.mapper = mapper;
            this.hubContext = hubContext;
            this.companyService = companyService;
        }
        #endregion

        #region GetCompanyData
        [HttpGet("GetCompanyData")]
        public async Task<IActionResult> GetCompanyData()
        {
            try
            {
                var data = await companyService.GetById(1);
                if (data != null)
                {
                    CompanyResponse response = new CompanyResponse
                    {
                        Code = "200",
                        Status = "Success",
                        Message = "Company Data Returned successfully !",
                        Data = data
                    };
                    //await hubContext.Clients.All.SendAsync("ReceiveNotification", $"Neue Bestellung testttttttttttttttt ", "order");

                    return Ok(response);
                }
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });
            }
            catch (Exception)
            {
                return StatusCode(400, new CustomResponse { Code = "400", Message = "Error" });

            }

        }
        #endregion

        #region check available
        [HttpGet("check-availability")]
        public async Task<ActionResult<RestaurantAvailabilityDto>> CheckAvailability()
        {
            var company = await companyService.GetById(1); // افتراضيا ID 1

            // تحديد توقيت المطعم (مثلاً مصر)
            var restaurantTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, restaurantTimeZone);

            // 1- Check Pause period
            if (company.Pausefrom.HasValue && company.Pausetill.HasValue)
            {
                var pauseFrom = TimeZoneInfo.ConvertTime(company.Pausefrom.Value, restaurantTimeZone);
                var pauseTill = TimeZoneInfo.ConvertTime(company.Pausetill.Value, restaurantTimeZone);

                if (now >= pauseFrom && now <= pauseTill)
                {
                    return Ok(new RestaurantAvailabilityDto
                    {
                        IsAvailable = false,
                        Message = $"Das Restaurant ist im Zeitraum von {pauseFrom:dd.MM.yyyy} bis {pauseTill:dd.MM.yyyy} geschlossen.\nVielen Dank für Ihr Verständnis."
                    });
                }
            }

            // 2- Check daily working times
            var dayOfWeek = now.DayOfWeek;
            string? from1 = null, till1 = null, from2 = null, till2 = null;

            switch (dayOfWeek)
            {
                case DayOfWeek.Monday:
                    from1 = company.MondayFrom1; till1 = company.MondayTill1;
                    from2 = company.MondayFrom2; till2 = company.MondayTill2;
                    break;
                case DayOfWeek.Tuesday:
                    from1 = company.TuesdayFrom1; till1 = company.TuesdayTill1;
                    from2 = company.TuesdayFrom2; till2 = company.TuesdayTill2;
                    break;
                case DayOfWeek.Wednesday:
                    from1 = company.WednesdayFrom1; till1 = company.WednesdayTill1;
                    from2 = company.WednesdayFrom2; till2 = company.WednesdayTill2;
                    break;
                case DayOfWeek.Thursday:
                    from1 = company.ThursdayFrom1; till1 = company.ThursdayTill1;
                    from2 = company.ThursdayFrom2; till2 = company.ThursdayTill2;
                    break;
                case DayOfWeek.Friday:
                    from1 = company.FridayFrom1; till1 = company.FridayTill1;
                    from2 = company.FridayFrom2; till2 = company.FridayTill2;
                    break;
                case DayOfWeek.Saturday:
                    from1 = company.SaturdayFrom1; till1 = company.SaturdayTill1;
                    from2 = company.SaturdayFrom2; till2 = company.SaturdayTill2;
                    break;
                case DayOfWeek.Sunday:
                    from1 = company.SundayFrom1; till1 = company.SundayTill1;
                    from2 = company.SundayFrom2; till2 = company.SundayTill2;
                    break;
            }

            // تحويل للنصوص إلى أوقات
            bool isInTimeRange = false;

            if (TimeSpan.TryParse(from1, out var f1) && TimeSpan.TryParse(till1, out var t1))
                isInTimeRange |= now.TimeOfDay >= f1 && now.TimeOfDay <= t1;

            if (TimeSpan.TryParse(from2, out var f2) && TimeSpan.TryParse(till2, out var t2))
                isInTimeRange |= now.TimeOfDay >= f2 && now.TimeOfDay <= t2;

            if (!isInTimeRange)
            {
                string allowedTimes = "";
                if (!string.IsNullOrEmpty(from1) && !string.IsNullOrEmpty(till1))
                    allowedTimes += $"von: {from1} bis: {till1}";
                if (!string.IsNullOrEmpty(from2) && !string.IsNullOrEmpty(till2))
                    allowedTimes += allowedTimes != "" ? $" & von: {from2} bis: {till2}" : $"von: {from2} bis: {till2}";

                return Ok(new RestaurantAvailabilityDto
                {
                    IsAvailable = false,
                    Message = $"Leider sind derzeit keine Bestellungen möglich. Erlaubte Bestellzeiten heute: {allowedTimes}. Bitte versuchen Sie es später erneut. \nVielen Dank für Ihr Verständnis."
                });
            }

            return Ok(new RestaurantAvailabilityDto
            {
                IsAvailable = true,
                Message = "The restaurant is available for orders."
            });
        }
        #endregion


        #region available-order-times
        [HttpGet("available-order-times")]
        public async Task<IActionResult> GetAvailableOrderTimes()
        {
            var company = await companyService.GetById(1);

            // توقيت المطعم
            var restaurantTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
            var now = TimeZoneInfo.ConvertTime(DateTime.UtcNow, restaurantTimeZone);

            // start time (now + 45 min)
            var startTime = now.AddMinutes(45);

            // today schedule
            string? from1 = null, till1 = null, from2 = null, till2 = null;
            switch (now.DayOfWeek)
            {
                case DayOfWeek.Monday: from1 = company.MondayFrom1; till1 = company.MondayTill1; from2 = company.MondayFrom2; till2 = company.MondayTill2; break;
                case DayOfWeek.Tuesday: from1 = company.TuesdayFrom1; till1 = company.TuesdayTill1; from2 = company.TuesdayFrom2; till2 = company.TuesdayTill2; break;
                case DayOfWeek.Wednesday: from1 = company.WednesdayFrom1; till1 = company.WednesdayTill1; from2 = company.WednesdayFrom2; till2 = company.WednesdayTill2; break;
                case DayOfWeek.Thursday: from1 = company.ThursdayFrom1; till1 = company.ThursdayTill1; from2 = company.ThursdayFrom2; till2 = company.ThursdayTill2; break;
                case DayOfWeek.Friday: from1 = company.FridayFrom1; till1 = company.FridayTill1; from2 = company.FridayFrom2; till2 = company.FridayTill2; break;
                case DayOfWeek.Saturday: from1 = company.SaturdayFrom1; till1 = company.SaturdayTill1; from2 = company.SaturdayFrom2; till2 = company.SaturdayTill2; break;
                case DayOfWeek.Sunday: from1 = company.SundayFrom1; till1 = company.SundayTill1; from2 = company.SundayFrom2; till2 = company.SundayTill2; break;
            }

            var timeOptions = new List<OrderTimeOptionDto>();
            bool added = AddTimeRange(timeOptions, from1, till1, startTime, restaurantTimeZone);
            added |= AddTimeRange(timeOptions, from2, till2, startTime, restaurantTimeZone);

            if (!added)
            {
                return Ok(new
                {
                    isVisible = false,
                    message = "Zeit nicht erlaubt",
                    options = new List<OrderTimeOptionDto>()
                });
            }

            return Ok(new
            {
                isVisible = true,
                message = "",
                options = timeOptions
            });
        }

        private bool AddTimeRange(List<OrderTimeOptionDto> timeOptions, string? f, string? t, DateTime startTime, TimeZoneInfo tz)
        {
            if (string.IsNullOrEmpty(f) || string.IsNullOrEmpty(t)) return false;
            if (!TimeSpan.TryParse(f, out var from)) return false;
            if (!TimeSpan.TryParse(t, out var till)) return false;

            // استخدام توقيت اليوم في المطعم
            var today = TimeZoneInfo.ConvertTime(DateTime.UtcNow, tz).Date;

            var lastTime = today.Add(till);
            var forbiddenTime = lastTime.AddMinutes(-45);

            if (startTime > forbiddenTime) return false;

            var current = today.Add(from);
            if (current < startTime) current = startTime;

            bool hasValid = false;
            while (current <= lastTime)
            {
                if (current == forbiddenTime)
                {
                    current = current.AddMinutes(15);
                    continue;
                }

                timeOptions.Add(new OrderTimeOptionDto
                {
                    Value = current.ToString("HH:mm")
                });

                hasValid = true;
                current = current.AddMinutes(15);
            }

            return hasValid;
        }

        public class OrderTimeOptionDto
        {
            public string Value { get; set; } = string.Empty;
        }
        #endregion


    }

}
