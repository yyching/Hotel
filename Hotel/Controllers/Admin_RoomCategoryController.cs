using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class Admin_RoomCategoryController : Controller
    {
        public IActionResult RoomCategory()
        {
            return View();
        }
    }
}
