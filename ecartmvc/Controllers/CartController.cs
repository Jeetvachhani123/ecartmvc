using ecartmvc.Data;
using ecartmvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ecartmvc.Controllers
{
    public class CartController : Controller
    {
        private readonly EcartDbContext _context;

        public CartController(EcartDbContext context)
        {
            _context = context;
        }

        // 🔑 Helper method to get current user id
        private string GetUserId()
        {
            return User.Identity?.IsAuthenticated == true
                ? User.Identity.Name // use username or change to ClaimTypes.NameIdentifier
                : "guest";
        }

        // Show cart page
        public IActionResult Index()
        {
            string userId = GetUserId();

            var cartItems = _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToList();

            var total = cartItems.Sum(c => (c.Product?.Price ?? 0) * c.Quantity);

            ViewBag.Total = total;
            return View(cartItems);
        }

        // Add to cart
        [HttpPost]
        public IActionResult Add(int productId, int quantity = 1)
        {
            string userId = GetUserId();

            var existing = _context.CartItems
                .FirstOrDefault(c => c.UserId == userId && c.ProductId == productId);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                var product = _context.Products.Find(productId);
                if (product != null)
                {
                    var cartItem = new CartItem
                    {
                        UserId = userId,
                        ProductId = product.Id,
                        Quantity = quantity
                    };
                    _context.CartItems.Add(cartItem);
                }
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }
        public IActionResult Drawer()
        {
            string userId = GetUserId();
            var cartItems = _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToList();

            var total = cartItems.Sum(c => (c.Product?.Price ?? 0) * c.Quantity);
            ViewBag.Total = total;

            return PartialView("_CartDrawer", cartItems);
        }

        // ✅ Update cart quantities
        [HttpPost]
        public IActionResult UpdateQuantities(List<CartItem> cartItems)
        {
            foreach (var entry in cartItems)
            {
                var cartItem = _context.CartItems.Find(entry.Id);
                if (cartItem != null)
                {
                    cartItem.Quantity = entry.Quantity > 0 ? entry.Quantity : 1;
                }
            }
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        // Remove item
        public IActionResult Remove(int id)
        {
            var item = _context.CartItems.Find(id);
            if (item != null)
            {
                _context.CartItems.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }
    }
}
