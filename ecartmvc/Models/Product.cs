using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Http;

namespace ecartmvc.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        public required string Name { get; set; }

        [Required(ErrorMessage = "Price is required")]
        [Range(0.1, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [Display(Name = "Category")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public required Category Category { get; set; }

        public required string Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock must be a positive number")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "Brand is required")]
        public string Brand { get; set; } = string.Empty;

        // Exclude upload property from EF mapping
        [NotMapped]
        public IFormFile? ImageFile { get; set; }

        public string? ImageUrl { get; set; }

        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public double AverageRating { get; set; } = 0;
        public int ReviewCount { get; set; }
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
