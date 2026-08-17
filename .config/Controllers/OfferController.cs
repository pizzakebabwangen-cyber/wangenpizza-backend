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

    public class OfferController : Controller
    {
        #region Ctor

        private readonly IMapper mapper;
        private readonly IOfferService OfferService;
        private readonly IProductService productService;
        private readonly ISubCategoryService subCategoryService;

        public OfferController(IMapper mapper, IOfferService OfferService ,IProductService productService, ISubCategoryService subCategoryService)
        {
            this.mapper = mapper;
            this.OfferService = OfferService;
            this.productService = productService;
            this.subCategoryService = subCategoryService;
        }
        #endregion

        #region Index
        public async Task<IActionResult> Index()
        {
            var data = await productService.GetOffers();
            var model = mapper.Map<IEnumerable<ProductDto>>(data);
            return View(model);
        }
        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
        {
            var data = await OfferService.GetById(id);
            var model = mapper.Map<OfferDto>(data);
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
                await OfferService.Create(data);

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
            var data = await OfferService.GetById(id);
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
                OfferService.Update(data);

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
                var data = await OfferService.GetById(id);
                OfferService.Delete(data);

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
