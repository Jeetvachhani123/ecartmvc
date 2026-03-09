using System.ComponentModel.DataAnnotations;

namespace ecartmvc.Models
{
    public class AddReviewViewModel
    {
        [Required]
        public int ProductId { get; set; }

        [Required, Range(1, 5)]
        public int Rating { get; set; }

        [Required, StringLength(500)]
        public string Comment { get; set; }
    }
}
