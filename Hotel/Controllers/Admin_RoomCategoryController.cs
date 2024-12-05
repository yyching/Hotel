using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class Admin_RoomCategoryController : Controller
    {

        private readonly DB db;

        public Admin_RoomCategoryController(DB db)
        {
            this.db = db;
        }

        public IActionResult RoomCategory()
        {
            var m = db.Categories;

            return View(m);
        }
    }
}
