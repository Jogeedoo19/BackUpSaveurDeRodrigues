using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace sdrproj.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context; 

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .OrderByDescending(u => u.CreatedAt)
                .ToListAsync();
            return View("UserManagement", users);
        }

        // GET: Admin/Merchants  
        public async Task<IActionResult> Merchants()
        {
            var merchants = await _context.Merchants
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();
            return View("MerchantManagement", merchants);
        }

        // POST: Admin/BlockUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Users));
                }

                user.Status = "blocked";
                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"User '{user.Name}' has been blocked successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while blocking the user.";
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/UnblockUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockUser(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Users));
                }

                user.Status = "active";
                _context.Update(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"User '{user.Name}' has been unblocked successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while unblocking the user.";
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/BlockMerchant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BlockMerchant(int id)
        {
            try
            {
                var merchant = await _context.Merchants.FindAsync(id);
                if (merchant == null)
                {
                    TempData["Error"] = "Merchant not found.";
                    return RedirectToAction(nameof(Merchants));
                }

                merchant.Status = "blocked";
                _context.Update(merchant);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Merchant '{merchant.Name}' has been blocked successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while blocking the merchant.";
            }

            return RedirectToAction(nameof(Merchants));
        }

        // POST: Admin/UnblockMerchant
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnblockMerchant(int id)
        {
            try
            {
                var merchant = await _context.Merchants.FindAsync(id);
                if (merchant == null)
                {
                    TempData["Error"] = "Merchant not found.";
                    return RedirectToAction(nameof(Merchants));
                }

                merchant.Status = "active";
                _context.Update(merchant);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Merchant '{merchant.Name}' has been unblocked successfully.";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "An error occurred while unblocking the merchant.";
            }

            return RedirectToAction(nameof(Merchants));
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var totalUsers = await _context.Users.CountAsync();
            var activeUsers = await _context.Users.CountAsync(u => u.Status == "active");
            var blockedUsers = await _context.Users.CountAsync(u => u.Status == "blocked");

            var totalMerchants = await _context.Merchants.CountAsync();
            var activeMerchants = await _context.Merchants.CountAsync(m => m.Status == "active");
            var blockedMerchants = await _context.Merchants.CountAsync(m => m.Status == "blocked");

            ViewBag.TotalUsers = totalUsers;
            ViewBag.ActiveUsers = activeUsers;
            ViewBag.BlockedUsers = blockedUsers;
            ViewBag.TotalMerchants = totalMerchants;
            ViewBag.ActiveMerchants = activeMerchants;
            ViewBag.BlockedMerchants = blockedMerchants;

            return View(Dashboard);
        }
    }
}