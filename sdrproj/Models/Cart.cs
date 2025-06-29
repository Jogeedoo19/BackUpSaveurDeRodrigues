using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sdrproj.Models
{
    public class Cart
    {
        [Key]
        public int CartId { get; set; }

        [Required(ErrorMessage = "User is required")]
        public string UserId { get; set; } = string.Empty; 

        [Required(ErrorMessage = "Product is required")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Count { get; set; }

        public DateTime AddedDateTime { get; set; } = DateTime.Now;

        // Navigation properties
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

      
    }
}