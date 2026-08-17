using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMover.Helper;
using System.Globalization;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using WangenPizza.Services;

namespace WangenPizza.Controllers
{
    [Authorize(Roles = "Admin")]

    public class DiscountCodeController : Controller
    {

        #region Ctor

        private readonly IMapper mapper;
        private readonly IDiscountCodeService DiscountCodeService;
        public DiscountCodeController(IMapper mapper , IDiscountCodeService DiscountCodeService)
        {
            this.mapper = mapper;
            this.DiscountCodeService = DiscountCodeService;
        }
        #endregion

        #region Index
        public async Task<IActionResult> Index()
        {
            var data = await DiscountCodeService.Get();
            var model = mapper.Map<IEnumerable<DiscountCodeDto>>(data);
            return View(model);
        }
        #endregion

        #region Wertgutscheine
        public async Task<IActionResult> Wertgutscheine()
        {
            var data = await DiscountCodeService.Get();
            var model = data
                .Where(IsWertgutscheinRow)
                .OrderByDescending(a => a.CreatedAtUtc == default ? a.ExpiryDate : a.CreatedAtUtc)
                .ThenByDescending(a => a.Id);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            var data = await DiscountCodeService.GetById(id);
            if (data == null)
            {
                TempData[key: "ErrorMessage"] = "error";
                return RedirectToAction(nameof(Wertgutscheine));
            }

            data.IsActive = false;
            DiscountCodeService.Update(data);
            TempData[key: "CompanyUpdated"] = "done";
            return RedirectToAction(nameof(Wertgutscheine));
        }

        private static bool IsWertgutscheinRow(DiscountCode code) =>
            code != null &&
            (code.IsWertgutschein ||
             (code.Name ?? "").StartsWith("WPV-", StringComparison.OrdinalIgnoreCase));
        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
        {
            var data = await DiscountCodeService.GetById(id);
            var model = mapper.Map<DiscountCodeDto>(data);
            return View(model);
        }
        #endregion

        #region Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new DiscountCodeDto
            {
                IsActive = true,
                Note = "Frühlings-Aktion 2026"
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create(DiscountCodeDto dto)
        {
            try
            {
                var expiryDate = DateTime.ParseExact(dto.ExpiryDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                var data = mapper.Map<DiscountCode>(dto);
                data.ExpiryDate = expiryDate;
                data.IsActive = dto.IsActive;
                data.CreatedAtUtc = DateTime.UtcNow;
                data.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();

                await DiscountCodeService.Create(data);

                if (data != null)
                {
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                TempData[key: "ErrorMessage"] = "error";
                return View();

            }
            TempData[key: "ErrorMessage"] = "error";
            return View();


        }
        #endregion

        #region Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await DiscountCodeService.GetById(id);
            var model = mapper.Map<DiscountCodeDto>(data);
            model.ExpiryDate = data.ExpiryDate.ToString("dd.MM.yyyy");
            return View(model);
        }
        [HttpPost]
        public async Task<IActionResult> Edit(DiscountCodeDto dto)
        {
            try
            {
                var existing = await DiscountCodeService.GetById(dto.Id);
                if (existing == null)
                {
                    TempData[key: "ErrorMessage"] = "error";
                    return View();
                }

                var expiryDate = DateTime.ParseExact(dto.ExpiryDate, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                var data = mapper.Map<DiscountCode>(dto);
                data.ExpiryDate = expiryDate;
                data.IsWertgutschein = existing.IsWertgutschein;
                data.IsActive = dto.IsActive;
                data.CreatedAtUtc = existing.CreatedAtUtc;
                data.OriginalValueChf = existing.OriginalValueChf;
                data.Note = string.IsNullOrWhiteSpace(dto.Note) ? null : dto.Note.Trim();
                DiscountCodeService.Update(data);

                if (data != null)
                {
                    TempData[key: "CompanyUpdated"] = "done";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception)
            {
                TempData[key: "ErrorMessage"] = "error";
                return View();

            }
            TempData[key: "ErrorMessage"] = "error";
            return View();


        }
        #endregion

        #region Delete


        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var data = await DiscountCodeService.GetById(id);
                DiscountCodeService.Delete(data);

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
    }
}
