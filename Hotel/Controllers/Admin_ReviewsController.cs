using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class Admin_ReviewsController : Controller
    {
        public IActionResult Reviews()
        {
            return View();
        }
    }
}
