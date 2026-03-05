using Microsoft.AspNetCore.Mvc;

namespace AracKiralamaPortali.UI.Controllers
{
    public class ReservationController : Controller
    {
        public IActionResult MyReservations() => View();
    }
}
