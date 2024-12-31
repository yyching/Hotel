using System.Net.Mail;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hotel.Models;
using Microsoft.EntityFrameworkCore;
using iText.Html2pdf;
using iText.Kernel.Pdf;
using Stripe;
using System;
using Microsoft.Extensions.Caching.Memory;
using X.PagedList.Extensions;

namespace Hotel.Controllers
{
    public class AccountController : Controller
    {
        private readonly DB db;
        private readonly IWebHostEnvironment en;
        private readonly Helper hp;
        private readonly IMemoryCache cache;
        private const int OTP_VALIDITY_MINUTES = 1;

        public AccountController(DB db, IWebHostEnvironment en, Helper hp, IMemoryCache cache)
        {
            this.db = db;
            this.en = en;
            this.hp = hp;
            this.cache = cache;
        }


        // GET: Account/Login
        public IActionResult Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public IActionResult Login(LoginVM vm, string? returnURL)
        {
            // (1) Get user record based on email
            var u = db.Users.FirstOrDefault(user => user.Email == vm.Email);

            // (2) Custom validation -> verify password
            if (u == null || !hp.VerifyPassword(u.Password, vm.Password))
            {
                TempData["Info"] = "Invalid Email or Password";
                return View();
            }

            if (ModelState.IsValid)
            {
                TempData["Info"] = "Login successfully.";

                // (3) Sign in
                hp.SignIn(u!.Email, u.Role, vm.RememberMe, u.UserID);

                // (4) Handle return URL
                if (string.IsNullOrEmpty(returnURL))
                {
                    switch (u.Role.ToLower())
                    {
                        case "admin":
                            return RedirectToAction("Dashboard", "Admin");
                        case "member":
                            return RedirectToAction("Index", "Home");
                        default:
                            return RedirectToAction("Index", "Home");
                    }
                }
            }

            return View(vm);
        }

        // GET: Account/Logout
        public IActionResult Logout(string? returnURL)
        {
            TempData["Info"] = "Logout successfully.";

            // Sign out
            hp.SignOut();

            return RedirectToAction("Index", "Home");
        }

        // GET: Account/CheckEmail
        public bool CheckEmail(string email)
        {
            return !db.Users.Any(u => u.Email == email);
        }

        // GET: Account/SendOtp
        public IActionResult SendOtp(string? email)
        {
            return View();
        }

        // POST: Account/SendOtp
        [HttpPost]
        public IActionResult SendOTP(OptVM model)
        {
            if (string.IsNullOrEmpty(model.Email))
            {
                TempData["Info"] = "Please enter an email address";
                return View();
            }

            try
            {
                // Generate OTP
                Random random = new Random();
                string otp = random.Next(100000, 999999).ToString();

                // Save OTP to cache with 5 minutes expiration
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(OTP_VALIDITY_MINUTES));

                cache.Set($"OTP_{model.Email}", otp, cacheOptions);

                // Create and send email
                var mail = new MailMessage();
                mail.To.Add(new MailAddress(model.Email));
                mail.Subject = "Verification Code - Account Registration";
                mail.IsBodyHtml = true;
                mail.Body = $@"
                <html>
                <body style='font-family: Arial, sans-serif;'>
                    <div style='padding: 20px; background-color: #f5f5f5;'>
                        <h2 style='color: #333;'>Verify Your Email</h2>
                        <p>Your verification code is:</p>
                        <h1 style='color: #007bff; font-size: 32px; letter-spacing: 5px;'>{otp}</h1>
                        <p>This code will expire in {OTP_VALIDITY_MINUTES} minutes.</p>
                        <p style='color: #666; font-size: 12px;'>If you didn't request this code, please ignore this email.</p>
                    </div>
                </body>
                </html>";

                hp.SendEmail(mail);

                var email = model.Email;

                TempData["Info"] = "OTP send to your email";
                return RedirectToAction("VerifyOtp", new { Email = email});
            }
            catch (Exception ex)
            {
                TempData["Info"] = "Failed to send verification code. Please try again.";
                return View();
            }
        }

