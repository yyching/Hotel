using System;
using System.Linq;
using Hotel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using X.PagedList.Extensions;

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

        // Get room from category
        var categories = db.Rooms
                    .Include(r => r.Category)
                    .Where(r => r.Status == "Active" && r.Category.Status == "Active")
                    .GroupBy(r => r.Category.CategoryID)
                    .Select(g => g.First().Category)
                    .ToList();

        var vm = new HomePageVM
        {
            FoodServices = foodServices.ToList(),
            Categories = categories,
            SearchVM = new RoomSearchVM
            {
                CheckInDate = DateTime.Today.ToDateOnly(),
                CheckOutDate = DateTime.Today.ToDateOnly().AddDays(1),
                Person = 1
            }
        };

        if (Request.IsAjax())
        {
            return PartialView("FoodMenu", vm);
        }

        return View(vm);
    }

    [HttpPost]
    public IActionResult Index([Bind(Prefix = "SearchVM")]RoomSearchVM sm, string? Category)
    {
        // Validation (1): CheckIn within 30 days range
        if (ModelState.IsValid("SearchVM.CheckInDate"))
        {
            var minDate = DateTime.Today.ToDateOnly();
            var maxDate = DateTime.Today.ToDateOnly().AddDays(30);

            if (sm.CheckInDate < minDate || sm.CheckInDate > maxDate)
            {
                ModelState.AddModelError("SearchVM.CheckInDate", "Date out of range.");
            }
        }

        // Validation (2): CheckOut within 10 days range (after CheckIn)
        if (ModelState.IsValid("SearchVM.CheckInDate") && ModelState.IsValid("SearchVM.CheckOutDate"))
        {
            var minDate = sm.CheckInDate.AddDays(1);
            var maxDate = sm.CheckInDate.AddDays(10);

            if (sm.CheckOutDate < minDate || sm.CheckOutDate > maxDate)
            {
                ModelState.AddModelError("SearchVM.CheckOutDate", "Date out of range.");
            }
        }

        if (ModelState.IsValid)
        {
            // If model state is valid get the available room
            var rooms = db.Rooms
                     .Include(r => r.Category)
                     .Where(r => r.Status == "Active" && r.Category.Status == "Active")
                     .ToList();

            var availableRooms = rooms
                .Where(r => int.Parse(r.Category.Capacity) >= sm.Person &&
                    !db.Bookings.Any(b =>
                        b.CheckInDate < sm.CheckOutDate &&
                        b.CheckOutDate > sm.CheckInDate))
                .ToList();

            if (availableRooms.Count == 0)
            {
                TempData["Info"] = "No rooms are available for the selected dates and capacity.";
                return RedirectToAction("Index");
            }
            else
            {
                return RedirectToAction("RoomPage", new
                {
                    checkIn = sm.CheckInDate.ToString("yyyy-MM-dd"),
                    checkOut = sm.CheckOutDate.ToString("yyyy-MM-dd"),
                    persons = sm.Person
                });
            }
        }

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

        // Get room from category
        var categories = db.Rooms
                    .Include(r => r.Category)
                    .Where(r => r.Status == "Active" && r.Category.Status == "Active")
                    .GroupBy(r => r.Category.CategoryID)
                    .Select(g => g.First().Category)
                    .ToList();

        var vm = new HomePageVM
        {
            FoodServices = foodServices.ToList(),
            Categories = categories,
            SearchVM = new RoomSearchVM
            {
                CheckInDate = sm.CheckInDate,
                CheckOutDate = sm.CheckOutDate,
                Person = sm.Person
            }
        };

        return View(vm);
    }

    public IActionResult RoomPage(DateOnly? checkIn, DateOnly? checkOut, int? persons, double? minPrice, double? maxPrice, string? themes, string? category)
    {
        // Get room from category
        var categories = db.Rooms
                    .Include(r => r.Category)
                    .Where(r => r.Status == "Active" && r.Category.Status == "Active")
                    .GroupBy(r => r.Category.CategoryID)
                    .Select(g => g.First().Category)
                    .ToList();

        // Get themes for room
        ViewBag.Themes = db.Categories
            .Where(c => c.Status == "Active")
            .Select(c => c.Theme)
            .Distinct() 
            .ToList();

        ViewBag.RoomCategory = db.Categories
            .Where(c => c.Status == "Active")
            .Select(c => c.CategoryName)
            .Distinct()
            .ToList();

        if (minPrice.HasValue && maxPrice.HasValue)
        {
            categories = categories.Where(c => c.PricePerNight >= minPrice && c.PricePerNight <= maxPrice).ToList();
        }

        if (!string.IsNullOrEmpty(themes))
        {
            var themeList = themes.Split(',').Select(t => t.Trim()).ToList();
            categories = categories.Where(c => themeList.Contains(c.Theme)).ToList();
        }

        if (!string.IsNullOrEmpty(category))
        {
            var categoryList = category.Split(',').Select(t => t.Trim()).ToList();
            categories = categories.Where(c => category.Contains(c.CategoryName)).ToList();
        }

        if (checkIn != null && checkOut != null && persons != null)
        {
            // Validation (1): CheckIn within 30 days range
            if (ModelState.IsValid("CheckInDate"))
            {
                var a = DateTime.Today.ToDateOnly();
                var b = DateTime.Today.ToDateOnly().AddDays(30);

                if (checkIn < a || checkIn > b)
                {
                    ModelState.AddModelError("CheckInDate", "Date out of range.");
                }
            }

            // Validation (2): CheckOut within 10 days range (after CheckIn)
            if (ModelState.IsValid("CheckInDate") && ModelState.IsValid("CheckOutDate"))
            {
                var a = checkIn.Value.AddDays(1);
                var b = checkIn.Value.AddDays(30);

                if (checkIn < a || checkOut > b)
                {
                    ModelState.AddModelError("CheckOutDate", "Date out of range.");
                }
            }

            var availableRooms = categories
                .Where(r => int.Parse(r.Capacity) >= persons &&
                        !db.Bookings.Any(b =>
                        b.CheckInDate < checkOut &&
                        b.CheckOutDate > checkIn))
                .ToList();

            var m = new RoomPageVM
            {
                Categories = availableRooms,
                SearchVM = new RoomSearchVM
                {
                    CheckInDate = checkIn.Value,
                    CheckOutDate = checkOut.Value,
                    Person = persons ?? 1
                }
            };

            if (Request.IsAjax())
            {
                return PartialView("ShowRoom", m);
            }

            return View(m);
        }

        var vm = new RoomPageVM
        {
            Categories = categories,
            SearchVM = new RoomSearchVM
            {
                CheckInDate = DateTime.Today.ToDateOnly(),
                CheckOutDate = DateTime.Today.ToDateOnly().AddDays(1),
                Person = 1
            }
        };

        if (Request.IsAjax())
        {
            return PartialView("ShowRoom", vm);
        }

        return View(vm);
    }

    [HttpPost]
    public IActionResult RoomPage([Bind(Prefix = "SearchVM")]RoomSearchVM sm)
    {
        // Validation (1): CheckIn within 30 days range
        if (ModelState.IsValid("SearchVM.CheckInDate"))
        {
            var minDate = DateTime.Today.ToDateOnly();
            var maxDate = DateTime.Today.ToDateOnly().AddDays(30);

            if (sm.CheckInDate < minDate || sm.CheckInDate > maxDate)
            {
                ModelState.AddModelError("SearchVM.CheckInDate", "Date out of range.");
            }
        }

        // Validation (2): CheckOut within 10 days range (after CheckIn)
        if (ModelState.IsValid("SearchVM.CheckInDate") && ModelState.IsValid("SearchVM.CheckOutDate"))
        {
            var minDate = sm.CheckInDate.AddDays(1);
            var maxDate = sm.CheckInDate.AddDays(10);

            if (sm.CheckOutDate < minDate || sm.CheckOutDate > maxDate)
            {
                ModelState.AddModelError("SearchVM.CheckOutDate", "Date out of range.");
            }
        }

        // Get room from category
        var categories = db.Rooms
                    .Include(r => r.Category)
                    .Where(r => r.Status == "Active" && r.Category.Status == "Active")
                    .GroupBy(r => r.Category.CategoryID)
                    .Select(g => g.First().Category)
                    .ToList();

        if (ModelState.IsValid)
        {
            var availableRooms = categories
                .Where(r => int.Parse(r.Capacity) >= sm.Person &&
                        !db.Bookings.Any(b =>
                        b.CheckInDate < sm.CheckOutDate &&
                        b.CheckOutDate > sm.CheckInDate))
                .ToList();

            if (availableRooms.Count == 0)
            {
                TempData["Info"] = "No rooms are available for the selected dates and capacity.";
                var m = new RoomPageVM
                {
                    Categories = categories,
                    SearchVM = new RoomSearchVM
                    {
                        CheckInDate = sm.CheckInDate,
                        CheckOutDate = sm.CheckOutDate,
                        Person = sm.Person
                    }
                };

                return View(m);
            }
            else
            {
                return RedirectToAction("RoomPage", new
                {
                    checkIn = sm.CheckInDate.ToString("yyyy-MM-dd"),
                    checkOut = sm.CheckOutDate.ToString("yyyy-MM-dd"),
                    persons = sm.Person
                });
            }
        }

        // Get themes for room
        ViewBag.Themes = db.Categories
            .Where(c => c.Status == "Active")
            .Select(c => c.Theme)
            .Distinct() 
            .ToList();

        ViewBag.RoomCategory = db.Categories
            .Where(c => c.Status == "Active")
            .Select(c => c.CategoryName)
            .Distinct()
            .ToList();

        var vm = new RoomPageVM
        {
            Categories = categories,
            SearchVM = new RoomSearchVM
            {
                CheckInDate = sm.CheckInDate,
                CheckOutDate = sm.CheckOutDate,
                Person = sm.Person
            }
        };

        return View(vm);
    }

    // GET: ROOMDEATILS
    [Authorize]
    [Authorize(Roles = "Member")]
    public IActionResult RoomDetailsPage(string categoryID, string? FoodCategory)
    {
        // Get the room from roomID
        var categories = db.Categories
        .FirstOrDefault(c => c.CategoryID == categoryID);

        if (categories == null)
        {
            TempData["Info"] = "Invalid Room";
            return RedirectToAction("Index", "Home");
        }

        // Get distinct categories for food services
        ViewBag.ServiceTypes = db.Services
       .Where(s => s.ServiceType == "Food")
       .Select(s => s.Category)
       .Distinct()
       .ToList();

        // Get food services based on selected category
        // If no category is provided, default to "Breakfast"
        var foodServices = string.IsNullOrEmpty(FoodCategory)
        ? db.Services.Where(s => s.Category == "Breakfast" && s.ServiceType == "Food")
        : db.Services.Where(s => s.Category == FoodCategory && s.ServiceType == "Food");

        ViewBag.SelectedCategory = FoodCategory ?? "Breakfast";

        // Get room services (ServiceType == "Room")
        var roomServices = db.Services
        .Where(s => s.ServiceType == "Room")
        .ToList();

        // Create the view model and assign the lists
        var vm = new RoomDetailsVM
        {
            Categories = categories,
            FoodServices = foodServices.ToList(),
            RoomServices = roomServices
        };

        if (Request.IsAjax())
        {
            return PartialView("FoodAddOn", vm);
        }

        return View(vm);
    }
}
