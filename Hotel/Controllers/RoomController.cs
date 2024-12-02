using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers;

public class RoomController : Controller
{
    private readonly DB db;

    public RoomController(DB db)
    {
        this.db = db;
    }

    public IActionResult RoomPage()
    {
        return View();
    }
}
