using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Controllers;

public class RoomDetailsController : Controller
{
    private readonly DB db;

    public RoomDetailsController(DB db)
    {
        this.db = db;
    }

    public IActionResult RoomDetailsPage()
    {
        return View();
    }
}
