using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Controllers
{
    public class Admin_RoomsController : Controller
    {

        private readonly DB db;

        public Admin_RoomsController(DB db)
        {
            this.db = db;
        }

        public IActionResult Rooms()
        {
            var m = db.Rooms.Include(rm => rm.Category);

            return View(m);
        }
    }
}
