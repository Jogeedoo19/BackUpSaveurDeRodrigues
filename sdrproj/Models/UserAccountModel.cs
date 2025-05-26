using System;
using System.ComponentModel.DataAnnotations;

namespace sdrproj.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Name is required")]
        [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Invalid email format")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [RegularExpression(@"^(?=.*[a-zA-Z])(?=.*\d)(?=.*[@$!%*?&]).{6,}$", ErrorMessage = "Password must contain a letter, number, and special character")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [RegularExpression(@"^\d{8}$", ErrorMessage = "Phone number must be exactly 8 digits")]
        public string Phone { get; set; }

        public string Address { get; set; } // Will be populated via API in frontend

        [Required(ErrorMessage = "Postal code is required")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Postal code must be numeric")]
        public string PostalCode { get; set; }

        public string? ProfileImagePath { get; set; }

        [Required]
        public string Status { get; set; } = "active";

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }

}
