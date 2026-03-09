using System.ComponentModel.DataAnnotations;

namespace ecartmvc.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        public string? PhoneNumber { get; set; } // Make nullable if DB allows null
        public string? Address { get; set; }
        public string? City { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        public string Role { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
