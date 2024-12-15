using Hotel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        // Get distinct categories for food services
        ViewBag.ServiceTypes = db.Services
       .Where(s => s.ServiceType == "Food")
       .Select(s => s.Category)
       .Distinct()
       .ToList();

        // Get food services based on selected category
        // If no category is provided, default to "Breakfast"
        var foodServices = string.IsNullOrEmpty(Category)
        ? db.Services.Where(s => s.Category == "Breakfast" && s.ServiceType == "Food")
        : db.Services.Where(s => s.Category == Category && s.ServiceType == "Food");

        ViewBag.SelectedCategory = Category ?? "Breakfast";

        //Get room from category
        var rooms = db.Rooms
        .Include(r => r.Category)
        .Where(r => r.Status == "Active" && r.Category.Status == "Active")
        .Select(r => new RoomViewModel
        {
            RoomID = r.RoomID,
            RoomNumber = r.RoomNumber,
            Status = r.Status,
            CategoryID = r.CategoryID,
            CategoryName = r.Category.CategoryName,
            Theme = r.Category.Theme,
            Size = r.Category.Size,
            Capacity = r.Category.Capacity,
            Bed = r.Category.Bed,
            Description = r.Category.Description,
            PricePerNight = r.Category.PricePerNight,
            RoomImage = r.Category.RoomImage,
            CategoryStatus = r.Category.Status
        })
        .ToList();

        var viewModel = new HomePageVM
        {
            FoodServices = foodServices.ToList(),
            Rooms = rooms
        };

        return View(viewModel);
    }

    public IActionResult RoomPage()
    {
        return View();
    }

    // GET: ROOMDEATILS
    [Authorize(Roles = "Member")]
    public IActionResult RoomDetailsPage(string? Category)
    {
        // Get distinct categories for food services
        ViewBag.ServiceTypes = db.Services
       .Where(s => s.ServiceType == "Food")
       .Select(s => s.Category)
       .Distinct()
       .ToList();

        // Get food services based on selected category
        // If no category is provided, default to "Breakfast"
        var foodServices = string.IsNullOrEmpty(Category)
        ? db.Services.Where(s => s.Category == "Breakfast" && s.ServiceType == "Food")
        : db.Services.Where(s => s.Category == Category && s.ServiceType == "Food");

        ViewBag.SelectedCategory = Category ?? "Breakfast";

        // Get room services (ServiceType == "Room")
        var roomServices = db.Services
        .Where(s => s.ServiceType == "Room")
        .ToList();

        // Create the view model and assign the lists
        var viewModel = new RoomDetailsVM
        {
            FoodServices = foodServices.ToList(),
            RoomServices = roomServices
        };

        return View(viewModel);
    }
}
