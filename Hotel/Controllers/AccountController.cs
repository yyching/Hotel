using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        public IActionResult Login()
        {
            return View();
        }


        public IActionResult _Login()
        {
            return View();
        }

        // POST: Account/Login
        [HttpPost]
        public IActionResult Login(LoginVM vm, string? returnURL)
        {
            // (1) Get user (admin or member) record based on email (PK)
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
                hp.SignIn(u!.Email, u.Role, vm.RememberMe);

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
                    UserImage = null
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
            // Get member record based on email (PK)
            var m = db.Users.FirstOrDefault(u => u.Email == User.Identity!.Name);
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
            // Get member record based on email (PK)
            var m = db.Users.FirstOrDefault(u => u.Email == User.Identity!.Name);
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
            return View();
        }

        // POST: Account/UpdatePassword
        [Authorize]
        [HttpPost]
        public IActionResult UpdatePassword(UpdatePasswordVM vm)
        {
            // Get user (admin or member) record based on email (PK)
            var u = db.Users.Find(User.Identity!.Name);
            if (u == null) return RedirectToAction("Index", "Home");

            // If current password not matched
            if (!hp.VerifyPassword(u.Password, vm.Current))
            {
                ModelState.AddModelError("Current", "Current Password not matched.");
            }

            if (!ModelState.IsValid)
            {
                // Update user password (hash)
                u.Password = hp.HashPassword(vm.New);
                db.SaveChanges();

                TempData["Info"] = "Password updated.";
                return RedirectToAction();
            }

            return View();
        }

        public IActionResult RPassword()
        {
            return View();
        }

        public IActionResult SEmail()
        {
            return View();
        }
    }
}
