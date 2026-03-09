using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ecartmvc.Models
{
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public int ProductId { get; set; }  // FK to Products

        [Required]
        public int Quantity { get; set; }

        // FK (if using custom Users table → int, if Identity → string)
        [Required]
        public string UserId { get; set; }

        // Navigation properties
        public Product Product { get; set; }

        [NotMapped] // EF won’t map this column in DB
        public decimal Total => (Product != null ? Product.Price : 0) * Quantity;
    }
}
