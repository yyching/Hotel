using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class Admin_ProfileController : Controller
    {
        public IActionResult Profile()
        {
            return View();
        }

        public IActionResult EditProfile()
        {
            return View();
        }

        public IActionResult ChangePassword()
        {
            return View();
        }
    }
}
