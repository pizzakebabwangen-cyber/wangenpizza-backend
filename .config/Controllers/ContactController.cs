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

    public class ContactController : Controller
    {

        #region Ctor

        private readonly IMapper mapper;
        private readonly IContactService ContactService;
        public ContactController(IMapper mapper , IContactService ContactService)
        {
            this.mapper = mapper;
            this.ContactService = ContactService;
        }
        #endregion

        #region Index
        public async Task<IActionResult> Index()
        {
            var data = await ContactService.Get();
            var model = mapper.Map<IEnumerable<ContactDto>>(data);
            return View(model);
        }
        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
        {
            var data = await ContactService.GetById(id);
            var model = mapper.Map<ContactDto>(data);
            return View(model);
        }
        #endregion

        #region Create
        [HttpGet]
        public IActionResult Add()
        {
            ViewBag.TextAreaValue = "Keine Nachricht";

            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Add(ContactDto dto)
        {
            try
            {
                var data = mapper.Map<Contact>(dto);
                await ContactService.Create(data);

                if (data != null)
                {
                    return RedirectToAction("Index", "Contact");
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
            var data = await ContactService.GetById(id);
            var model = mapper.Map<ContactDto>(data);
            return View(model);
        }
        [HttpPost]
        public IActionResult Edit(ContactDto dto)
        {
            try
            {
               

                
                var data = mapper.Map<Contact>(dto);
                ContactService.Update(data);

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
                var data = await ContactService.GetById(id);
                ContactService.Delete(data);

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
