using Hotel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hotel.Controllers;

public class AdminController : Controller
{

    private readonly DB db;
    private readonly IWebHostEnvironment en;
    private readonly Helper hp;

    public AdminController(DB db, IWebHostEnvironment en, Helper hp)
    {
        this.db = db;
        this.en = en;
        this.hp = hp;
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

    // Room Category - Add | Get
    public IActionResult AddRoomCategory()
    {
        return View();
    }

    // Room Category - Add | Post
    [HttpPost]
    public IActionResult AddRoomCategory(AddRoomCategoryVM vm)
    {
        if (vm.Photo != null)
        {
            var err = hp.ValidatePhoto(vm.Photo);
            if (err != "") ModelState.AddModelError("Photo", err);
        }

        if (ModelState.IsValid) 
        {
            // Generate random ID
            var random = new Random();
            // Generate a random number
            string randomPart = random.Next(100, 999).ToString();

            // Get the current date and time
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");

            // Combine to form the new ID
            string newID = $"RCT{randomPart}{timestamp}";

            db.Categories.Add(new() 
            {
                CategoryID      = newID,
                CategoryName    = vm.categoryName,
                Theme           = vm.theme,
                Size            = vm.size,
                Capacity        = vm.capacity,
                Bed             = vm.bed,
                Description     = vm.description,
                PricePerNight   = vm.price,
                RoomImage       = hp.SavePhoto(vm.Photo, "uploads"),
                Status = "Active",
            });

            db.SaveChanges();

            TempData["Info"] = "Room Category added successfully";
            return RedirectToAction("RoomCategory");
        }

        return View();
    }

    // Room Category - Update | Get
    public IActionResult UpdateRoomCategory(string? id) 
    { 
        var rc = db.Categories.Find(id);

        if (rc == null)
        {
            return RedirectToAction("RoomCategory");
        }

        var vm = new UpdateRoomCategoryVM
        {
            categoryID      = rc.CategoryID,
            categoryName    = rc.CategoryName,
            theme           = rc.Theme,
            size            = rc.Size,
            capacity        = rc.Capacity,
            bed             = rc.Bed,
            description     = rc.Description,
            price           = rc.PricePerNight,
            roomImage       = rc.RoomImage,
        };

        return View(vm);
    }

    // Room Category - Update | Post
    [HttpPost]
    public IActionResult UpdateRoomCategory(UpdateRoomCategoryVM vm)
    {
        var rc = db.Categories.Find(vm.categoryID);

        if (vm.Photo != null)
        {
            // TODO
            var e = hp.ValidatePhoto(vm.Photo);
            if (e != "") ModelState.AddModelError("Photo", e);
        }

        if (ModelState.IsValid)
        {
            rc.CategoryName  = vm.categoryName;
            rc.Theme         = vm.theme;
            rc.Size          = vm.size;
            rc.Capacity      = vm.capacity;
            rc.Bed           = vm.bed;
            rc.Description   = vm.description;
            rc.PricePerNight = vm.price;
            if (vm.Photo != null)
            {
                hp.DeletePhoto(rc.RoomImage, "uploads");
                rc.RoomImage = hp.SavePhoto(vm.Photo, "uploads");
            }
            db.SaveChanges();

            TempData["Info"] = "Room Category updated.";
            return RedirectToAction("RoomCategory");
        }

        return View(vm);
    }

    // Room Category - Terminate | Post
    [HttpPost]
    public IActionResult TerminateRoomCategory(string? id)
    {
        var rc = db.Categories.Find(id);

        if (rc != null)
        {
            rc.Status = "Terminate";

            db.SaveChanges();

            TempData["Info"] = "Terminated.";
        }

        return RedirectToAction("RoomCategory");
    }

    // Room Category - Activate | Post
    [HttpPost]
    public IActionResult ActivateRoomCategory(string? id)
    {
        var rc = db.Categories.Find(id);

        if (rc != null)
        {
            rc.Status = "Active";

            db.SaveChanges();

            TempData["Info"] = "Activate.";
        }

        return RedirectToAction("RoomCategory");
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
