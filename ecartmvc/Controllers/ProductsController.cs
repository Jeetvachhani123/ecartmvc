using ecartmvc.Data;
using ecartmvc.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecartmvc.Controllers
{
    public class ProductsController : Controller
    {
        private readonly EcartDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ProductsController(EcartDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        // 🔹 List products with filters
        public IActionResult Index(string Category, string Brand, string Search, decimal? PriceRange, int? Rating)
        {
            var categories = _context.Categories.Select(c => c.Name).Distinct().ToList();

            var allProducts = _context.Products
                .Include(p => p.Category)
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Brand = p.Brand,
                    Category = p.Category != null ? p.Category.Name : "Uncategorized",
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    AverageRating = p.AverageRating
                }).ToList();

            var brands = allProducts.Select(p => p.Brand).Distinct().ToList();
            var minPrice = allProducts.Any() ? allProducts.Min(p => p.Price) : 0;
            var maxPrice = allProducts.Any() ? allProducts.Max(p => p.Price) : 0;

            var filtered = allProducts.AsQueryable();
            if (!string.IsNullOrEmpty(Category))
                filtered = filtered.Where(p => p.Category == Category);
            if (!string.IsNullOrEmpty(Brand))
                filtered = filtered.Where(p => p.Brand == Brand);
            if (!string.IsNullOrEmpty(Search))
                filtered = filtered.Where(p => p.Name.Contains(Search, StringComparison.OrdinalIgnoreCase));
            if (PriceRange.HasValue && PriceRange.Value > 0)
                filtered = filtered.Where(p => p.Price <= PriceRange.Value);
            if (Rating.HasValue && Rating.Value > 0)
                filtered = filtered.Where(p => p.AverageRating >= Rating.Value);

            var viewModel = new ProductsIndexViewModel
            {
                Products = filtered.ToList(),
                Categories = categories,
                SelectedCategory = Category,
                Brands = brands,
                SelectedBrand = Brand,
                Search = Search,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                SelectedPriceRange = PriceRange ?? maxPrice,
                SelectedRating = Rating
            };

            return View(viewModel);
        }

        // 🔹 Product details + reviews
        public IActionResult Details(int id)
        {
            var product = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
                return NotFound();

            var viewModel = new ProductViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Category = product.Category?.Name ?? "Uncategorized",
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                AverageRating = product.Reviews.Any() ? product.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = product.Reviews.Count(),
                Brand = product.Brand,
                Description = product.Description,
                Reviews = product.Reviews
                    .Select(r => new Review
                    {
                        Id = r.Id,
                        UserName = r.UserName,
                        Comment = r.Comment,
                        Rating = r.Rating,
                        CreatedAt = r.CreatedAt
                    }).ToList()
            };

            return View(viewModel);
        }

        // 🔹 Add review (only if logged in)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddReview(AddReviewViewModel model)
        {
            var sessionUser = _httpContextAccessor.HttpContext.Session.GetString("Username");
            var loggedInUser = !string.IsNullOrEmpty(sessionUser)
                ? sessionUser
                : (User.Identity != null && User.Identity.IsAuthenticated ? User.Identity.Name : null);

            if (string.IsNullOrEmpty(loggedInUser))
            {
                TempData["Error"] = "You must be logged in to leave a review.";
                return RedirectToAction("Details", new { id = model.ProductId });
            }

            if (ModelState.IsValid)
            {
                var review = new Review
                {
                    ProductId = model.ProductId,
                    UserName = loggedInUser, // Now you set it here
                    Rating = model.Rating,
                    Comment = model.Comment,
                    CreatedAt = DateTime.Now
                };

                _context.Reviews.Add(review);
                _context.SaveChanges();

                TempData["Success"] = "Review submitted successfully!";
            }
            else
            {
                TempData["Error"] = "Please fill all required fields.";
            }

            return RedirectToAction("Details", new { id = model.ProductId });
        }

        // 🔹 Search
        public IActionResult Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return RedirectToAction("Index");

            var results = _context.Products
                .Include(p => p.Category)
                .Where(p => p.Name.Contains(query) ||
                            p.Category.Name.Contains(query) ||
                            p.Brand.Contains(query))
                .Select(p => new ProductViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Brand = p.Brand,
                    Category = p.Category != null ? p.Category.Name : "Uncategorized",
                    Price = p.Price,
                    ImageUrl = p.ImageUrl,
                    AverageRating = p.AverageRating
                })
                .ToList();

            var viewModel = new ProductsIndexViewModel
            {
                Products = results,
                Search = query,
                Categories = _context.Categories.Select(c => c.Name).Distinct().ToList(),
                Brands = results.Select(r => r.Brand).Distinct().ToList(),
                MinPrice = results.Any() ? results.Min(r => r.Price) : 0,
                MaxPrice = results.Any() ? results.Max(r => r.Price) : 0
            };

            return View("Index", viewModel);
        }
    }
}