        // GET: Account/VerifyOtp
        public IActionResult VerifyOtp(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // POST: Account/VerifyOtp
        [HttpPost]
        public IActionResult VerifyOTP(OptVM model)
        {
            try
            {
                var email = model.Email;

                // get the opt form the memory
                if (!cache.TryGetValue($"OTP_{model.Email}", out string storedOTP))
                {
                    TempData["Info"] = "Verification code has expired. Please request a new one.";
                    return RedirectToAction("SendOtp", new { Email = email});
                }

                // verify opt
                if (model.Otp != storedOTP)
                {
                    TempData["Info"] = "Invalid verification code";
                    ViewBag.Email = email;
                    return View();
                }

                // remove the opt if success
                cache.Remove($"OTP_{model.Email}");

                TempData["Info"] = "Email verified successfully";
                return RedirectToAction("Register", new { Email = email });
            }
            catch (Exception ex)
            {
                TempData["Info"] = "An error occurred during verification";
                ViewBag.Email = model.Email;
                return View();
            }
        }

        // GET: Account/Register
        public IActionResult Register(string email)
        {
            ViewBag.Email = email;
            return View();
        }

        // POST: Account/Register
        [HttpPost]
        public IActionResult Register(RegisterVM vm)
        {
            if (ModelState.IsValid("Email") &&
                db.Users.Any(u => u.Email == vm.Email))
            {
                ModelState.AddModelError("Email", "Duplicated Email.");
            }

            if (ModelState.IsValid)
            {
                // Generate random UserID in the format USR000
                var random = new Random();
                var currentDateTime = DateTime.Now;
                string newUserID = $"USR{random.Next(100, 999)}{currentDateTime:yyyyMMdd}";

                // Ensure the random UserID is unique
                while (db.Users.Any(u => u.UserID == newUserID))
                {
                    newUserID = $"USR{random.Next(100, 999)}{currentDateTime:yyyyMMdd}";
                }

                // Insert new member
                db.Users.Add(new()
                {
                    UserID = newUserID,
                    Name = vm.Name,
                    Email = vm.Email,
                    Password = hp.HashPassword(vm.Password),
                    PhoneNumber = vm.PhoneNumber,
                    Role = "Member",
                    Status = "Active",
                    UserImage = ""
                });

                db.SaveChanges();

                TempData["Info"] = "Register successfully. Please login.";
                return RedirectToAction("Login");
            }

            ViewBag.Email = vm.Email;
            return View(vm);
        }

        // GET: Account/Profile
        [Authorize]
        [Authorize(Roles = "Member")]
        public IActionResult Profile()
        {
            var userId = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            // Get member record based on UserID
            var m = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (m == null) return RedirectToAction("Index", "Home");

            var vm = new UpdateProfileVM
            {
                Email = m.Email,
                Name = m.Name,
                PhoneNumber = m.PhoneNumber,
                UserImage = m.UserImage,
            };

            return View(vm);
        }

        // GET: Account/EditProfile
        [Authorize]
        [Authorize(Roles = "Member")]
        public IActionResult EditProfile()
        {
            var userId = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            // Get member record based on UserID
            var m = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (m == null) return RedirectToAction("Index", "Home");

            var vm = new UpdateProfileVM
            {
                Email = m.Email,
                Name = m.Name,
                PhoneNumber = m.PhoneNumber,
                UserImage = m.UserImage,
            };

            return View(vm);
        }

        // POST: Account/EditProfile
        [Authorize(Roles = "Member")]
        [HttpPost]
        public IActionResult EditProfile(UpdateProfileVM vm)
        {
            var userId = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            // Get member record based on UserID
            var m = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (m == null) return RedirectToAction("Index", "Home");

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
                return RedirectToAction("Profile", "Account");
            }

            vm.Name = m.Name;
            vm.Email = m.Email;
            vm.PhoneNumber = m.PhoneNumber;
            vm.UserImage = m.UserImage;
            return View(vm);
        }

        // GET: Account/UpdatePassword
        [Authorize]
        [Authorize(Roles = "Member")]
        public IActionResult UpdatePassword()
        {
            var userId = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            // Get member record based on UserID
            var u = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (u == null) return RedirectToAction("Index", "Home");

            ViewBag.userImage = u.UserImage;
            ViewBag.userName = u.Name;

            return View();
        }

        // POST: Account/UpdatePassword
        [Authorize]
        [Authorize(Roles = "Member")]
        [HttpPost]
        public IActionResult UpdatePassword(UpdatePasswordVM vm)
        {
            var userId = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            // Get member record based on UserID
            var u = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (u == null) return RedirectToAction("Index", "Home");

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

            return View();
        }

        // GET Account/SendEmail
        public IActionResult SendEmail()
        {
            return View();
        }

