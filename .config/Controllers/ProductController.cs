using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using QuickMover.Helper;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using WangenPizza.Services;

namespace WangenPizza.Controllers
{
    [Authorize(Roles = "Admin")]

    public class ProductController : Controller
    {
        #region Ctor

        private readonly ILogger<ProductController> logger;
        private readonly IMapper mapper;
        private readonly IProductService ProductService;
        private readonly ISubCategoryService subCategoryService;

        public ProductController(ILogger<ProductController> logger, IMapper mapper, IProductService ProductService , ISubCategoryService subCategoryService)
        {
            this.logger = logger;
            this.mapper = mapper;
            this.ProductService = ProductService;
            this.subCategoryService = subCategoryService;
        }
        #endregion

        #region Index
        public async Task<IActionResult> Index()
        {
            try
            {
                var data = await ProductService.Get();
                var model = mapper.Map<IEnumerable<ProductDto>>(data);
                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Product/Index: Produkte konnten nicht geladen werden.");
                TempData["ProductListError"] =
                    "Produktliste konnte nicht geladen werden (Datenbank oder Mapping). Details in den Server-Logs.";
                return View(Array.Empty<ProductDto>());
            }
        }
        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
        {
            var data = await ProductService.GetById(id);
            var model = mapper.Map<ProductDto>(data);
            return View(model);
        }
        #endregion

        #region Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var SubCategories = await subCategoryService.Get();
            var SubCategoryModel = mapper.Map<IEnumerable<SubCategoryDto>>(SubCategories);
            ViewBag.SubCategoryList = SubCategoryModel;
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(ProductDto dto)
        {
            try
            {
                dto.PhotoName = FileUploader.UploadFile("Images", dto.Photo);
                var data = mapper.Map<Product>(dto);
                await ProductService.Create(data);

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
            var data = await ProductService.GetById(id);
            var model = mapper.Map<ProductDto>(data);
            var SubCategories = await subCategoryService.Get();
            var SubCategoryModel = mapper.Map<IEnumerable<SubCategoryDto>>(SubCategories);
            ViewBag.SubCategoryList = SubCategoryModel;
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
                ProductService.Update(data);

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


        [HttpGet]
        public IActionResult Delete(int id)
        {
            try
            {
                ProductService.Delete(id);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                // ممكن تسجل الخطأ هنا
                return View("Error");
            }
        }


        #endregion
    }
}
