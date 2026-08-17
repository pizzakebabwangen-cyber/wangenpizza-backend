using Microsoft.AspNetCore.Mvc;

namespace WangenPizza.Controllers
{
    public class OpeningHoursController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
