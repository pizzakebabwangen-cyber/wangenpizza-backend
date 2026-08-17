using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMover.Helper;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using WangenPizza.Services;

namespace WangenPizza.Controllers
{
    [Authorize(Roles = "Admin")]

    public class TodayBonusController : Controller
    {
        #region Ctor

        private readonly IMapper mapper;
        private readonly ITodayBonusService TodayBonusService;
        private readonly IProductService productService;
        private readonly ISubCategoryService subCategoryService;

        public TodayBonusController(IMapper mapper, ITodayBonusService TodayBonusService , IProductService productService, ISubCategoryService subCategoryService)
        {
            this.mapper = mapper;
            this.TodayBonusService = TodayBonusService;
            this.productService = productService;
            this.subCategoryService = subCategoryService;
        }
        #endregion

        #region Index
        public async Task<IActionResult> Index()
        {
            var data = await productService.GetTodayBonus();
            var model = mapper.Map<IEnumerable<ProductDto>>(data);
            return View(model);
        }
        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
        {
            var data = await TodayBonusService.GetById(id);
            var model = mapper.Map<ProductDto>(data);
            return View(model);
        }
        #endregion

        #region Create
        [HttpGet]
        public  IActionResult Create()
        {

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductDto dto)
        {
            try
            {
                dto.PhotoName = FileUploader.UploadFile("Images", dto.Photo);
                var data = mapper.Map<Product>(dto);
                await TodayBonusService.Create(data);

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
            var data = await TodayBonusService.GetById(id);
            var model = mapper.Map<ProductDto>(data);

            return View(model);
        }
        [HttpPost]
        public IActionResult Edit(ProductDto dto)
        {
            try
            {

                if (dto.Photo == null)
                {
                    dto.PhotoName = dto.PhotoName;

                }
                else
                {
                    dto.PhotoName = FileUploader.UploadFile("Images", dto.Photo);

                }
                var data = mapper.Map<Product>(dto);
                TodayBonusService.Update(data);

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
                var data = await TodayBonusService.GetById(id);
                TodayBonusService.Delete(data);

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
