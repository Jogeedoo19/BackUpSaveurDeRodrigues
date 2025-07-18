using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdrproj.Models;
using System.Linq;
using System.Threading.Tasks;

namespace sdrproj.Controllers
{
    public class MerchantOrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MerchantOrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: MerchantOrder/ViewOrders
        public async Task<IActionResult> ViewOrders()
        {
            var orders = await _context.OrderHeaders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // POST: MerchantOrder/UpdateOrderStatus/5
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            if (string.IsNullOrEmpty(status))
            {
                TempData["Error"] = "Order status is required.";
                return RedirectToAction(nameof(ViewOrders));
            }

            var order = await _context.OrderHeaders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Order #{id} status updated to {status}.";
            return RedirectToAction(nameof(ViewOrders));
        }

        // POST: MerchantOrder/MarkAsShipped/5
        [HttpPost]
        public async Task<IActionResult> MarkAsShipped(int id, string trackingNumber)
        {
            if (string.IsNullOrWhiteSpace(trackingNumber))
            {
                TempData["Error"] = "Tracking number is required to mark as shipped.";
                return RedirectToAction(nameof(ViewOrders));
            }

            var order = await _context.OrderHeaders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = "Shipped";
            order.TrackingNumber = trackingNumber;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Order #{id} marked as shipped.";
            return RedirectToAction(nameof(ViewOrders));
        }

        // POST: MerchantOrder/MarkAsDelivered/5
        [HttpPost]
        public async Task<IActionResult> MarkAsDelivered(int id)
        {
            var order = await _context.OrderHeaders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.OrderStatus = "Delivered";
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Order #{id} marked as delivered.";
            return RedirectToAction(nameof(ViewOrders));
        }
    }
}
