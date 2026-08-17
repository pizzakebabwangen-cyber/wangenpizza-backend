using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using WangenPizza.Interfaces;
using WangenPizza.Dtos;
using WangenPizza.Models;
using Microsoft.AspNetCore.Authorization;
using System.Globalization;

namespace WangenPizza.Controllers
{
    [Authorize(Roles = "Admin")]

    public class CompanyController : Controller
    {
        #region Ctor
        private readonly IMapper mapper;
        private readonly ICompanyService CompanyDataService;
        private readonly string CompanyDataDataForm = "CreateCompanyDataData";

        public CompanyController(IMapper mapper, ICompanyService CompanyDataService)
        {
            this.mapper = mapper;
            this.CompanyDataService = CompanyDataService;
        }
        #endregion

        #region Index
        public async Task<IActionResult> Index()
        {
            var data = await CompanyDataService.Get();
            var model = mapper.Map<IEnumerable<CompanyDataDto>>(data);
            return View(model);
        }
        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
        {
            var data = await CompanyDataService.GetById(id);
            var model = mapper.Map<CompanyDataDto>(data);
            return View(model);
        }
        #endregion

        #region Create
        [HttpGet]
        public IActionResult CreateCompanyData()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> CreateCompanyData(CompanyDataDto dto)
        {
            try
            {
                var data = mapper.Map<CompanyData>(dto);
                await CompanyDataService.Create(data);

                if (data != null)
                {
                    return RedirectToAction("Index" , "Home");
                }
            }
            catch (Exception)
            {
                return View();

            }
            return View();


        }
        #endregion

        #region Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await CompanyDataService.GetById(id);
            var model = mapper.Map<CompanyDataDto>(data);
            return View(model);
        }
        [HttpPost]
        public IActionResult Edit(CompanyDataDto dto)
        {
            try
            {
                var data = mapper.Map<CompanyData>(dto);
                CompanyDataService.Update(data);

                if (data != null)
                {
                    return RedirectToAction("Index", "Home");
                }
            }
            catch (Exception)
            {
                return View();

            }
            return View();


        }
        #endregion

        #region ShortBreak
        [HttpGet]
        public async Task<IActionResult> ShortBreak(int id)
        {
            var data = await CompanyDataService.GetById(id);
            if (data == null) return NotFound();

            var model = new CompanyDataDto
            {
                Id = data.Id,
                Pausefrom = data.Pausefrom.HasValue
            ? data.Pausefrom.Value.ToString("dd.MM.yyyy")
            : string.Empty,

                Pausetill = data.Pausetill.HasValue
            ? data.Pausetill.Value.ToString("dd.MM.yyyy")
            : string.Empty,
                Pausetyp = data.Pausetyp,
                MondayFrom1 = data.MondayFrom1,
                MondayTill1 = data.MondayTill1,
                MondayFrom2 = data.MondayFrom2,
                MondayTill2 = data.MondayTill2,
                TuesdayFrom1 = data.TuesdayFrom1,
                TuesdayTill1 = data.TuesdayTill1,
                TuesdayFrom2 = data.TuesdayFrom2,
                TuesdayTill2 = data.TuesdayTill2,
                WednesdayFrom1 = data.WednesdayFrom1,
                WednesdayTill1 = data.WednesdayTill1,
                WednesdayFrom2 = data.WednesdayFrom2,
                WednesdayTill2 = data.WednesdayTill2,
                ThursdayFrom1 = data.ThursdayFrom1,
                ThursdayTill1 = data.ThursdayTill1,
                ThursdayFrom2 = data.ThursdayFrom2,
                ThursdayTill2 = data.ThursdayTill2,
                FridayFrom1 = data.FridayFrom1,
                FridayTill1 = data.FridayTill1,
                FridayFrom2 = data.FridayFrom2,
                FridayTill2 = data.FridayTill2,
                SaturdayFrom1 = data.SaturdayFrom1,
                SaturdayTill1 = data.SaturdayTill1,
                SaturdayFrom2 = data.SaturdayFrom2,
                SaturdayTill2 = data.SaturdayTill2,
                SundayFrom1 = data.SundayFrom1,
                SundayTill1 = data.SundayTill1,
                SundayFrom2 = data.SundayFrom2,
                SundayTill2 = data.SundayTill2
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ShortBreak(CompanyDataDto dto)
        {
            try
            {
                var data = await CompanyDataService.GetById(1);
                if (data == null) return NotFound();

                var culture = new System.Globalization.CultureInfo("de-DE");

                // تحويل Pausefrom من نص إلى DateTime
                if (!string.IsNullOrWhiteSpace(dto.Pausefrom) &&
                    DateTime.TryParseExact(dto.Pausefrom, "dd.MM.yyyy", culture, DateTimeStyles.None, out var pauseFromDate))
                {
                    data.Pausefrom = pauseFromDate;
                }
                else
                {
                    data.Pausefrom = null;
                }

                // تحويل Pausetill من نص إلى DateTime
                if (!string.IsNullOrWhiteSpace(dto.Pausetill) &&
                    DateTime.TryParseExact(dto.Pausetill, "dd.MM.yyyy", culture, DateTimeStyles.None, out var pauseTillDate))
                {
                    data.Pausetill = pauseTillDate;
                }
                else
                {
                    data.Pausetill = null;
                }

                data.Pausetyp = dto.Pausetyp;

                // باقي الأيام
                data.MondayFrom1 = dto.MondayFrom1; data.MondayTill1 = dto.MondayTill1;
                data.MondayFrom2 = dto.MondayFrom2; data.MondayTill2 = dto.MondayTill2;

                data.TuesdayFrom1 = dto.TuesdayFrom1; data.TuesdayTill1 = dto.TuesdayTill1;
                data.TuesdayFrom2 = dto.TuesdayFrom2; data.TuesdayTill2 = dto.TuesdayTill2;

                data.WednesdayFrom1 = dto.WednesdayFrom1; data.WednesdayTill1 = dto.WednesdayTill1;
                data.WednesdayFrom2 = dto.WednesdayFrom2; data.WednesdayTill2 = dto.WednesdayTill2;

                data.ThursdayFrom1 = dto.ThursdayFrom1; data.ThursdayTill1 = dto.ThursdayTill1;
                data.ThursdayFrom2 = dto.ThursdayFrom2; data.ThursdayTill2 = dto.ThursdayTill2;

                data.FridayFrom1 = dto.FridayFrom1; data.FridayTill1 = dto.FridayTill1;
                data.FridayFrom2 = dto.FridayFrom2; data.FridayTill2 = dto.FridayTill2;

                data.SaturdayFrom1 = dto.SaturdayFrom1; data.SaturdayTill1 = dto.SaturdayTill1;
                data.SaturdayFrom2 = dto.SaturdayFrom2; data.SaturdayTill2 = dto.SaturdayTill2;

                data.SundayFrom1 = dto.SundayFrom1; data.SundayTill1 = dto.SundayTill1;
                data.SundayFrom2 = dto.SundayFrom2; data.SundayTill2 = dto.SundayTill2;

                await CompanyDataService.UpdateWithdays(data);

                TempData["SuccessTimeMessage"] = "Die Öffnungszeiten wurden erfolgreich aktualisiert!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Fehler beim Speichern: " + ex.Message);
                return View(dto);
            }
        }

        #endregion
    }
}
