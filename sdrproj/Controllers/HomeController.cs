using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdrproj.Models;

namespace sdrproj.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> ViewProduct(string searchString)
        {
            IQueryable<Product> products = _context.Products.Include(p => p.SubCategory);

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString) ||
                                              p.Description.Contains(searchString));
            }

            return View(await products.ToListAsync());
        }

        public async Task<IActionResult> Detail(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.SubCategory)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // Cart Methods
        public async Task<IActionResult> AddToCart(int productId, int count = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                TempData["CartMessage"] = "Product not found.";
                return RedirectToAction("ViewProduct");
            }

            if (product.Stock < count)
            {
                TempData["CartMessage"] = "Product added to cart successfully!";
                return RedirectToAction("ViewProduct");

            }

            // For demo purposes, using a session-based user ID
            // In production, you'd use actual user authentication
            string userId = GetOrCreateSessionUserId();

            var existingCartItem = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == productId);

            if (existingCartItem != null)
            {
                // Update existing cart item
                if (existingCartItem.Count + count > product.Stock)
                {
                    TempData["CartMessage"] = "Product added to cart successfully!";
                    return RedirectToAction("ViewProduct");

                }

                existingCartItem.Count += count;
                existingCartItem.AddedDateTime = DateTime.Now;
                _context.Carts.Update(existingCartItem);
            }
            else
            {
                // Add new cart item
                var cartItem = new Cart
                {
                    UserId = userId,
                    ProductId = productId,
                    Count = count,
                    AddedDateTime = DateTime.Now
                };
                _context.Carts.Add(cartItem);
            }

            await _context.SaveChangesAsync();
            TempData["CartMessage"] = "Product added to cart successfully!";

            return RedirectToAction("ViewCart");

        }

        public async Task<IActionResult> ViewCart()
        {
            string userId = GetOrCreateSessionUserId();

            var cartItems = await _context.Carts
                .Include(c => c.Product)
                .Where(c => c.UserId == userId)
                .OrderByDescending(c => c.AddedDateTime)
                .ToListAsync();

            // Calculate total
            decimal total = cartItems.Sum(c => c.Product.Price * c.Count);
            ViewBag.CartTotal = total;
            ViewBag.ItemCount = cartItems.Sum(c => c.Count);

            return View(cartItems);
        }

        public async Task<IActionResult> UpdateCartQuantity(int cartId, int count)
        {
            if (count < 1)
            {
                return await RemoveFromCart(cartId);
            }

            var cartItem = await _context.Carts
                .Include(c => c.Product)
                .FirstOrDefaultAsync(c => c.CartId == cartId);

            if (cartItem == null)
            {
                TempData["CartMessage"] = "Cart item not found.";
                return RedirectToAction("ViewCart");
            }

            // Check stock availability
            if (count > cartItem.Product.Stock)
            {
                TempData["CartMessage"] = $"Only {cartItem.Product.Stock} items available in stock.";
                return RedirectToAction("ViewCart");
            }

            cartItem.Count = count;
            cartItem.AddedDateTime = DateTime.Now;
            _context.Carts.Update(cartItem);
            await _context.SaveChangesAsync();

            TempData["CartMessage"] = "Cart updated successfully!";
            return RedirectToAction("ViewCart");





        }

        public async Task<IActionResult> RemoveFromCart(int id)
        {
            //var cartItem = await _context.Carts.FindAsync(cartId);
            //if (cartItem == null)
            //{
            //    TempData["CartMessage"] = "Cart item not found.";
            //    return RedirectToAction("ViewCart");
            //}

            //_context.Carts.Remove(cartItem);
            //await _context.SaveChangesAsync();

            //TempData["CartMessage"] = "Item removed from cart.";
            //return RedirectToAction("ViewCart");

            var wishlistItem = await _context.Carts.FindAsync(id);
            if (wishlistItem == null)
            {
                TempData["WishlistMessage"] = "Item not found in wishlist.";
                return RedirectToAction("ViewCart");
            }

            _context.Carts.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            TempData["WishlistMessage"] = "Item removed from wishlist.";
            return RedirectToAction("ViewCart");

        }

        public async Task<IActionResult> ClearCart()
        {
            string userId = GetOrCreateSessionUserId();

            var cartItems = await _context.Carts
                .Where(c => c.UserId == userId)
                .ToListAsync();

            if (cartItems.Any())
            {
                _context.Carts.RemoveRange(cartItems);
                await _context.SaveChangesAsync();
                TempData["CartMessage"] = "Cart cleared successfully!";
            }

            return RedirectToAction("ViewCart");
        }

        // Get cart item count for display in navigation
        public async Task<IActionResult> GetCartItemCount()
        {
            string userId = GetOrCreateSessionUserId();

            var itemCount = await _context.Carts
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Count);

            return Json(itemCount);
        }

        // Helper method to get or create session-based user ID
        private string GetOrCreateSessionUserId()
        {
            string sessionKey = "SessionUserId";
            string userId = HttpContext.Session.GetString(sessionKey);

            if (string.IsNullOrEmpty(userId))
            {
                userId = Guid.NewGuid().ToString();
                HttpContext.Session.SetString(sessionKey, userId);
            }

            return userId;
        }

        // Existing wishlist methods
        public async Task<IActionResult> Add(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["WishlistMessage"] = "Product not found.";
                return RedirectToAction("ViewProduct");
            }

            // Using session-based user ID for wishlist too
            string userId = GetOrCreateSessionUserId();

            bool exists = _context.WishList.Any(w => w.ProductId == id);
            if (!exists)
            {
                var wishlistItem = new WishList
                {
                    ProductId = id,
                    TimeAdd = DateTime.Now
                };

                _context.WishList.Add(wishlistItem);
                await _context.SaveChangesAsync();

                TempData["WishlistMessage"] = "Product added to wishlist!";
            }
            else
            {
                TempData["WishlistMessage"] = "Product already in wishlist.";
            }

            return RedirectToAction("Detail", "Home", new { id = id });
        }

        public async Task<IActionResult> ViewWishlist()
        {
            var wishlistItems = await _context.WishList
                .Include(w => w.Product)
                .ToListAsync();

            return View(wishlistItems);
        }

        public async Task<IActionResult> Remove(int id)
        {
            var wishlistItem = await _context.WishList.FindAsync(id);
            if (wishlistItem == null)
            {
                TempData["WishlistMessage"] = "Item not found in wishlist.";
                return RedirectToAction("ViewWishlist");
            }

            _context.WishList.Remove(wishlistItem);
            await _context.SaveChangesAsync();

            TempData["WishlistMessage"] = "Item removed from wishlist.";
            return RedirectToAction("ViewWishlist");
        }

        // Existing action methods
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ContactUs()
        {
            return View();
        }

        public IActionResult Shop()
        {
            return View();
        }

        public IActionResult ShopDetail()
        {
            return View();
        }

        public IActionResult Cart()
        {
            return RedirectToAction("ViewCart");
        }

        public IActionResult Checkout()
        {
            return View();
        }

        public IActionResult Testimonial()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}