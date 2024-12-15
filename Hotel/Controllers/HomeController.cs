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

    // GET: HOMEPAGE
    public IActionResult Index(string? Category)
    {
        ViewBag.ServiceTypes = db.Services
        .Where(s => s.ServiceType == "Food") 
        .Select(s => s.Category)            
        .Distinct()                         
        .ToList();

        // If no category is provided, default to "Breakfast"
        var services = string.IsNullOrEmpty(Category)
        ? db.Services.Where(s => s.Category == "Breakfast" && s.ServiceType == "Food")
        : db.Services.Where(s => s.Category == Category && s.ServiceType == "Food");

        return View(services);
    }

    public IActionResult RoomPage()
    {
        return View();
    }

    // GET: ROOMDEATILS
    [Authorize(Roles = "Member")]
    public IActionResult RoomDetailsPage(string? Category)
    {
        ViewBag.ServiceTypes = db.Services
       .Where(s => s.ServiceType == "Food")
       .Select(s => s.Category)
       .Distinct()
       .ToList();

        // If no category is provided, default to "Breakfast"
        var services = string.IsNullOrEmpty(Category)
        ? db.Services.Where(s => s.Category == "Breakfast" && s.ServiceType == "Food")
        : db.Services.Where(s => s.Category == Category && s.ServiceType == "Food");

        ViewBag.SelectedCategory = Category ?? "Breakfast";

        return View(services);
    }
}
