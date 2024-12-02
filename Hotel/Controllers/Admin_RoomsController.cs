using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class Admin_RoomsController : Controller
    {
        public IActionResult Rooms()
        {
            return View();
        }
    }
}
