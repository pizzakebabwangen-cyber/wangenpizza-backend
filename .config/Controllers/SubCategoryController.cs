using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Controllers
{
    [Authorize(Roles = "Admin")]

    public class SubCategoryController : Controller
    {
        #region Ctor

        private readonly IMapper mapper;
        private readonly ISubCategoryService subCategoryService;
        private readonly ICategoryService categoryService;

        public SubCategoryController(IMapper mapper, ISubCategoryService subCategoryService , ICategoryService categoryService)
        {
            this.mapper = mapper;
            this.subCategoryService = subCategoryService;
            this.categoryService = categoryService;
        }
        #endregion

        #region Index
        public async Task<IActionResult> Index()
        {
            var data = await subCategoryService.Get();
            var model = mapper.Map<IEnumerable<SubCategoryDto>>(data);
            return View(model);
        }
        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
        {
            var data = await subCategoryService.GetById(id);
            var model = mapper.Map<SubCategoryDto>(data);
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
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(SubCategoryDto dto)
        {
            try
            {
                var data = mapper.Map<SubCategory>(dto);
                await subCategoryService.Create(data);

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
            var data = await subCategoryService.GetById(id);
            var model = mapper.Map<SubCategoryDto>(data);
              var Categories = await categoryService.Get();
            var CategoryModel = mapper.Map<IEnumerable<CategoryDto>>(Categories);
            ViewBag.CategoryList = CategoryModel;
            return View(model);
        }
        [HttpPost]
        public IActionResult Edit(SubCategoryDto dto)
        {
            try
            {


                var data = mapper.Map<SubCategory>(dto);
                subCategoryService.Update(data);

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
                var data = await subCategoryService.GetById(id);
                subCategoryService.Delete(data);

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
