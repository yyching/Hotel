using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class Admin_DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
