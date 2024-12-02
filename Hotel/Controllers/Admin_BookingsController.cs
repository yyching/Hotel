using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class Admin_BookingsController : Controller
    {
        public IActionResult Bookings()
        {
            return View();
        }
    }
}
