using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QuickMover.Helper;
using WangenPizza.Context;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Controllers
{
    [Authorize(Roles = "Admin")]

    public class EmailTextController : Controller
    {

        #region Ctor

        private readonly IMapper mapper;
        private readonly IEmailTextService EmailTextService;
        private readonly ApplicationDbContext context;

        public EmailTextController(IMapper mapper , IEmailTextService EmailTextService , ApplicationDbContext context)
        {
            this.mapper = mapper;
            this.EmailTextService = EmailTextService;
            this.context = context;
        }
        #endregion

        #region Index
        public async Task<IActionResult> Index()
        {
            var data = await EmailTextService.Get();
            var model = mapper.Map<IEnumerable<EmailTextDto>>(data);

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
        public async Task<IActionResult> Create(EmailTextDto dto)
        {
            try
            {
                var data = mapper.Map<EmailText>(dto);
                await EmailTextService.Create(data);

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
            var data = await EmailTextService.GetById(id);
            var model = mapper.Map<EmailTextDto>(data);
            return View(model);
        }
        [HttpPost]
        public IActionResult Edit(EmailTextDto dto)
        {
            try
            {

                var data = mapper.Map<EmailText>(dto);
                EmailTextService.Update(data);

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
                var data = await EmailTextService.GetById(id);
                EmailTextService.Delete(data);

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

        #region get texts 
       
        [HttpGet]
        public IActionResult GetText(int id)
        {
            var text = context.EmailText.FirstOrDefault(t => t.Id == id); // Fetch text by id
            return Json(text); // Return text as JSON
        }

        #endregion
    }
}
