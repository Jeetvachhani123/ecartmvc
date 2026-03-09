using ecartmvc.Data;
using ecartmvc.Models;
using ecartmvc.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecartmvc.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        private readonly EcartDbContext _context;

        public AccountController(AuthService authService, EcartDbContext context)
        {
            _authService = authService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Login(string returnUrl = null)
        {
            ViewBag.ReturnUrl = returnUrl; 
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _authService.AuthenticateAsync(model.Username, model.Password);

            if (user == null)
            {
                ModelState.AddModelError("", "Invalid username or password");
                ViewBag.ReturnUrl = returnUrl; // keep it when login fails
                return View(model);
            }

            //  Save session
            HttpContext.Session.SetString("Username", user.Username);
            HttpContext.Session.SetString("Role", user.Role);
            HttpContext.Session.SetString("UserId", user.Id.ToString());
            HttpContext.Session.SetString("Email", user.Email); // <-- store email for orders lookup

            //  Redirect back if local URL, otherwise fallback
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (user.Role == "Admin")
                return RedirectToAction("Dashboard", "Admin");

            return RedirectToAction("Index", "Home");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View("~/Views/Account/Register.cshtml");
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Views/Account/Register.cshtml", model);

            var existingUser = _authService.GetUserByUsernameOrEmail(model.Username, model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Username or email already exists.");
                return View("~/Views/Account/Register.cshtml", model);
            }

            var user = new User
            {
                FullName = model.FullName,
                Email = model.Email,
                Username = model.Username,
                Password = model.Password, //  hash in real apps
                Role = "Customer",          // Auto assign role
                PhoneNumber = model.PhoneNumber,
                Address = model.Address,
                City = model.City
            };

            _authService.RegisterUser(user);

            TempData["Message"] = "Registration successful!";
            return RedirectToAction("Login");
        }

        // profile page
        [HttpGet]
        public IActionResult Profile()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login");

            int userId = int.Parse(userIdString);  // convert string to int

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            return View(user);
        }

        [HttpPost]
        public IActionResult UpdateProfile(User model)
        {
            if (ModelState.IsValid)
            {
                var user = _context.Users.FirstOrDefault(u => u.Id == model.Id);
                if (user != null)
                {
                    user.FullName = model.FullName;
                    user.PhoneNumber = model.PhoneNumber;
                    user.Address = model.Address;
                    user.City = model.City;

                    _context.SaveChanges();
                }
                TempData["Message"] = "Profile updated successfully!";
                return RedirectToAction("Profile");
            }
            return View("Profile", model);
        }

        [HttpPost]
        public IActionResult ChangePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (!int.TryParse(userIdString, out int userId))
            {
                return RedirectToAction("Login"); // invalid session, force re-login
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return RedirectToAction("Login");

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "New password and confirmation do not match.";
                return RedirectToAction("Profile");
            }

            if (user.Password != CurrentPassword) //  should hash/verify in real app
            {
                TempData["Error"] = "Current password is incorrect.";
                return RedirectToAction("Profile");
            }

            user.Password = NewPassword; //  should hash before saving
            _context.SaveChanges();

            TempData["Message"] = "Password updated successfully!";
            return RedirectToAction("Profile");
        }

        // GET: Account/Settings
        [HttpGet]
        public IActionResult Settings()
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString))
                return RedirectToAction("Login");

            if (!int.TryParse(userIdString, out int userId))
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            return View(user);
        }

        // POST: Account/Settings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Settings(User model, string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var userIdString = HttpContext.Session.GetString("UserId");
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                return RedirectToAction("Login");

            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //  Update profile info
            user.FullName = model.FullName;
            user.Username = model.Username;
            user.Email = model.Email;
            user.PhoneNumber = model.PhoneNumber;

            //  Change password logic
            if (!string.IsNullOrEmpty(NewPassword))
            {
                if (user.Password != CurrentPassword) //  Should hash/verify in real apps
                {
                    ModelState.AddModelError("CurrentPassword", "Current password is incorrect.");
                    return View(model);
                }

                if (NewPassword != ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "New passwords do not match.");
                    return View(model);
                }

                user.Password = NewPassword; //  Should hash before saving
            }

            _context.SaveChanges();

            ViewBag.SuccessMessage = "Settings updated successfully.";
            return View(user);
        }

    }
}
