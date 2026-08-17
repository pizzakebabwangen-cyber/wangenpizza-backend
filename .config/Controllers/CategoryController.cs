using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMover.Helper;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;
using WangenPizza.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace WangenPizza.Controllers
{
    [Authorize(Roles = "Admin")]

    public class CategoryController : Controller
    {

        #region Ctor

        private readonly IMapper mapper;
        private readonly ICategoryService categoryService;
        public CategoryController(IMapper mapper , ICategoryService categoryService)
        {
            this.mapper = mapper;
            this.categoryService = categoryService;
        }
        #endregion

        #region Index
        public async Task<IActionResult> Index()
        {
            var data = await categoryService.Get();
            var model = mapper.Map<IEnumerable<CategoryDto>>(data);
            return View(model);
        }
        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
        {
            var data = await categoryService.GetById(id);
            var model = mapper.Map<CategoryDto>(data);
            return View(model);
        }
        #endregion

        #region Create
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(CategoryDto dto)
        {
            try
            {
                dto.PhotoName = FileUploader.UploadFile("Images", dto.Photo);
                var data = mapper.Map<Category>(dto);
                await categoryService.Create(data);

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
            var data = await categoryService.GetById(id);
            var model = mapper.Map<CategoryDto>(data);
            return View(model);
        }
        [HttpPost]
        public IActionResult Edit(CategoryDto dto)
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
                var data = mapper.Map<Category>(dto);
                categoryService.Update(data);

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
                var data = await categoryService.GetById(id);
                categoryService.Delete(data);

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
