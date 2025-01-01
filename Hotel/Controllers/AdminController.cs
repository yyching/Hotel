using System.Globalization;
using System.Reflection.Emit;
using Azure;
using Hotel.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
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

    // =======================================================================================================================================
    // Admin Profile                        ==================================================================================================
    // =======================================================================================================================================
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

    // Admin Profile - Edit | Get
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

    // Admin Profile - Edit | Post
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

            return RedirectToAction("Profile");
        }

        vm.Name = m.Name;
        vm.Email = m.Email;
        vm.PhoneNumber = m.PhoneNumber;
        vm.UserImage = m.UserImage;
        return View(vm);
    }

    // Admin Profile - Change Password | Get
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

    // Admin Profile - Change Password | Post
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

            return RedirectToAction("Profile");
        }

        ViewBag.userImage = u.UserImage;
        ViewBag.userName = u.Name;
        ViewBag.userRole = u.Role;

        return View();
    }

    // =======================================================================================================================================

    // =======================================================================================================================================
    // Room Category                        ==================================================================================================
    // =======================================================================================================================================
    public IActionResult RoomCategory(string? name, int page = 1)
    {
        // Searching ------------------------
        ViewBag.Name = name = name?.Trim() ?? "";

        var searched = db.Categories
                .Where(rc => rc.CategoryName.Contains(name));

        // Paging ---------------------------
        if (page < 1)
        {
            return RedirectToAction(null, new { name, page = 1 });
        }

        var m = searched.ToPagedList(page, 10);

        if (page > m.PageCount && m.PageCount > 0)
        {
            return RedirectToAction(null, new { name, page = m.PageCount });
        }

        if (Request.IsAjax())
        {
            return PartialView("_RoomCategory", m);
        }

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
        if (vm.Photos != null && vm.Photos.Any())
        {
            foreach (var photo in vm.Photos)
            {
                var err = hp.ValidatePhoto(photo); // Assuming `ValidatePhoto` checks the file
                if (!string.IsNullOrEmpty(err))
                {
                    ModelState.AddModelError("Photos", err);
                    break;
                }
            }
        }

        if (ModelState.IsValid)
        {
            // Handle category creation logic
            var random = new Random();
            string randomPart = random.Next(100, 999).ToString();
            string timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
            string newID = $"RCT{randomPart}{timestamp}";

            var category = new Category
            {
                CategoryID = newID,
                CategoryName = vm.categoryName,
                Theme = vm.theme,
                Size = vm.size,
                Capacity = vm.capacity,
                Bed = vm.bed,
                Description = vm.description,
                PricePerNight = vm.price,
                Status = "Active",
            };

            db.Categories.Add(category);
            db.SaveChanges();

            // Process and Save Photos
            if (vm.Photos != null && vm.Photos.Any())
            {
                foreach (var photo in vm.Photos)
                {
                    var savedFileName = hp.SavePhoto(photo, "uploads");

                    var categoryImage = new CategoryImage
                    {
                        ImagePath = savedFileName,
                        CategoryID = newID,
                    };

                    db.CategoryImages.Add(categoryImage);
                    db.SaveChanges();
                }
            }

            TempData["Info"] = "Room Category added successfully with images.";
            return RedirectToAction("RoomCategory");
        }

        return View();
    }

    // Room Category - Update | Get
    public IActionResult UpdateRoomCategory(string? id)
    {
        var rc = db.Categories
                   .Include(c => c.CategoryImages)  // Ensure CategoryImages are included
                   .FirstOrDefault(c => c.CategoryID == id);

        if (rc == null)
        {
            return RedirectToAction("RoomCategory");
        }

        var vm = new UpdateRoomCategoryVM
        {
            categoryID = rc.CategoryID,
            categoryName = rc.CategoryName,
            theme = rc.Theme,
            size = rc.Size,
            capacity = rc.Capacity,
            bed = rc.Bed,
            description = rc.Description,
            price = rc.PricePerNight,
            CategoryImages = rc.CategoryImages,
        };

        return View(vm);
    }

    // Room Category - Update | Post
    [HttpPost]
    public IActionResult UpdateRoomCategory(UpdateRoomCategoryVM vm)
    {
        var rc = db.Categories.Find(vm.categoryID);

        if (rc == null)
        {
            TempData["Error"] = "Room category not found.";
            return RedirectToAction("RoomCategory");
        }

        if (ModelState.IsValid)
        {
            // Update category details
            rc.CategoryName = vm.categoryName;
            rc.Theme = vm.theme;
            rc.Size = vm.size;
            rc.Capacity = vm.capacity;
            rc.Bed = vm.bed;
            rc.Description = vm.description;
            rc.PricePerNight = vm.price;

            // Save updated category details
            db.SaveChanges();

            // Process Images
            if (vm.RemoveImagePaths != null && vm.RemoveImagePaths.Any())
            {
                // Remove selected images (both files and database records)
                foreach (var imagePath in vm.RemoveImagePaths)
                {
                    var existingImage = db.CategoryImages.FirstOrDefault(ci => ci.ImagePath == imagePath && ci.CategoryID == rc.CategoryID);
                    if (existingImage != null)
                    {
                        // Delete the file from the server
                        hp.DeletePhoto(existingImage.ImagePath, "uploads");

                        // Remove the record from the database
                        db.CategoryImages.Remove(existingImage);
                    }
                }

                // Save changes after removing images
                db.SaveChanges();
            }

            // Insert new images
            if (vm.Photos != null && vm.Photos.Any())
            {
                foreach (var photo in vm.Photos)
                {
                    // Save the new photo file to the server
                    var savedFileName = hp.SavePhoto(photo, "uploads");

                    // Add the new photo record to the database
                    var categoryImage = new CategoryImage
                    {
                        ImagePath = savedFileName,
                        CategoryID = rc.CategoryID
                    };
                    db.CategoryImages.Add(categoryImage);
                }

                // Save changes after adding new images
                db.SaveChanges();
            }

            TempData["Info"] = "Room Category updated successfully with updated images.";
            return RedirectToAction("RoomCategory");
        }

        // Reload existing images in case of an error
        vm.CategoryImages = db.CategoryImages.Where(ci => ci.CategoryID == rc.CategoryID).ToList();
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

        TempData["Info"] = "Room Category terminated.";
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
        }

        TempData["Info"] = "Room Category activated.";
        return RedirectToAction("RoomCategory");
    }

    //Import - RoomCategory Function
    [HttpPost]
    public IActionResult Import_RoomCategory(IFormFile file)
    {
        if (file != null
            && file.FileName.EndsWith(".txt")
            && file.ContentType == "text/plain")
        {
            int n = ImportRoomCategory(file);
            ImportRoomCategoryImage(file);
            TempData["Info"] = $"{n} Room Category imported.";
        }

        return RedirectToAction("RoomCategory");
    }

    //Import - RoomCategory Logic
    private int ImportRoomCategory(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        int insertedCount = 0;

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrEmpty(line)) continue;

            var data = line.Split("\t", StringSplitOptions.TrimEntries);

            try
            {
                if (data.Length == 9) // Process category data
                {
                    var existingCategory = db.Categories.FirstOrDefault(c => c.CategoryID == data[0]);
                    if (existingCategory != null)
                    {
                        Console.WriteLine($"Category with ID {data[0]} already exists. Skipping.");
                        continue;
                    }

                    var category = new Category
                    {
                        CategoryID = data[0],
                        CategoryName = data[1],
                        Theme = data[2],
                        Size = int.Parse(data[3]),
                        Capacity = data[4],
                        Bed = data[5],
                        Description = data[6],
                        PricePerNight = double.Parse(data[7]),
                        Status = data[8],
                    };

                    db.Categories.Add(category);
                    insertedCount++;

                }
                else
                {
                    Console.WriteLine($"Invalid data format: {line}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing line: {line}. Error: {ex.Message}");
            }
        }

        return db.SaveChanges();
    }

    //Import - RoomCategory Image Logic
    private int ImportRoomCategoryImage(IFormFile file)
    {
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        int insertedCount = 0;

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrEmpty(line)) continue;

            var data = line.Split("\t", StringSplitOptions.TrimEntries);

            try
            {
                if (data.Length == 2) // Process category data
                {
                    var imagePath = data[0];  // Image path
                    var categoryId = data[1];  // Category ID

                    // Check if the category exists for the image
                    var existingCategory = db.Categories.FirstOrDefault(c => c.CategoryID == categoryId);
                    if (existingCategory == null)
                    {
                        Console.WriteLine($"Category with ID {categoryId} does not exist. Skipping image.");
                        continue;
                    }

                    // Check if the image already exists
                    var existingImage = db.CategoryImages.FirstOrDefault(ci => ci.ImagePath == imagePath && ci.CategoryID == categoryId);
                    if (existingImage != null)
                    {
                        Console.WriteLine($"Image with path {imagePath} already exists for Category {categoryId}. Skipping.");
                        continue;
                    }

                    // Create and add the new image (let the database auto-generate the ImageID)
                    var image = new CategoryImage
                    {
                        ImagePath = imagePath,
                        CategoryID = categoryId,
                    };
                    db.CategoryImages.Add(image);
                }
                else
                {
                    Console.WriteLine($"Invalid data format: {line}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing line: {line}. Error: {ex.Message}");
            }
        }

        return db.SaveChanges();
    }

    // =======================================================================================================================================

    // =======================================================================================================================================
    // Room                                 ==================================================================================================
    // =======================================================================================================================================
    public IActionResult Rooms(string? name, int page = 1)
    {
        // Searching ------------------------
        ViewBag.Name = name = name?.Trim() ?? "";

        var searched = db.Rooms
                       .Include(rm => rm.Category)
                       .Where(rm => rm.RoomNumber.Contains(name) || rm.Category.CategoryName.Contains(name));

        // Paging ---------------------------
        if (page < 1)
        {
            return RedirectToAction(null, new { name, page = 1 });
        }

        var m = searched.ToPagedList(page, 10);

        if (page > m.PageCount && m.PageCount > 0)
        {
            return RedirectToAction(null, new { name, page = m.PageCount });
        }

        if (Request.IsAjax())
        {
            return PartialView("_Room", m);
        }

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

        TempData["Info"] = "Room terminated.";
        return RedirectToAction("Rooms");
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

        TempData["Info"] = "Room terminated.";
        return RedirectToAction("Rooms");
    }

    // =======================================================================================================================================

    // =======================================================================================================================================
    // Service                              ==================================================================================================
    // =======================================================================================================================================
    public IActionResult Services(string? name, int page = 1)
    {
        // (1) Searching ------------------------
        ViewBag.Name = name = name?.Trim() ?? "";

        var searched = db.Services.Where(s => s.ServiceName.Contains(name));

        // (2) Paging ---------------------------
        if (page < 1)
        {
            return RedirectToAction(null, new { name, page = 1 });
        }

        var m = searched.ToPagedList(page, 10);

        if (page > m.PageCount && m.PageCount > 0)
        {
            return RedirectToAction(null, new { name, page = m.PageCount });
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

        TempData["Info"] = "Service terminated.";
        return RedirectToAction("Services");
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

        TempData["Info"] = "Service activated.";
        return RedirectToAction("Services");
    }

    //Import - Service Function
    [HttpPost]
    public IActionResult Import_Service(IFormFile file)
    {
        if (file != null
            && file.FileName.EndsWith(".txt")
            && file.ContentType == "text/plain")
        {
            int n = ImportService(file);
            TempData["Info"] = $"{n} services imported.";
        }

        return RedirectToAction("Services");
    }

    //Import - Service Logic
    private int ImportService(IFormFile file)
    {
        // Read from uploaded file --> import services
        // Return the number of new services inserted
        using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        int insertedCount = 0;

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine()?.Trim() ?? "";

            if (string.IsNullOrEmpty(line)) continue;

            var data = line.Split("\t", StringSplitOptions.TrimEntries);

            try
            {
                // Check if a service with the same ServiceID already exists
                var existingService = db.Services.FirstOrDefault(s => s.ServiceID == data[0]);
                if (existingService != null)
                {
                    Console.WriteLine($"Service with ID {data[0]} already exists. Skipping.");
                    continue; // Skip to the next line if the service already exists
                }

                // Attempt to parse the price to a double
                if (double.TryParse(data[2], out var price))
                {
                    db.Services.Add(new Service
                    {
                        ServiceID = data[0],
                        ServiceName = data[1],
                        UnitPrice = price,
                        ServiceDescription = data[3],
                        ServiceType = data[4],
                        Category = data[5],
                        Status = data[6]
                    });

                    insertedCount++; // Increment the count of inserted services
                }
                else
                {
                    Console.WriteLine($"Invalid price format for service: {data[0]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing line: {line}. Error: {ex.Message}");
            }
        }

        return db.SaveChanges(); // Returns the number of records saved to the database
    }

    // =======================================================================================================================================

    // =======================================================================================================================================
    // Booking                              ==================================================================================================
    // =======================================================================================================================================
    public IActionResult Bookings(string? name, int page = 1)
    {
        // (1) Searching ------------------------
        ViewBag.Name = name = name?.Trim() ?? "";

        var searched = db.Bookings.Include(b => b.User).Where(b => b.BookingID.Contains(name));

        // (2) Paging ---------------------------
        if (page < 1)
        {
            return RedirectToAction(null, new { name, page = 1 });
        }
        var m = searched.ToPagedList(page, 10);

        if (page > m.PageCount && m.PageCount > 0)
        {
            return RedirectToAction(null, new { name, page = m.PageCount });
        }

        if (Request.IsAjax())
        {
            return PartialView("_Bookings", m);
        }

        return View(m);
    }

    public IActionResult BookingDetail(string? id)
    {
        // Fetch the booking based on BookingID
        var bd = db.Bookings.Find(id);

        if (bd == null)
        {
            return RedirectToAction("Services");
        }

        var user = db.Users.Find(bd.UserID);  
        var room = db.Rooms.Find(bd.RoomID);  

        // Prepare the view model to pass to the view
        var bookingDetail = new ViewBookingDetail
        {
            bookingID = bd.BookingID,
            bookingDate = bd.BookingDate.ToString("yyyy-MM-dd"),  
            checkInDate = bd.CheckInDate.ToString("yyyy-MM-dd"),  
            checkOutDate = bd.CheckOutDate.ToString("yyyy-MM-dd"), 
            totalAmount = bd.TotalAmount.ToString("C"),  
            userName = user?.Name,
            roomNumber = room?.RoomNumber
        };

        // Pass the model to the view
        return View(bookingDetail);
    }


    // =======================================================================================================================================

    // =======================================================================================================================================
    // User                                 ==================================================================================================
    // =======================================================================================================================================
    public IActionResult Users(string? name, int page = 1)
    {
        // Searching ------------------------
        ViewBag.Name = name = name?.Trim() ?? "";

        var searched = db.Users
                .Where(u => u.Role == "Member" && u.Name.Contains(name));

        // Paging ---------------------------
        if (page < 1)
        {
            return RedirectToAction(null, new { name, page = 1 });
        }

        var m = searched.ToPagedList(page, 10);

        if (page > m.PageCount && m.PageCount > 0)
        {
            return RedirectToAction(null, new { name, page = m.PageCount });
        }

        if (Request.IsAjax())
        {
            return PartialView("_User", m);
        }

        return View(m);
    }

    // User - Terminate | Post
    [HttpPost]
    public IActionResult TerminateUser(string? id)
    {
        var u = db.Users.Find(id);

        if (u != null)
        {
            u.Status = "Terminate";
            db.SaveChanges();
        }

        TempData["Info"] = "User terminated.";
        return RedirectToAction("Users");
    }

    // User - Activate | Post
    [HttpPost]
    public IActionResult ActivateUser(string? id)
    {
        var u = db.Users.Find(id);

        if (u != null)
        {
            u.Status = "Active";
            db.SaveChanges();
        }

        TempData["Info"] = "User activated.";
        return RedirectToAction("Users");
    }

    // =======================================================================================================================================

}
