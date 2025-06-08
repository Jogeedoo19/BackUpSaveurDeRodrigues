using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sdrproj.Models;

namespace sdrproj.Controllers
{
    public class MerchantProductController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MerchantProductController(ApplicationDbContext context)
        {
            _context = context;
        }

        private void PopulateSubCategoryDropDown(object selectedSubCategory = null)
        {
            var subCategories = _context.SubCategories.ToList();
            ViewBag.SubCategoryId = new SelectList(subCategories, "SubCategoryId", "Name", selectedSubCategory);
        }

        // GET: Product List / Search
        public async Task<IActionResult> Index(string searchString)
        {
            var products = _context.Products.Include(p => p.SubCategory).AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            var productList = await products.ToListAsync();

            // Debug: Show product IDs
            ViewBag.Debug = $"Found {productList.Count} products. IDs: {string.Join(", ", productList.Select(p => p.ProductId))}";

            return View(productList);
        }

        // GET: Create
        public IActionResult Create()
        {
            PopulateSubCategoryDropDown();
            ViewData["Title"] = "Create Product";
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            try
            {
                // Remove MerchantId from ModelState validation since we're setting it manually
                ModelState.Remove("MerchantId");

                // Debug: Log ModelState errors
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value.Errors.Select(e => e.ErrorMessage) })
                        .ToList();

                    // You can set a breakpoint here to inspect errors
                    ViewBag.ValidationErrors = errors;
                }

                if (ModelState.IsValid)
                {
                    // Assigning dummy MerchantId for now – you can pull from session/login later
                    product.MerchantId = 1;

                    _context.Add(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                // Debug: Catch any exceptions
                ViewBag.Error = ex.Message;
                if (ex.InnerException != null)
                    ViewBag.InnerError = ex.InnerException.Message;
            }

            // If we got this far, something failed, redisplay form
            PopulateSubCategoryDropDown(product.SubCategoryId);
            return View(product);
        }

        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0)
                return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            PopulateSubCategoryDropDown(product.SubCategoryId);
            ViewData["Title"] = "Edit Product";
            return View(product);
        }


        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.ProductId)
                return NotFound();

            // Optional: remove if you set MerchantId manually
            ModelState.Remove("MerchantId");

            if (ModelState.IsValid)
            {
                try
                {
                    // Assign a dummy MerchantId if needed
                    if (product.MerchantId == 0)
                        product.MerchantId = 1;

                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(e => e.ProductId == id))
                        return NotFound();
                    else
                        throw;
                }
            }

            PopulateSubCategoryDropDown(product.SubCategoryId);
            ViewData["Title"] = "Edit Product";
            return View(product);
        }


        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.SubCategory)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null) return NotFound();

            return View(product);
        }

        // POST: Delete
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}