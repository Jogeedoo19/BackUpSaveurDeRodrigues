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

           

            return View(await products.ToListAsync());
        }


        public async Task<IActionResult> Detail(int? id)
        {
            

            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null ) return Unauthorized();

           
            return View(product);
        }


        public async Task<IActionResult> Add(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                TempData["WishlistMessage"] = "Product not found.";
                return RedirectToAction("Index", "Home");
            }

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

            //  Redirect to Product/Detail/{id}
            return RedirectToAction("Detail", "Home", new { id = id });
        }


        public async Task<IActionResult> ViewWishlist()
        {
            var wishlistItems = await _context.WishList
                .Include(w => w.Product) // Load related Product info
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
            return View();
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
