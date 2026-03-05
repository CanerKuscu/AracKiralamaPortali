using Microsoft.AspNetCore.Mvc;

namespace AracKiralamaPortali.UI.Controllers
{
    public class AdminController : Controller
    {
        public IActionResult Brands() => View();
        public IActionResult Vehicles() => View();
        public IActionResult Reservations() => View();
        public IActionResult Payments() => View();
        public IActionResult Users() => View();
        public IActionResult AdditionalServices() => View();
        public IActionResult Maintenances() => View();
        public IActionResult Fleet() => View();
        public IActionResult Reports() => View();
    }
}
