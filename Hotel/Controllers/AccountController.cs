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
                TempData["Info"] = "Login credentials not matched.";
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

        // GET: Account/UpdatePassword
        [Authorize]
        public IActionResult UpdatePassword()
        {
            var userId = User.FindFirst("UserID")?.Value;
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Dashboard", "Admin");

            // Get member record based on UserID
            var u = db.Users.FirstOrDefault(u => u.UserID == userId);
            if (u == null) return RedirectToAction("Dashboard", "Admin");

            ViewBag.userImage = u.UserImage;
            ViewBag.userName = u.Name;

            return View();
        }

        // POST: Account/UpdatePassword
        [Authorize]
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
            db.Tokens.Add(new Token
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
    }
}
