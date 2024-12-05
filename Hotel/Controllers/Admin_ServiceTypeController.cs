using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class Admin_ServiceTypeController : Controller
    {

        private readonly DB db;

        public Admin_ServiceTypeController(DB db)
        {
            this.db = db;
        }

        public IActionResult ServiceType()
        {
            var m = db.ServiceTypes;

            return View(m);
        }
    }
}
