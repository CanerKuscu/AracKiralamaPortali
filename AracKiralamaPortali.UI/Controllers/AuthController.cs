using Microsoft.AspNetCore.Mvc;

namespace AracKiralamaPortali.UI.Controllers
{
    public class AuthController : Controller
    {
        public IActionResult Login() => View();
        public IActionResult Register() => View();
        public IActionResult Profile() => View();
    }
}
