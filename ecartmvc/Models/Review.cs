using ecartmvc.Models;
using System.ComponentModel.DataAnnotations;

public class Review
{
    public int Id { get; set; }

    [Required]
    public int ProductId { get; set; }

    [Required, StringLength(50)]
    public string UserName { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [Required, StringLength(500)]
    public string Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Product Product { get; set; }
}
