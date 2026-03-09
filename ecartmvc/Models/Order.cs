using System.ComponentModel.DataAnnotations;

namespace ecartmvc.Models
{
    public class Order
    {
        public int Id { get; set; }

        // Customer / Shipping Info
        [Required]
        public string CustomerName { get; set; }   // maps FullName
        [Required]
        public string Address { get; set; }
        [Required]
        public string City { get; set; }
        [Required]
        public string PostalCode { get; set; }
        [Required]
        public string Phone { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        // Order Info
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        // Payment
        [Required]
        public string PaymentMethod { get; set; }
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }
}
