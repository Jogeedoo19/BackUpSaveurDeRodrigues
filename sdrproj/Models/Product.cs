using System.ComponentModel.DataAnnotations;

namespace sdrproj.Models
{
    public class Product
    {
        [Key]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Price is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }

        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters")]
        public string? Description { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int Stock { get; set; }

        // Make MerchantId nullable or remove Required attribute since we set it in controller
        public int MerchantId { get; set; }

        // SubCategoryId should be required if it's a foreign key
        [Required(ErrorMessage = "Please select a subcategory")]
        public int SubCategoryId { get; set; }

        // Navigation property
        public virtual SubCategory? SubCategory { get; set; }

    
        public string ImageUrl { get; set; } = string.Empty;

        public DateTime CreatedDateTime { get; set; } = DateTime.Now;

        public string Status { get; set; } = "active";
    }
}