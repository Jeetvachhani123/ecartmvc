using ecartmvc.Data;
using ecartmvc.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Diagnostics;

namespace ecartmvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, EcartDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Categories.ToList();
            var products = _context.Products
                .OrderByDescending(p => p.Id) // Show latest products first
                .Take(8) // Show only a few new arrivals
                .ToList();

            ViewBag.Categories = categories;
            ViewBag.Products = products;

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private readonly EcartDbContext _context;

        

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existingUser = _context.Users.FirstOrDefault(u => u.Username == model.Username || u.Email == model.Email);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", "Username or email already exists.");
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Username = model.Username,
                    Password = model.Password // NOTE: store hashed password in real apps
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                TempData["Message"] = "Registration successful!";
                return RedirectToAction("Login");
            }

            return View(model);
        }

        // About us
        public IActionResult AboutUs()
        {
            return View();
        }

        // Contact us
        public IActionResult Contact()
        {
             return View();
        }

        [HttpGet]
        public IActionResult GetCategoriesPartial()
        {
            var categories = _context.Categories.ToList();
            return PartialView("_CategoryListPartial", categories);
        }

        [HttpGet]
        public IActionResult GetProductsPartial()
        {
            var products = _context.Products.ToList();
            return PartialView("_ProductListPartial", products);
        }
    }
}
