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

    public class DeliveryController : Controller
    {
        #region Ctor

        private readonly IMapper mapper;
        private readonly IDeliveryService DeliveryService;
        public DeliveryController(IMapper mapper, IDeliveryService DeliveryService)
        {
            this.mapper = mapper;
            this.DeliveryService = DeliveryService;
        }
        #endregion

        #region Index
        public async Task<IActionResult> Index()
        {
            var data = await DeliveryService.Get();
            var model = mapper.Map<IEnumerable<DeliveryDto>>(data);
            return View(model);
        }
        #endregion

        #region Details
        public async Task<IActionResult> Details(int id)
        {
            var data = await DeliveryService.GetById(id);
            var model = mapper.Map<DeliveryDto>(data);
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
        public async Task<IActionResult> Create(DeliveryDto dto)
        {
            try
            {
                var data = mapper.Map<Delivery>(dto);
                await DeliveryService.Create(data);

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

        #region Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var data = await DeliveryService.GetById(id);
            var model = mapper.Map<DeliveryDto>(data);
            return View(model);
        }
        [HttpPost]
        public IActionResult Edit(DeliveryDto dto)
        {
            try
            {
                var data = mapper.Map<Delivery>(dto);
                DeliveryService.Update(data);

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

        #region Delete


        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var data = await DeliveryService.GetById(id);
                DeliveryService.Delete(data);

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
