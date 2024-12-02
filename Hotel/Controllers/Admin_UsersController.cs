using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class Admin_UsersController : Controller
    {
        public IActionResult Users()
        {
            return View();
        }
    }
}