        // POST Account/SendEmail
        [HttpPost]
        public IActionResult SendEmail(SendEmailVM vm)
        {
            var u = db.Users.SingleOrDefault(user => user.Email == vm.Email);

            if (u == null)
            {
                ModelState.AddModelError("Email", "Email not found.");
            }

            if (ModelState.IsValid)
            {
                // Send reset password email
                SendResetPasswordEmail(u);

                TempData["Info"] = $"Password reset. Check your email.";
                return RedirectToAction();
            }

            return View();
        }

        private void SendResetPasswordEmail(User u)
        {
            // Generate a unique token
            string token = Guid.NewGuid().ToString();

            // Save the token in the database
            var tokenExpireTime = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour
            db.Tokens.Add(new Models.Token
            {
                Id = token,
                UserID = u.UserID,
                Expire = tokenExpireTime
            });
            db.SaveChanges();

            // Generate the reset URL with the token
            var resetUrl = Url.Action("ResetPassword", "Account", new { token }, Request.Scheme);

            // Create the email message
            var mail = new MailMessage();
            mail.To.Add(new MailAddress(u.Email, u.Name));
            mail.Subject = "Reset Your Password";
            mail.IsBodyHtml = true;
            mail.Body = $@"
            <p>Dear {u.Name},</p>
            <p>You requested to reset your password. Please click the link below to reset your password:</p>
            <p><a href='{resetUrl}'>Reset Password</a></p>
            <p>If you did not request this, please ignore this email.</p>
            <p>Best regards,<br>EASYSTAYS HOTEL</p>
            ";

            // Send the email
            hp.SendEmail(mail);
        }

        // GET: Account/ResetPassword
        public IActionResult ResetPassword(string token)
        {
            var tokenEntry = db.Tokens.SingleOrDefault(t => t.Id == token);

            if (tokenEntry == null || tokenEntry.Expire < DateTime.UtcNow)
            {
                // Token is invalid or expired
                TempData["Info"] = $"Invalid or expired token.";
                return RedirectToAction("SendEmail");
            }

            // Show the reset password form
            return View(new ResetPasswordVM { Token = token });
        }

        // POST: Account/ResetPassword
        [HttpPost]
        public IActionResult ResetPassword(ResetPasswordVM vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var tokenEntry = db.Tokens.SingleOrDefault(t => t.Id == vm.Token);

            if (tokenEntry == null || tokenEntry.Expire < DateTime.UtcNow)
            {
                // Token is invalid or expired
                return BadRequest("Invalid or expired token.");
            }

            // Find the user by the token's UserID
            var user = db.Users.SingleOrDefault(u => u.UserID == tokenEntry.UserID);

            if (user != null)
            {
                // Update the user's password
                user.Password = hp.HashPassword(vm.NewPassword);
                db.SaveChanges();

                // Remove the used token
                db.Tokens.Remove(tokenEntry);
                db.SaveChanges();
            }

            return RedirectToAction("Login");
        }

        // GET: BookingHistory
        [Authorize]
        [Authorize(Roles = "Member")]
        public IActionResult BookingHistory(int page = 1)
        {
            var userId = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            var bookings = db.Bookings
                .Include(b => b.Room)
                    .ThenInclude(r => r.Category)
                .Where(b => b.UserID == userId)
                .OrderByDescending(b => b.BookingDate)
                .ToList();

            // For the paging
            int pageSize = 5;
            var pagedBookings = bookings.ToPagedList(page, pageSize);

            var serviceBookings = db.ServiceBooking
                .Include(s => s.Service)
                .ToList();

            var breakfastDict = new Dictionary<string, Dictionary<string, int>>();
            var lunchDict = new Dictionary<string, Dictionary<string, int>>();
            var dinnerDict = new Dictionary<string, Dictionary<string, int>>();
            var roomDict = new Dictionary<string, Dictionary<string, int>>();

            foreach (var booking in bookings)
            {
                var services = serviceBookings
                    .Where(s => s.BookingID == booking.BookingID)
                    .ToList();

                if (services.Any())
                {
                    var breakfastServices = services
                    .Where(s => s.Service.ServiceType == "Food" && s.Service.Category == "Breakfast")
                    .ToDictionary(s => s.Service.ServiceName, s => s.Qty);
                    if (breakfastServices.Any())
                    {
                        breakfastDict[booking.BookingID] = breakfastServices;
                    }

                    var lunchServices = services
                    .Where(s => s.Service.ServiceType == "Food" && s.Service.Category == "Lunch")
                    .ToDictionary(s => s.Service.ServiceName, s => s.Qty);
                    if (lunchServices.Any())
                    {
                        lunchDict[booking.BookingID] = lunchServices;
                    }

                    var dinnerServices = services
                    .Where(s => s.Service.ServiceType == "Food" && s.Service.Category == "Dinner")
                    .ToDictionary(s => s.Service.ServiceName, s => s.Qty);
                    if (dinnerServices.Any())
                    {
                        dinnerDict[booking.BookingID] = dinnerServices;
                    }

                    var roomServices = services
                        .Where(s => s.Service.ServiceType == "Room" && s.Qty >= 1)
                        .ToDictionary(s => s.Service.ServiceName, s => s.Qty);
                    if (roomServices.Any())
                    {
                        roomDict[booking.BookingID] = roomServices;
                    }
                }
            }

            ViewBag.bookingCount = bookings.Count;

            var categoryImages = db.CategoryImages
                           .Include(r => r.Category)
                           .ToList();

            var categoryImageDict = categoryImages
                                    .GroupBy(ci => ci.CategoryID)
                                    .Select(g => g.First())
                                    .ToDictionary(ci => ci.CategoryID, ci => ci.ImagePath);

            // Get the category Image
            ViewBag.categoryImage = categoryImageDict;

            ViewBag.Bookings = pagedBookings;
            ViewBag.BreakfastServices = breakfastDict;
            ViewBag.LunchServices = lunchDict;
            ViewBag.DinnerServices = dinnerDict;
            ViewBag.RoomServices = roomDict;

            if (Request.IsAjax())
            {
                return PartialView("History", pagedBookings);
            }

            return View(pagedBookings);
        }

