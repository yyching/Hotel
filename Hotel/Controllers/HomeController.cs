using System;
using System.Linq;
using Hotel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using X.PagedList.Extensions;
using static System.Net.Mime.MediaTypeNames;

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

    // GET: Home/Index
    public IActionResult Index(string? Category)
    {
        // Get distinct categories for food services
        ViewBag.ServiceTypes = db.Services
       .Where(s => s.ServiceType == "Food" && s.Status == "Active")
       .Select(s => s.Category)
       .Distinct()
       .ToList();

        ViewBag.SelectedCategory = Category ?? "Breakfast";

        // Get food services based on selected category
        // If no category is provided, default to "Breakfast"
        var foodServices = string.IsNullOrEmpty(Category)
        ? db.Services.Where(s => s.Category == "Breakfast" && s.ServiceType == "Food" && s.Status == "Active")
        : db.Services.Where(s => s.Category == Category && s.ServiceType == "Food" && s.Status == "Active");

        ViewBag.Food = foodServices.ToList();

        // Get room from category
        var categories = db.Rooms
                    .Include(r => r.Category)
                    .Where(r => r.Status == "Active" && r.Category.Status == "Active")
                    .GroupBy(r => r.Category.CategoryID)
                    .Select(g => g.First().Category)
                    .ToList();

        ViewBag.Room = categories;

        var categoryImages = db.CategoryImages
                            .Include(r => r.Category)
                            .ToList();

        var categoryImageDict = categoryImages
                                .GroupBy(ci => ci.CategoryID)
                                .Select(g => g.First())
                                .ToDictionary(ci => ci.CategoryID, ci => ci.ImagePath);

        // Get the category Image
        ViewBag.categoryImage = categoryImageDict;

        var vm = new HomePageVM
        {
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

    // POST: Home/Index
    [HttpPost]
    public IActionResult Index([Bind(Prefix = "SearchVM")] RoomSearchVM sm, string? Category)
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
            return RedirectToAction("RoomPage", new
            {
                checkIn = sm.CheckInDate.ToString("MM/dd/yyyy"),
                checkOut = sm.CheckOutDate.ToString("MM/dd/yyyy"),
                persons = sm.Person
            });
        }

        return View();
    }

    // GET: Home/RoomPage
    public IActionResult RoomPage(DateOnly? checkIn, DateOnly? checkOut, int? persons, double? minPrice, double? maxPrice, string? themes, string? category, string? sort)
    {
        // Get category from room
        var categories = db.Rooms
                    .Include(r => r.Category)
                    .Where(r => r.Status == "Active" && r.Category.Status == "Active")
                    .GroupBy(r => r.Category.CategoryID)
                    .Select(g => g.First().Category)
                    .ToList();

        var categoryImages = db.CategoryImages
                           .Include(r => r.Category)
                           .ToList();

        var categoryImageDict = categoryImages
                                .GroupBy(ci => ci.CategoryID)
                                .Select(g => g.First())
                                .ToDictionary(ci => ci.CategoryID, ci => ci.ImagePath);

        // Get the category Image
        ViewBag.categoryImage = categoryImageDict;

        // Get themes for room
        ViewBag.Themes = db.Categories
            .Where(c => c.Status == "Active")
            .Select(c => c.Theme)
            .Distinct()
            .ToList();

        // Get category for room
        ViewBag.RoomCategory = db.Categories
            .Where(c => c.Status == "Active")
            .Select(c => c.CategoryName)
            .Distinct()
            .ToList();

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

            // check the room available
            var occupiedRooms = db.Bookings
                                  .Where(b => checkIn < b.CheckOutDate &&
                                         b.CheckInDate < checkOut)
                                  .Select(b => b.RoomID);

            var availableRooms = db.Rooms
                .Include(r => r.Category)
                .Where(r => !occupiedRooms.Contains(r.RoomID))
                .ToList()
                .Where(r => Convert.ToInt32(r.Category.Capacity) >= persons)
                .GroupBy(r => r.Category.CategoryID)
                .Select(g => g.First().Category)
                .ToList();

            if (availableRooms.Count == 0)
            {
                TempData["Info"] = "No rooms are available for the selected dates and capacity.";
            }

            // sort by price
            if (!string.IsNullOrEmpty(sort))
            {
                switch (sort.ToLower())
                {
                    case "low-to-high":
                        availableRooms = availableRooms.OrderBy(r => r.PricePerNight).ToList();
                        break;
                    case "high-to-low":
                        availableRooms = availableRooms.OrderByDescending(r => r.PricePerNight).ToList();
                        break;
                }
            }

            // filter by price
            if (minPrice.HasValue && maxPrice.HasValue)
            {
                availableRooms = availableRooms.Where(c => c.PricePerNight >= minPrice && c.PricePerNight <= maxPrice).ToList();
            }

            // filter by themes
            if (!string.IsNullOrEmpty(themes))
            {
                var themeList = themes.Split(',').Select(t => t.Trim()).ToList();
                availableRooms = availableRooms.Where(c => themeList.Contains(c.Theme)).ToList();
            }

            // filter by category
            if (!string.IsNullOrEmpty(category))
            {
                var categoryList = category.Split(',').Select(t => t.Trim()).ToList();
                availableRooms = availableRooms.Where(c => category.Contains(c.CategoryName)).ToList();
            }

            ViewBag.Categories = availableRooms;

            var sm = new RoomPageVM
            {
                SearchVM = new RoomSearchVM
                {
                    CheckInDate = checkIn.Value,
                    CheckOutDate = checkOut.Value,
                    Person = persons.Value
                }
            };

            if (Request.IsAjax())
            {
                return PartialView("ShowRoom", sm);
            }

            return View(sm);
        }

        // sort by price
        if (!string.IsNullOrEmpty(sort))
        {
            switch (sort.ToLower())
            {
                case "low-to-high":
                    categories = categories.OrderBy(c => c.PricePerNight).ToList();
                    break;
                case "high-to-low":
                    categories = categories.OrderByDescending(c => c.PricePerNight).ToList();
                    break;
            }
        }

        // filter by price
        if (minPrice.HasValue && maxPrice.HasValue)
        {
            categories = categories.Where(c => c.PricePerNight >= minPrice && c.PricePerNight <= maxPrice).ToList();
        }

        // filter by theme
        if (!string.IsNullOrEmpty(themes))
        {
            var themeList = themes.Split(',').Select(t => t.Trim()).ToList();
            categories = categories.Where(c => themeList.Contains(c.Theme)).ToList();
        }

        // filter by category
        if (!string.IsNullOrEmpty(category))
        {
            var categoryList = category.Split(',').Select(t => t.Trim()).ToList();
            categories = categories.Where(c => category.Contains(c.CategoryName)).ToList();
        }

        ViewBag.Categories = categories;

        var vm = new RoomPageVM
        {
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

    // POST: Home/RoomPage
    [HttpPost]
    public IActionResult RoomPage([Bind(Prefix = "SearchVM")] RoomSearchVM sm)
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
            return RedirectToAction("RoomPage", new
            {
                checkIn = sm.CheckInDate.ToString("MM/dd/yyyy"),
                checkOut = sm.CheckOutDate.ToString("MM/dd/yyyy"),
                persons = sm.Person
            });
        }

        return View();
    }

    // GET: Home/RoomDetails
    [Authorize]
    [Authorize(Roles = "Member")]
    public IActionResult RoomDetailsPage(string categoryID, DateOnly? CheckInDate, DateOnly? CheckOutDate, DateOnly? month, 
                                         string[]? foodServiceIds, int[]? foodQuantities, string[]? roomServiceIds, int[]? roomQuantities)
    {
        // show the reservation table
        var m = month.GetValueOrDefault(DateTime.Today.ToDateOnly());
        // Min = First day of the month
        // Max = First day of next month
        var min = new DateOnly(m.Year, m.Month, 1);
        var max = min.AddMonths(1);
        ViewBag.Min = min;
        ViewBag.Max = max;

        var rooms = db.Rooms
           .Where(rm => rm.CategoryID == categoryID)
           .OrderBy(rm => rm.RoomNumber)
           .ToList();

        var dict = rooms.ToDictionary(rm => rm, rn => new List<DateOnly>());

        var roomIds = rooms.Select(r => r.RoomID).ToList();

        var reservations = db.Bookings
           .Where(rs => roomIds.Contains(rs.RoomID))
           .Where(rs => rs.CheckInDate < max && min < rs.CheckOutDate);

        foreach (var rs in reservations)
        {
            for (var d = rs.CheckInDate; d < rs.CheckOutDate; d = d.AddDays(1))
            {
                dict[rs.Room].Add(d);
            }
        }
        ViewBag.RoomReservations = dict;

        // Get the category from categoryID
        var categories = db.Categories
        .FirstOrDefault(c => c.CategoryID == categoryID);

        ViewBag.Categories = categories;

        if (categories == null)
        {
            TempData["Info"] = "Invalid Room";
            return RedirectToAction("RoomPage", "Home");
        }

        // Get the first ImagePath
        var categoryImagePath = db.CategoryImages
                          .Where(ci => ci.CategoryID == categoryID)
                          .Select(ci => ci.ImagePath)
                          .FirstOrDefault();
        ViewBag.CategoryImagePath = categoryImagePath;

        // Get the ImagePath list
        var categoryImagePaths = db.CategoryImages
                           .Where(ci => ci.CategoryID == categoryID)
                           .Select(ci => ci.ImagePath)
                           .ToList();
        ViewBag.CategotyImagePaths = categoryImagePaths;

        // Get distinct categories for food services
        ViewBag.breakfastServices = db.Services
       .Where(s => s.Category == "Breakfast" && s.Status == "Active")
       .ToList();

        // Get distinct categories for food services
        ViewBag.lunchServices = db.Services
       .Where(s => s.Category == "Lunch" && s.Status == "Active")
       .ToList();

        // Get distinct categories for food services
        ViewBag.dinnerServices = db.Services
       .Where(s => s.Category == "Dinner" && s.Status == "Active")
       .ToList();

        // Get room services (ServiceType == "Room")
        ViewBag.RoomServices = db.Services
        .Where(s => s.ServiceType == "Room" && s.Status == "Active")
        .ToList();

        // Get the selected service qty if no available room
        ViewBag.FoodServiceIds = foodServiceIds;
        ViewBag.FoodQuantities = foodQuantities;
        ViewBag.RoomServiceIds = roomServiceIds;
        ViewBag.RoomQuantities = roomQuantities;

        var vm = new RoomDetailsVM
        {
            CheckInDate = CheckInDate ?? DateTime.Today.ToDateOnly(),
            CheckOutDate = CheckOutDate ?? DateTime.Today.ToDateOnly().AddDays(1),
        };

        if (Request.IsAjax())
        {
            return PartialView("RoomStatus", vm);
        }

        return View(vm);
    }

    //POST: Home/RoomDetails
    [Authorize]
    [Authorize(Roles = "Member")]
    [HttpPost]
    public IActionResult RoomDetailsPage(RoomDetailsVM sm, DateOnly? month, string categoryID)
    {
        // Validation (1): CheckIn within 30 days range
        if (ModelState.IsValid("CheckInDate"))
        {
            var minDate = DateTime.Today.ToDateOnly();
            var maxDate = DateTime.Today.ToDateOnly().AddDays(30);

            if (sm.CheckInDate < minDate || sm.CheckInDate > maxDate)
            {
                ModelState.AddModelError("CheckInDate", "Date out of range.");
            }
        }

        // Validation (2): CheckOut within 10 days range (after CheckIn)
        if (ModelState.IsValid("CheckInDate") && ModelState.IsValid("CheckOutDate"))
        {
            var minDate = sm.CheckInDate.AddDays(1);
            var maxDate = sm.CheckInDate.AddDays(10);

            if (sm.CheckOutDate < minDate || sm.CheckOutDate > maxDate)
            {
                ModelState.AddModelError("CheckOutDate", "Date out of range.");
            }
        }

        if (ModelState.IsValid)
        {
            return RedirectToAction("PaymentPage", "Payment", new
            {
                categoryID = sm.CategoryID,
                checkIn = sm.CheckInDate,
                checkOut = sm.CheckOutDate,
                foodServiceIds = sm.FoodServiceIds,
                foodQuantities = sm.FoodQuantities,
                roomServiceIds = sm.RoomServiceIds,
                roomQuantities = sm.RoomQuantities
            });
        }

        return View();
    }

    public IActionResult AboutUs()
    {
        return View();
    }
}
