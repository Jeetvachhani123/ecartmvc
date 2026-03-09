using ecartmvc.Data;
using ecartmvc.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace ecartmvc.Controllers
{
    public class OrdersController : Controller
    {
        private readonly EcartDbContext _context;

        public OrdersController(EcartDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult UpdateCart(List<CartItem> cartItems)
        {
            foreach (var item in cartItems)
            {
                var cartItem = _context.CartItems.FirstOrDefault(c => c.Id == item.Id);
                if (cartItem != null)
                {
                    cartItem.Quantity = item.Quantity;
                }
            }
            _context.SaveChanges();

            return RedirectToAction("Checkout");
        }

        [HttpPost]
        public IActionResult RemoveItem(int id)
        {
            var cartItem = _context.CartItems.FirstOrDefault(c => c.Id == id);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();
            }
            return RedirectToAction("Checkout");
        }

        // Checkout page (shows summary before placing order)
        public IActionResult Checkout()
        {
            string userId = "guest"; // later replace with session userId

            var cartItems = _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToList();

            if (!cartItems.Any())
                return RedirectToAction("Index", "Cart");

            var total = cartItems.Sum(c => c.Product.Price * c.Quantity);

            ViewBag.Total = total;
            return View(cartItems); // Checkout.cshtml
        }

        // Place Order (POST)
        [HttpPost]
        public IActionResult PlaceOrder(string customerName, string email,string phone,string address,string city,string postalCode,string paymentMethod)
        {
            string userId = "guest"; // later replace with logged-in userId/session

            var cartItems = _context.CartItems
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .ToList();

            if (!cartItems.Any())
                return RedirectToAction("Index", "Cart");

            var order = new Order
            {
                CustomerName = customerName,
                Email = email,
                Phone = phone,
                Address = address,
                City = city,
                PostalCode = postalCode,
                PaymentMethod = paymentMethod,
                OrderDate = DateTime.Now,
                Status = "Pending",
                TotalAmount = cartItems.Sum(c => c.Product.Price * c.Quantity),
                OrderItems = cartItems.Select(c => new OrderItem
                {
                    ProductId = c.ProductId,
                    Quantity = c.Quantity,
                    Price = c.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);

            // ✅ Clear cart
            _context.CartItems.RemoveRange(cartItems);
            _context.SaveChanges();

            return RedirectToAction("MyOrders");
        }

        // MyOrders (use email from session)
        public IActionResult MyOrders()
        {
            var email = HttpContext.Session.GetString("Email");
            if (string.IsNullOrEmpty(email))
            {
                var returnUrl = Url.Action("MyOrders", "Orders");
                return RedirectToAction("Login", "Account", new { returnUrl });
            }
            var orders = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.Email == email)
                .OrderByDescending(o => o.OrderDate)
                .ToList();

            return View(orders);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CancelOrder(int id)
        {
            var order = _context.Orders.FirstOrDefault(o => o.Id == id);
            if (order != null)
            {
                var status = (order.Status ?? "").Trim().ToLowerInvariant();
                if (status == "pending" || status == "waiting for payment" || status == "in process")
                {
                    order.Status = "Cancelled"; // normalized value
                    _context.SaveChanges();
                }
            }
            return RedirectToAction("MyOrders");
        }

        // Placeholders (optional)
        public IActionResult Pay(int id) => View();    // implement payment flow
        public IActionResult Track(int id) => View();  // implement tracking
        public IActionResult Review(int id) => View(); // implement review page
    }
}