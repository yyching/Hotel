using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers
{
    public class Admin_UsersController : Controller
    {

        private readonly DB db;

        public Admin_UsersController(DB db)
        {
            this.db = db;
        }

        public IActionResult Users()
        {
            var m = db.Users.Where(u => u.Role == "Member").ToList();

            return View(m);
        }
    }
}
