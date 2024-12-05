using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Controllers
{
    public class Admin_ServicesController : Controller
    {
        private readonly DB db;

        public Admin_ServicesController(DB db)
        {
            this.db = db;
        }

        public IActionResult Services()
        {
            var m = db.Services.Include(sr => sr.ServiceType);

            return View(m);
        }
    }
}
