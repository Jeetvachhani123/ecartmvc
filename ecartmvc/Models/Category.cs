using System.ComponentModel.DataAnnotations;

namespace ecartmvc.Models
{
    public class Category
    {
        
            public int Id { get; set; }

            [Required]
            public string Name { get; set; }
            public string IconClass { get; set; } // e.g., "bi-phone", "bi-laptop"

            // Navigation (optional)
            public ICollection<Product> Products { get; set; }
        

    }
}
