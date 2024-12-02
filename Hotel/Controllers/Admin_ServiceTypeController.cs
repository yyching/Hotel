using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class Admin_ServiceTypeController : Controller
    {
        public IActionResult ServiceType()
        {
            return View();
        }
    }
}
