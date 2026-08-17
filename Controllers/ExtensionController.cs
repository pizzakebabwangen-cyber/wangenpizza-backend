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

    public class ExtensionController : Controller
    {

        #region Ctor

        private readonly IMapper mapper;
        private readonly IExtensionService ExtensionService;
        private readonly ICategoryService categoryService;
        private readonly ISubCategoryService subcategoryService;

        public ExtensionController(IMapper mapper , IExtensionService ExtensionService,ICategoryService categoryService, ISubCategoryService subcategoryService)
        {
            this.mapper = mapper;
            this.ExtensionService = ExtensionService;
            this.categoryService = categoryService;
            this.subcategoryService = subcategoryService;
        }
        #endregion

        #region Index
        public async Task<IActionResult> Index()
        {
            var data = await ExtensionService.Get();
            var model = mapper.Map<IEnumerable<ExtensionDto>>(data);
            return View(model);
        }
        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
        {
            var data = await ExtensionService.GetById(id);
            var model = mapper.Map<ExtensionDto>(data);
            return View(model);
        }
        #endregion

        #region Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var Categories = await categoryService.Get();
            var CategoryModel = mapper.Map<IEnumerable<CategoryDto>>(Categories);
            ViewBag.CategoryList = CategoryModel;

            return View(new ExtensionDto
            {
                Name = "",
                Kind = "MainExtension",
                Price = 0,
                DisplayOrder = 0,
                CategoryId = null
            });
        }
        [HttpPost]
        public async Task<IActionResult> Create(ExtensionDto dto)
        {
            try
            {
                var data = mapper.Map<Extension>(dto);
                await ExtensionService.Create(data);

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

            var data = await ExtensionService.GetById(id);
            var model = mapper.Map<ExtensionDto>(data);
            var Categories = await categoryService.Get();
            var CategoryModel = mapper.Map<IEnumerable<CategoryDto>>(Categories);
            ViewBag.CategoryList = CategoryModel;

            return View(model);
        }
        [HttpPost]
        public IActionResult Edit(ExtensionDto dto)
        {
            try
            {
               
                var data = mapper.Map<Extension>(dto);
                ExtensionService.Update(data);

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
                var data = await ExtensionService.GetById(id);
                ExtensionService.Delete(data);

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
