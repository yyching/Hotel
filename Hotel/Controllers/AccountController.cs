using System.Net.Mail;
using System.Reflection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hotel.Models;

namespace Hotel.Controllers
{
    public class AccountController : Controller
    {
        private readonly DB db;
        private readonly IWebHostEnvironment en;
        private readonly Helper hp;

        public AccountController(DB db, IWebHostEnvironment en, Helper hp)
        {
            this.db = db;
            this.en = en;
            this.hp = hp;
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
                ModelState.AddModelError("", "Login credentials not matched.");
            }

            if (ModelState.IsValid)
            {
                TempData["Info"] = "Login successfully.";

                // (3) Sign in
                hp.SignIn(u!.Email, u.Role, vm.RememberMe, u.UserID);

                // (4) Handle return URL
                if (string.IsNullOrEmpty(returnURL))
                {
                    return RedirectToAction("Index", "Home");
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

        // GET: Account/Register
        public IActionResult Register()
        {
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
                string newUserID = "USR" + random.Next(100, 999).ToString();

                // Ensure the random UserID is unique
                while (db.Users.Any(u => u.UserID == newUserID))
                {
                    newUserID = "USR" + random.Next(100, 999).ToString();
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

            return View(vm);
        }

        // Account/Profile
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

        // POST: Account/UpdateProfile
        [Authorize(Roles = "Member")]
        [HttpPost]
        public IActionResult Profile(UpdateProfileVM vm)
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
                return RedirectToAction();
            }

            vm.Name = m.Name;
            vm.Email = m.Email;
            vm.PhoneNumber = m.PhoneNumber;
            vm.UserImage = m.UserImage;
            return View(vm);
        }

        [Authorize]
        public IActionResult UpdatePassword()
        {
            var userId = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            // Get member record based on UserID
            var m = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (m == null) return RedirectToAction("Index", "Home");

            var vm = new UpdatePasswordVM
            {
                Name = m.Name, // User's name from the database
                UserImage = m.UserImage // User's image (path) from the database
            };

            // Pass the ViewModel to the view
            return View(vm);
        }

        // POST: Account/UpdatePassword
        [Authorize]
        [HttpPost]
        public IActionResult UpdatePassword(UpdatePasswordVM vm)
        {
            var userId = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Index", "Home");

            // Get member record based on UserID
            var m = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (m == null) return RedirectToAction("Index", "Home");

            // If current password not matched
            if (!hp.VerifyPassword(m.Password, vm.Current))
            {
                ModelState.AddModelError("Current", "Current Password not matched.");
            }

            if (!ModelState.IsValid)
            {
                // Update user password (hash)
                m.Password = hp.HashPassword(vm.New);
                db.SaveChanges();

                TempData["Info"] = "Password updated.";
                return RedirectToAction();
            }

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
            var mail = new MailMessage();
            mail.To.Add(new MailAddress(u.Email, u.Name));
            mail.Subject = "Reset Password";
            mail.IsBodyHtml = true;

            var url = Url.Action("ResetPassword", "Account", null, "https");

            // Generate File Path
            string path;
            if (u.UserImage == "")
            {
                path = Path.Combine(en.WebRootPath, "img/HomePage", "LOGO.jpg");
            }
            else
            {
                // Default or other roles (adjust as necessary)
                path = Path.Combine(en.WebRootPath, "photos", u.UserImage);
            }

            var att = new Attachment(path);
            mail.Attachments.Add(att);
            att.ContentId = "photo";

            mail.Body = $@"
            <img src='cid:photo' style='width: 200px; height: 200px;
                                        border: 1px solid #333'>
            <p>Dear {u.Name},<p>
            <p>Your password has been reset to:</p>
            <p>
                Please <a href='{url}'>ResetPassword</a>
                with your new password.
            </p>
            <p>From, 🐱 Super Admin</p>
        ";

            hp.SendEmail(mail);
        }

        public IActionResult ResetPassword()
        {
            return View();
        }


    }
}
