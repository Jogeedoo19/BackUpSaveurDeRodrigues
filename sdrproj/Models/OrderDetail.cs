using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sdrproj.Models
{
    public class OrderDetail
    {
        [Key]
        public int OrderDetailId { get; set; }

        [Required]
        public int OrderHeaderId { get; set; }

        [ForeignKey("OrderHeaderId")]
        public OrderHeader OrderHeader { get; set; }

        [Required]
        public int ProductId { get; set; }

        [ForeignKey("ProductId")]
        public Product Product { get; set; }

        public int Quantity { get; set; }

        public decimal UnitPrice { get; set; }
    }
}
