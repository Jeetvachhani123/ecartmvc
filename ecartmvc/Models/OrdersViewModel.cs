using System.Collections.Generic;

namespace ecartmvc.Models
{
    public class OrdersViewModel
    {
        public List<CartItem> CartItems { get; set; } = new List<CartItem>();
        public List<Order> Orders { get; set; } = new List<Order>();
        public decimal CartTotal { get; set; }
    }
}
