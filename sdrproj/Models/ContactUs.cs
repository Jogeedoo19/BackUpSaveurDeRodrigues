using System.ComponentModel.DataAnnotations;

namespace sdrproj.Models
{
    public class ContactUs
    {

        [Key]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "name is required")]
        [MinLength(2, ErrorMessage = "name must be at least 2 characters")]
        public string Name { get; set; }



        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        public string Email { get; set; }



        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Message { get; set; }


        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
