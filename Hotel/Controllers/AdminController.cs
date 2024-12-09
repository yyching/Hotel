using Hotel.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Controllers;

public class AdminController : Controller
{

    private readonly DB db;

    public AdminController(DB db)
    {
        this.db = db;
    }

    // Dashboard
    public IActionResult Dashboard()
    {
        return View();
    }

    // Booking
    public IActionResult Bookings()
    {
        return View();
    }

    // Admin Profile
    public IActionResult Profile()
    {
        return View();
    }

    // Admin Profile - Edit
    public IActionResult EditProfile()
    {
        return View();
    }

    // Admin Profile - Change Password
    public IActionResult ChangePassword()
    {
        return View();
    }

    // Review
    public IActionResult Reviews()
    {
        return View();
    }

    // Room Category
    public IActionResult RoomCategory()
    {
        var m = db.Categories;

        return View(m);
    }

    // Room
    public IActionResult Rooms()
    {
        var m = db.Rooms.Include(rm => rm.Category);

        return View(m);
    }

    // Service
    public IActionResult Services()
    {
        var m = db.Services;

        return View(m);
    }

    // User
    public IActionResult Users()
    {
        var m = db.Users.Where(u => u.Role == "Member").ToList();

        return View(m);
    }
}
