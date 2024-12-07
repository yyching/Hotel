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

    public IActionResult RoomDetailsPage(string? ServiceTypeID)
    {
        ViewBag.ServiceTypes = db.ServiceTypes;

        var m = db.Services.Where(s => s.ServiceTypeID == ServiceTypeID);

        return View(m);
    }
}
