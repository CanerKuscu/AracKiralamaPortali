using Microsoft.AspNetCore.Mvc;

namespace AracKiralamaPortali.UI.Controllers
{
    public class ReservationController : Controller
    {
        public IActionResult Create(int vehicleId)
        {
            ViewData["VehicleId"] = vehicleId;
            return View();
        }

        public IActionResult MyReservations() => View();
    }
}
