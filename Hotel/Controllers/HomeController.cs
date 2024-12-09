using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hotel.Controllers;

public class HomeController : Controller
{
    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;

    public HomeController(DB db, IWebHostEnvironment en, Helper hp)
    {
        this.db = db;
        this.en = en;
        this.hp = hp;
    }

    //public IActionResult Index(string? ServiceTypeID)
    //{
    //    ViewBag.ServiceTypes = db.ServiceTypes;

    //    var m = db.Services.Where(s => s.ServiceTypeID == ServiceTypeID);

    //    return View(m);
    //}

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult RoomPage()
    {
        return View();
    }

    [Authorize(Roles = "Member")]
    public IActionResult RoomDetailsPage()
    {
        return View();
    }
}
