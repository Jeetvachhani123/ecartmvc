using ecartmvc.Data;
using ecartmvc.Hubs;
using ecartmvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ecartmvc.Controllers
{
   
    public class AdminController : Controller
    {
        private readonly EcartDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AdminController(EcartDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        public async Task<IActionResult> Dashboard()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUsername")))
            {
                return RedirectToAction("AdminLogin", "Admin");
            }
            ViewData["TotalProducts"] = await _context.Products.CountAsync();
            ViewData["TotalOrders"] = await _context.Orders.CountAsync();
            ViewData["TotalUsers"] = await _context.Users.CountAsync();
            ViewData["TotalRevenue"] = await _context.Orders.SumAsync(o => o.TotalAmount);
            return View();
        }

        public IActionResult ManageOrders()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUsername")))
                return RedirectToAction("AdminLogin", "Admin");

            try
            {
                var orders = _context.Orders
                    .Include(o => o.OrderItems) // if you want to show order items later
                    .ToList();

                //  Ensure defaults for null values so the view never breaks
                foreach (var order in orders)
                {
                    order.CustomerName ??= "N/A";
                    order.Status ??= "Pending";
                    order.Email ??= "N/A";
                    order.Phone ??= "N/A";
                    order.Address ??= "N/A";
                    order.City ??= "N/A";
                    order.PostalCode ??= "N/A";

                    if (order.TotalAmount == 0) order.TotalAmount = 0; // keep safe
                    if (order.OrderDate == default) order.OrderDate = DateTime.Now;
                }

                return View(orders);
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }

        // GET: Admin/OrderDetails/
        public IActionResult OrderDetails(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order); 
        }

        // ✅ Delete Order
        [HttpPost]
        public IActionResult DeleteOrder(int id)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems) // make sure related items are loaded
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Order not found.";
                return RedirectToAction("ManageOrders");
            }

            // Remove related order items first
            if (order.OrderItems != null && order.OrderItems.Any())
            {
                _context.OrderItems.RemoveRange(order.OrderItems);
            }

            // Then remove the order
            _context.Orders.Remove(order);
            _context.SaveChanges();

            TempData["SuccessMessage"] = $"Order #{id} deleted successfully.";
            return RedirectToAction("ManageOrders");
        }

        // POST: Admin/UpdateOrderStatus
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(
    int id,
    string status,
    [FromServices] IHubContext<NotificationHub> hubContext)
        {
            var order = _context.Orders.Find(id);
            if (order == null)
                return NotFound();

            order.Status = status;
            _context.SaveChanges();

            //  Notify all clients (or later filter by user)
            await hubContext.Clients.All.SendAsync("ReceiveOrderUpdate", order.Id, status);

            TempData["SuccessMessage"] = "Order status updated successfully!";
            return RedirectToAction("ManageOrders");
        }

        // List Products
        public IActionResult ManageProducts()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUsername")))
                return RedirectToAction("AdminLogin", "Admin");

            var products = _context.Products
                .Include(p => p.Category)
                .ToList();
            return View(products);
        }

        // GET: Admin/AddProduct
        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name");
            return View();
        }

        // POST: Admin/AddProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product product, IFormFile ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
                
            }

            // Handle Image Upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(ext) || !ImageFile.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("ImageFile", "Invalid image file.");
                    ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
                    return View(product);
                }

                if (ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "File size cannot exceed 5MB.");
                    ViewBag.Categories = new SelectList(await _context.Categories.ToListAsync(), "Id", "Name", product.CategoryId);
                    return View(product);
                }

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                product.ImageUrl = $"/images/products/{fileName}";
            }
            else
            {
                product.ImageUrl = "/images/products/default.png";
            }

            //  Save brand and rating directly (already bound by model)
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("RefreshProducts");
            TempData["SuccessMessage"] = "Product added successfully!";
            return RedirectToAction("ManageProducts");
        }


        // GET: Admin/EditProduct/5
        public IActionResult EditProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
                return NotFound();

            ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", product.CategoryId);
            return View(product);
        }

        // POST: Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, IFormFile? ImageFile)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", product.CategoryId);
                //return View(product);
            }

            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null)
                return NotFound();

            // ✅ Update all fields
            existingProduct.Name = product.Name;
            existingProduct.Brand = product.Brand;                 // NEW
            existingProduct.Price = product.Price;
            existingProduct.Description = product.Description;
            existingProduct.Stock = product.Stock;
            existingProduct.CategoryId = product.CategoryId;
            existingProduct.AverageRating = product.AverageRating; // NEW

            // Handle image upload
            if (ImageFile != null && ImageFile.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var ext = Path.GetExtension(ImageFile.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(ext) || !ImageFile.ContentType.StartsWith("image/"))
                {
                    ModelState.AddModelError("ImageFile", "Invalid image file.");
                    ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", product.CategoryId);
                    return View(product);
                }

                if (ImageFile.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageFile", "File size cannot exceed 5MB.");
                    ViewBag.Categories = new SelectList(_context.Categories.ToList(), "Id", "Name", product.CategoryId);
                    return View(product);
                }

                var uploadDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "products");
                if (!Directory.Exists(uploadDir))
                    Directory.CreateDirectory(uploadDir);

                var fileName = $"{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ImageFile.CopyToAsync(stream);
                }

                // delete old image if not default
                if (!string.IsNullOrEmpty(existingProduct.ImageUrl) && !existingProduct.ImageUrl.EndsWith("default.png"))
                {
                    var oldImagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingProduct.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);
                }

                existingProduct.ImageUrl = $"/images/products/{fileName}";
            }

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();

            await _hubContext.Clients.All.SendAsync("RefreshProducts");
            TempData["SuccessMessage"] = "Product updated successfully!";
            return RedirectToAction("ManageProducts");
        }

        // Delete Product
        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null) return NotFound();

            _context.Products.Remove(product);
            _context.SaveChanges();
            return RedirectToAction("ManageProducts");
        }

        public async Task<IActionResult> ManageUsers()
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("AdminUsername")))
                return RedirectToAction("AdminLogin", "Admin");

            try
            {
                var users = await _context.Users.ToListAsync();
                return View(users);
            }
            catch (Exception ex)
            {
                return Content("Error: " + ex.Message);
            }
        }

        [HttpGet]
        public IActionResult AdminLogin()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AdminLogin(string Username, string Password)
        {
            // Simple authentication logic (replace with secure hashing in production)
            var admin = _context.Admins
                .FirstOrDefault(a => a.Username == Username && a.Password == Password);

            if (admin != null)
            {
                // Set session or authentication cookie as needed
                HttpContext.Session.SetString("AdminUsername", admin.Username);
                return RedirectToAction("Dashboard", "Admin"); // Redirect to admin dashboard
            }
            else
            {
                ViewBag.ErrorMessage = "Invalid username or password.";
                return View();
            }
        }

        public IActionResult AdminLogout()
        {
            HttpContext.Session.Remove("AdminUsername");
            return RedirectToAction("AdminLogin", "Admin");
        }

        [HttpGet]
        public IActionResult AdminRegister()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AdminRegister(string Username, string Email, string MobileNo, string Password, string ConfirmPassword)
        {
            // Basic validation
            if (Password != ConfirmPassword)
            {
                ViewBag.ErrorMessage = "Passwords do not match.";
                return View();
            }

            if (_context.Admins.Any(a => a.Username == Username))
            {
                ViewBag.ErrorMessage = "Username already exists.";
                return View();
            }

            if (_context.Admins.Any(a => a.Email == Email))
            {
                ViewBag.ErrorMessage = "Email already registered.";
                return View();
            }

            // Create and save new admin
            var admin = new Admin
            {
                Username = Username,
                Email = Email,
                MobileNo = MobileNo,
                Password = Password // In production, hash the password!
            };
            _context.Admins.Add(admin);
            _context.SaveChanges();

            ViewBag.SuccessMessage = "Registration successful! You can now log in.";
            return View();
        }
    }
}
