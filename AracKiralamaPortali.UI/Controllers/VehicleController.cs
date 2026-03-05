using Microsoft.AspNetCore.Mvc;

namespace AracKiralamaPortali.UI.Controllers
{
    public class VehicleController : Controller
    {
        public IActionResult Index() => View();

        public IActionResult Detail(int id)
        {
            ViewData["VehicleId"] = id;
            return View();
        }
    }
}
