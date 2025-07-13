using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace sdrproj.Models
{
    public class OrderHeader
    {
        [Key]
        public int OrderHeaderId { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public int UserId { get; set; }

        [ForeignKey("UserId")]
        public User User { get; set; }

        [Required]
        public decimal TotalAmount { get; set; }

        public string OrderStatus { get; set; } = "Pending"; // Pending, Paid, Cancelled, Shipped, Delivered

        public string PaymentStatus { get; set; } = "Unpaid"; // Unpaid, Paid, Refunded

        public string? TrackingNumber { get; set; }

        public DateTime? PaymentDate { get; set; }

        public string? ReceiptUrl { get; set; }

        public ICollection<OrderDetail> OrderDetails { get; set; }

    }
}
