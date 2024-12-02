using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class Admin_ServicesController : Controller
    {
        public IActionResult Services()
        {
            return View();
        }
    }
}
