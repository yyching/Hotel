using System;
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

        // Get room from category
        var rooms = db.Rooms
        .Include(r => r.Category)
        .Where(r => r.Status == "Active" && r.Category.Status == "Active")
        .ToList();

        var viewModel = new HomeCombineVM{
            HomePageVM = new HomePageVM(), // Initialize HomePageVM
            SearchVM = new SearchVM()
        };
        viewModel.HomePageVM.FoodServices = foodServices.ToList();
        viewModel.HomePageVM.Rooms = rooms;
        viewModel.SearchVM.CheckInDate = DateTime.Now.Date;
        viewModel.SearchVM.CheckOutDate = DateTime.Now.Date.AddDays(1);
        viewModel.SearchVM.Person = 1;

        return View(viewModel);
    }

    [HttpPost]
    public IActionResult Index([Bind(Prefix = "SearchVM")]SearchVM sm)
    {
        if (ModelState.IsValid)
        {
            if (sm.CheckInDate < DateTime.Now.Date)
            {
                TempData["Info"] = "Check-in date cannot be in the past.";
                return RedirectToAction("Index");
            }

            if (sm.CheckOutDate <= sm.CheckInDate)
            {
                TempData["Info"] = "Check-out date must be after the check-in date.";
                return RedirectToAction("Index");
            }

            // If model state is valid and business logic passes
            var rooms = db.Rooms
                .Include(r => r.Category)
                .ToList();

            var availableRooms = rooms
                .Where(r => int.Parse(r.Category.Capacity) >= sm.Person &&
                    !db.Bookings.Any(b =>
                        b.RoomID == r.RoomID &&
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
        return RedirectToAction();
    }

    public IActionResult RoomPage(string? checkIn, string? checkOut, int? persons)
    {
        if (checkIn == null && checkOut == null && persons == null)
        {
            //Get room from category
            var rooms = db.Rooms
                .Include(r => r.Category)
                .Where(r => r.Status == "Active" && r.Category.Status == "Active")
                .ToList();

            var viewModel = new RoomVM
            {
                Rooms = rooms
            };

            return View(viewModel);
        }
        else
        {
            var rooms = db.Rooms
                .Include(r => r.Category)
                .Where(r => r.Status == "Active" && r.Category.Status == "Active")
                .ToList();

            DateTime checkInDate = DateTime.Parse(checkIn);
            DateTime checkOutDate = DateTime.Parse(checkOut);

            var availableRooms = rooms
                .Where(r => int.Parse(r.Category.Capacity) >= persons &&
                    !db.Bookings.Any(b =>
                        b.RoomID == r.RoomID &&
                        b.CheckInDate < checkInDate &&
                        b.CheckOutDate > checkOutDate))
                .ToList();

            var viewModel = new RoomVM
            {
                Rooms = availableRooms
            };

            return View(viewModel);
        }
    }

    // GET: ROOMDEATILS
    [Authorize]
    [Authorize(Roles = "Member")]
    public IActionResult RoomDetailsPage(string roomID, string? Category)
    {
        // Get the room from roomID
        var rooms = db.Rooms
        .Include(r => r.Category)
        .FirstOrDefault(r => r.RoomID == roomID);

        if (rooms == null)
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
            Rooms = rooms,
            FoodServices = foodServices.ToList(),
            RoomServices = roomServices
        };

        return View(viewModel);
    }
}
