using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sdrproj.Models
{
    public class SubCategory
    {
        [Key]
        public int SubCategoryId { get; set; }

        [Required(ErrorMessage = "SubCategory name is required")]
        [MinLength(2, ErrorMessage = "SubCategory name must be at least 2 characters")]
        public string Name { get; set; }

        // FK to Category
        [Required(ErrorMessage = "Please select a category")]
        public int CategoryId { get; set; }

        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public virtual ICollection<Product> Products { get; set; }
    }
}