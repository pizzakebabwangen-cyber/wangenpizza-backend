using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using WangenPizza.Dtos;
using WangenPizza.Interfaces;
using WangenPizza.Models;

namespace WangenPizza.Controllers
{
    [Authorize(Roles ="Admin")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> logger;
        private readonly ICartService cartService;
        private readonly IContactService contactService;
        private readonly IOfferService offerService;
        private readonly ITodayBonusService todayBonusService;
        private readonly ICategoryService categoryService;
        private readonly IProductService productService;
        private readonly IDiscountCodeService discountCodeService;

        public HomeController(ILogger<HomeController> logger, ICartService cartService , IContactService contactService , IOfferService offerService , ITodayBonusService todayBonusService , ICategoryService categoryService , IProductService productService , IDiscountCodeService discountCodeService)
        {
            this.logger = logger;
            this.cartService = cartService;
            this.contactService = contactService;
            this.offerService = offerService;
            this.todayBonusService = todayBonusService;
            this.categoryService = categoryService;
            this.productService = productService;
            this.discountCodeService = discountCodeService;
        }

        public async Task<IActionResult> Index()
        {
            var successMessage = TempData["SuccessEmailMessage"] as string;

            // Optionally, you can check if there's a message to display to the user
            if (!string.IsNullOrEmpty(successMessage))
            {
                // You can pass it to the view to display it
                TempData["SuccessEmailMessage"] = "E-Mail wurde gesendet, Danke";
            }

            try
            {
                var OrdersData = await cartService.GetAllSucceededOrders();
                var ContactData = await contactService.Get();
                var OfferstData = await productService.GetOffers();
                var TodayBonustData = await productService.GetTodayBonus();
                var CategoriestData = await categoryService.Get();
                var ProductstData = await productService.Get();
                var DiscountCodestData = await discountCodeService.Get();

                HomeDto dto = new HomeDto()
                {
                    OrdersCount = OrdersData.Count(),
                    OrdersTotalPrice = OrdersData.Sum(a => a.FinalTotalNumber),
                    ContactsCount = ContactData.Count(),
                    OffersCount = OfferstData.Count(),
                    TodayBonusCount = TodayBonustData.Count(),
                    CategoriesCount = CategoriestData.Count(),
                    ProductsCount = ProductstData.Count(),
                    CouponCodesCount = DiscountCodestData.Count()
                };
                return View(dto);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Home/Index: Dashboard-Daten konnten nicht geladen werden (DB oder Service). InnerException: {InnerException}, StackTrace: {StackTrace}", ex.InnerException?.Message, ex.StackTrace);
                var detail = ex.InnerException != null
                    ? $"{ex.Message} → {ex.InnerException.Message}"
                    : ex.Message;
                TempData["DashboardLoadError"] =
                    $"Statistik konnte nicht geladen werden. Bitte Verbindungszeichenfolge / SQL Server prüfen; Details stehen in den Server-Logs. Fehler: {detail}";
                return View(new HomeDto());
            }
        }

        /// <summary>
        /// ExceptionHandler-Pfad (Program.cs: /Home/Error). Ohne diese Action + [AllowAnonymous] kann die
        /// Fehlerbehandlung selbst an [Authorize(Roles=Admin)] scheitern → Redirect-Schleifen.
        /// </summary>
        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View();
        }
    }
}