        // POST: Generate receipt pdf
        [Authorize]
        [Authorize(Roles = "Member")]
        [HttpPost]
        public IActionResult BookingHistory(string bookingID)
        {
            var booking = db.Bookings
                            .Include(b => b.Room)
                            .Include(b => b.Room.Category)
                            .Where(b => b.BookingID == bookingID)
                            .FirstOrDefault();

            if (booking == null)
            {
                TempData["Info"] = $"Booking {bookingID} not found";
                return RedirectToAction("BookingHistory", "Account");
            }

            DateTime bookingDate = booking.BookingDate;
            DateOnly checkInDate = booking.CheckInDate;
            DateOnly checkOutDate = booking.CheckOutDate;
            var roomNumber = booking.Room.RoomNumber;
            var pricePerNight = booking.Room.Category.PricePerNight;

            int numberOfDays = checkOutDate.DayNumber - checkInDate.DayNumber;
            double roomTotalPrice = booking.Room.Category.PricePerNight * numberOfDays;
            double serviceSubtotal = 0;

            var allServices = new List<ServiceItem>();
            if (!string.IsNullOrEmpty(booking.BookingID))
            {
                var services = db.ServiceBooking
                    .Where(sb => sb.BookingID == booking.BookingID)
                    .Include(sb => sb.Service)
                    .Select(sb => new ServiceItem
                    {
                        serviceName = sb.Service.ServiceName,
                        category = sb.Service.Category,
                        quantity = sb.Qty,
                        price = sb.Service.UnitPrice
                    })
                    .ToList();

                allServices.AddRange(services);
                serviceSubtotal = services.Sum(s => s.price * s.quantity);
            }

            double subtotal = roomTotalPrice + serviceSubtotal;
            double tax = Math.Round(subtotal * 0.1, 2);
            double total = booking.TotalAmount;

            try
            {
                string receiptHtml = ReceiptTemplate.GenerateHtml(
                    bookingID,
                    bookingDate,
                    checkInDate,
                    checkOutDate,
                    roomNumber,
                    pricePerNight,
                    allServices,
                    subtotal,
                    tax,
                    total
                );

                byte[] pdfBytes = GenerateReceiptPdf(receiptHtml);

                return File(pdfBytes, "application/pdf", $"Receipt-{bookingID}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Info"] = "Cant generate pdf";
                return View();
            }
        }

        private byte[] GenerateReceiptPdf(string htmlContent)
        {
            byte[] pdfBytes;
            using (var memoryStream = new MemoryStream())
            {
                // create the convertor setting
                ConverterProperties properties = new ConverterProperties();

                // create pdf
                using (var writer = new PdfWriter(memoryStream))
                using (var pdf = new PdfDocument(writer))
                {
                    // pdf setting
                    pdf.SetTagged();
                    pdf.SetDefaultPageSize(iText.Kernel.Geom.PageSize.A4);

                    // HTML to PDF
                    HtmlConverter.ConvertToPdf(htmlContent, pdf, properties);
                }

                pdfBytes = memoryStream.ToArray();
            }
            return pdfBytes;
        }
    }
}
