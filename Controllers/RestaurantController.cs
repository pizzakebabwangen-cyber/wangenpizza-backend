using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMover.Helper;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RestaurantController : Controller
    {
        private readonly IMapper mapper;
        private readonly IRestaurantService restaurantService;

        public RestaurantController(IMapper mapper, IRestaurantService restaurantService)
        {
            this.mapper = mapper;
            this.restaurantService = restaurantService;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Settings = mapper.Map<RestaurantSettingsDto>(await restaurantService.GetSettingsAsync());
            var images = await restaurantService.GetImagesAsync();
            var model = mapper.Map<IEnumerable<RestaurantImageDto>>(images);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Settings()
        {
            var model = mapper.Map<RestaurantSettingsDto>(await restaurantService.GetSettingsAsync());
            return View(model);
        }

        [HttpPost]
        [RequestFormLimits(MultipartBodyLengthLimit = 104857600)]
        [RequestSizeLimit(104857600)]
        public async Task<IActionResult> Settings(RestaurantSettingsDto dto)
        {
            try
            {
                var existing = await restaurantService.GetSettingsAsync();

                if (dto.Video != null && dto.Video.Length > 0)
                {
                    var fileName = FileUploader.UploadFile("Videos", dto.Video);
                    if (fileName.StartsWith("Error:", StringComparison.OrdinalIgnoreCase))
                    {
                        TempData["ErrorMessage"] = "error";
                        return View(dto);
                    }

                    dto.VideoUrl = $"{Request.Scheme}://{Request.Host}/Videos/{fileName}";
                }
                else if (string.IsNullOrWhiteSpace(dto.VideoUrl))
                {
                    dto.VideoUrl = existing.VideoUrl;
                }

                var data = mapper.Map<RestaurantSettings>(dto);
                await restaurantService.UpdateSettingsAsync(data);
                TempData["CompanyUpdated"] = "done";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "error";
                return View(dto);
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(RestaurantImageDto dto)
        {
            try
            {
                if (dto.Photo == null)
                {
                    TempData["ErrorMessage"] = "error";
                    return View(dto);
                }

                dto.PhotoName = FileUploader.UploadFile("Images", dto.Photo);
                var data = mapper.Map<RestaurantImage>(dto);
                await restaurantService.CreateImageAsync(data);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "error";
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await restaurantService.GetImageByIdAsync(id);
            if (data == null)
            {
                return RedirectToAction(nameof(Index));
            }

            return View(mapper.Map<RestaurantImageDto>(data));
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RestaurantImageDto dto)
        {
            try
            {
                if (dto.Photo != null)
                {
                    dto.PhotoName = FileUploader.UploadFile("Images", dto.Photo);
                }

                var data = mapper.Map<RestaurantImage>(dto);
                restaurantService.UpdateImage(data);
                TempData["CompanyUpdated"] = "done";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "error";
                return View(dto);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            var data = await restaurantService.GetImageByIdAsync(id);
            if (data != null)
            {
                restaurantService.DeleteImage(data);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
