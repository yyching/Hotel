using System.Reflection.Emit;
using Hotel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

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
    [Authorize]
    [Authorize(Roles = "Admin")]
    public IActionResult Profile()
    {
        var userId = User.FindFirst("UserID")?.Value;
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Dashboard", "Admin");

        // Get admin record based on UserID
        var user = db.Users.FirstOrDefault(u => u.UserID == userId);
        if (user == null) return RedirectToAction("Dashboard", "Admin");

        ViewBag.UserEmail = user.Email;
        ViewBag.UserName = user.Name;
        ViewBag.UserPhone = user.PhoneNumber;
        ViewBag.UserImage = user.UserImage;
        ViewBag.UserRole = user.Role;

        return View();
    }

    // Admin Profile - Edit
    [Authorize]
    [Authorize(Roles = "Admin")]
    public IActionResult EditProfile()
    {
        var userId = User.FindFirst("UserID")?.Value;
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Dashboard", "Admin");

        // Get admin record based on UserID
        var m = db.Users.FirstOrDefault(u => u.UserID == userId);
        if (m == null) return RedirectToAction("Dashboard", "Admin");

        var vm = new UpdateProfileVM
        {
            Email = m.Email,
            Name = m.Name,
            PhoneNumber = m.PhoneNumber,
            UserImage = m.UserImage,
        };

        return View(vm);
    }

    [HttpPost]
    [Authorize]
    [Authorize(Roles = "Admin")]
    public IActionResult EditProfile(UpdateProfileVM vm)
    {
        var userId = User.FindFirst("UserID")?.Value;
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Dashboard", "Admin");

        // Get admin record based on UserID
        var m = db.Users.FirstOrDefault(u => u.UserID == userId);
        if (m == null) return RedirectToAction("Dashboard", "Admin");

        if (vm.Photo != null)
        {
            var err = hp.ValidatePhoto(vm.Photo);
            if (err != "") ModelState.AddModelError("Photo", err);
        }

        if (ModelState.IsValid)
        {
            m.Name = vm.Name;
            m.Email = vm.Email;
            m.PhoneNumber = vm.PhoneNumber;

            if (vm.Photo != null)
            {
                // Delete old photo if it exists
                if (!string.IsNullOrEmpty(m.UserImage))
                {
                    hp.DeletePhoto(m.UserImage, "photos");
                }

                m.UserImage = hp.SavePhoto(vm.Photo, "photos");
            }

            db.SaveChanges();

            TempData["Info"] = "Profile updated.";
            return RedirectToAction();
        }

        vm.Name = m.Name;
        vm.Email = m.Email;
        vm.PhoneNumber = m.PhoneNumber;
        vm.UserImage = m.UserImage;
        return View(vm);
    }

    // Admin Profile - Change Password
    [Authorize]
    public IActionResult ChangePassword()
    {
        var userId = User.FindFirst("UserID")?.Value;
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Dashboard", "Admin");

        // Get admin record based on UserID
        var u = db.Users.FirstOrDefault(u => u.UserID == userId);
        if (u == null) return RedirectToAction("Dashboard", "Admin");

        ViewBag.userImage = u.UserImage;
        ViewBag.userName = u.Name;
        ViewBag.userRole = u.Role;

        return View();
    }

    [Authorize]
    [HttpPost]
    public IActionResult ChangePassword(UpdatePasswordVM vm)
    {
        var userId = User.FindFirst("UserID")?.Value;
        if (string.IsNullOrEmpty(userId)) return RedirectToAction("Dashboard", "Admin");

        // Get member record based on UserID
        var u = db.Users.FirstOrDefault(u => u.UserID == userId);
        if (u == null) return RedirectToAction("Dashboard", "Admin");

        // If current password not matched
        if (!hp.VerifyPassword(u.Password, vm.Current))
        {
            ModelState.AddModelError("Current", "Current Password not matched.");
        }

        if (ModelState.IsValid)
        {
            // Update user password (hash)
            u.Password = hp.HashPassword(vm.New);
            db.SaveChanges();

            TempData["Info"] = "Password updated.";
            return RedirectToAction();
        }

        ViewBag.userImage = u.UserImage;
        ViewBag.userName = u.Name;
        ViewBag.userRole = u.Role;

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
                Status          = "Active",
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
        }

        return Json(new { status = "success", message = "Terminated." });
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
        }

        return Json(new { status = "success", message = "Activated." });
    }

    // Room
    public IActionResult Rooms()
    {
        var m = db.Rooms.Include(rm => rm.Category);

        return View(m);
    }

    // Room - Add | Get
    public IActionResult _AddRoom()
    {
        var activeCategories = db.Categories
                             .Where(c => c.Status == "Active")
                             .Select(c => new
                             {
                                 c.CategoryID,
                                 DisplayText = $"{c.CategoryName} - {c.Theme}"
                             })
                             .ToList();

        ViewBag.CategoryList = new SelectList(activeCategories, "CategoryID", "DisplayText");
        return View();
    }

    // Room - Add | Post
    [HttpPost]
    public IActionResult _AddRoom(AddRoomVMs vm)
    {
        if (ModelState.IsValid)
        {
            var code = "ROM";

            // Combine to form the new ID
            string newID = hp.IDGenerator(code);

            db.Rooms.Add(new Room
            {
                RoomID = newID,
                RoomNumber = vm.RoomNumber,
                Status = vm.Status,
                CategoryID = vm.CategoryID,
            });

            db.SaveChanges();
            TempData["Info"] = "Room added successfully";
            return RedirectToAction("Rooms");

        }

        return View(vm);
    }

    // Room - Update | Get
    public IActionResult _UpdateRoom(string? id)
    {
        var rm = db.Rooms.Find(id);

        if (rm == null)
        {
            return RedirectToAction("Rooms");
        }

        var activeCategories = db.Categories
                             .Where(c => c.Status == "Active")
                             .Select(c => new
                             {
                                 c.CategoryID,
                                 DisplayText = $"{c.CategoryName} - {c.Theme}"
                             })
                             .ToList();

        ViewBag.CategoryList = new SelectList(activeCategories, "CategoryID", "DisplayText");

        var vm = new UpdateRoomVMs
        {
            RoomID = rm.RoomID,
            RoomNumber = rm.RoomNumber,
            CategoryID = rm.CategoryID,
        };
        return View(vm);
    }

    // Room - Update | Post
    [HttpPost]
    public IActionResult _UpdateRoom(UpdateRoomVMs vm)
    {
        var rm = db.Rooms.Find(vm.RoomID);

        if (ModelState.IsValid)
        {
            rm.RoomNumber = vm.RoomNumber;
            rm.CategoryID = vm.CategoryID;
            db.SaveChanges();

            TempData["Info"] = "Room updated.";
            return RedirectToAction("Rooms");
        }

        return View(vm);
    }

    // Room - Terminate | Post
    [HttpPost]
    public IActionResult TerminateRoom(string? id)
    {
        var rc = db.Rooms.Find(id);

        if (rc != null)
        {
            rc.Status = "Terminate";

            db.SaveChanges();

        }

        return Json(new { status = "success", message = "Terminated." });
    }

    // Room - Activate | Post
    [HttpPost]
    public IActionResult ActivateRoom(string? id)
    {
        var rc = db.Rooms.Find(id);

        if (rc != null)
        {
            rc.Status = "Active";

            db.SaveChanges();

        }

        return Json(new { status = "success", message = "Activated." });
    }

    // Service
    public IActionResult Services(int page = 1)
    {
        if (page < 1)
        {
            return RedirectToAction(null, new { page = 1 });
        }

        var m = db.Services.ToPagedList(page, 10);

        if (page > m.PageCount && m.PageCount > 0)
        {
            return RedirectToAction(null, new { page = m.PageCount });
        }

        if (Request.IsAjax())
        {
            return PartialView("_Services", m);
        }

        return View(m);
    }

    // Service - Get Category
    public IActionResult GetCategories(string serviceType)
    {
        List<string> categories = new List<string>();

        if (serviceType == "Room")
        {
            categories.Add("Room");
        }
        else if (serviceType == "Food")
        {
            categories.Add("Breakfast");
            categories.Add("Lunch");
            categories.Add("Dinner");
        }

        return Json(categories);
    }

    // Service - Add | Get
    public IActionResult _AddService() 
    {
        return View();
    }

    // Service - Add | Post
    [HttpPost]
    public IActionResult _AddService(AddServiceVM vm)
    {
        if (ModelState.IsValid)
        {
            var code = "SRV";

            // Combine to form the new ID
            string newID = hp.IDGenerator(code);

            db.Services.Add(new Service
            {
                ServiceID = newID,
                ServiceName = vm.serviceName,
                UnitPrice = vm.unitPrice,
                ServiceDescription = vm.serviceDescription,
                ServiceType = vm.serviceType,
                Category = vm.category,
                Status = vm.Status,
            });

            db.SaveChanges();
            TempData["Info"] = "Services added successfully";
            return RedirectToAction("Services");

        }

        return View(vm);
    }

    // Service - Update | Get
    public IActionResult _UpdateService(string? id)
    {
        var sr = db.Services.Find(id);

        if (sr == null)
        {
            return RedirectToAction("Services");
        }

        var vm = new UpdateServiceVM
        {
            serviceID = sr.ServiceID,
            serviceName = sr.ServiceName,
            unitPrice = sr.UnitPrice,
            serviceDescription = sr.ServiceDescription,
            serviceType = sr.ServiceType,
            category = sr.Category,
        };

        return View(vm);
    }

    // Service - Update | Post
    [HttpPost]
    public IActionResult _UpdateService(UpdateServiceVM vm)
    {
        var sr = db.Services.Find(vm.serviceID);

        if (ModelState.IsValid)
        {
            sr.ServiceName = vm.serviceName;
            sr.UnitPrice = vm.unitPrice;
            sr.ServiceDescription = vm.serviceDescription;
            sr.ServiceType = vm.serviceType;
            sr.Category = vm.category;
            db.SaveChanges();

            TempData["Info"] = "Service updated.";
            return RedirectToAction("Services");
        }

        return View(vm);
    }

    // Service - Terminate | Post
    [HttpPost]
    public IActionResult TerminateService(string? id)
    {
        var sr = db.Services.Find(id);

        if (sr != null)
        {
            sr.Status = "Terminate";

            db.SaveChanges();

        }

        return Json(new { status = "success", message = "Terminated." });
    }

    // Service - Activate | Post
    [HttpPost]
    public IActionResult ActivateService(string? id)
    {
        var sr = db.Services.Find(id);

        if (sr != null)
        {
            sr.Status = "Active";

            db.SaveChanges();

        }

        return Json(new { status = "success", message = "Activated." });
    }

    // User
    public IActionResult Users()
    {
        var m = db.Users.Where(u => u.Role == "Member").ToList();

        return View(m);
    }
}
