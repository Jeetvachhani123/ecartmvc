using ecartmvc.Data;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Security.Claims;

namespace ecartmvc.ViewComponents
{
    public class CartCountViewComponent : ViewComponent
    {
        private readonly EcartDbContext _context;

        public CartCountViewComponent(EcartDbContext context)
        {
            _context = context;
        }

        public IViewComponentResult Invoke()
        {
            // Safe null-aware check
            var principal = ViewContext?.HttpContext?.User;
            var isAuthed = principal?.Identity?.IsAuthenticated == true;

            string userId;
            if (isAuthed && principal != null)
            {
                userId = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.Identity?.Name
                    ?? "guest";
            }
            else
            {
                userId = "guest";
            }

            // total quantity, not just distinct lines
            int count = _context.CartItems
                .Where(c => c.UserId == userId)
                .Sum(c => (int?)c.Quantity) ?? 0;

            return View(count);
        }
    }
}
