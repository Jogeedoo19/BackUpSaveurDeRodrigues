using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using sdrproj.Models;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace sdrproj.Controllers
{
    public class MerchantProductController : Controller
    {
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly ApplicationDbContext _context;

        public MerchantProductController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        private void PopulateSubCategoryDropDown(object selectedSubCategory = null)
        {
            var subCategories = _context.SubCategories.ToList();
            ViewBag.SubCategoryId = new SelectList(subCategories, "SubCategoryId", "Name", selectedSubCategory);
        }

        // GET: Product List / Search
        public async Task<IActionResult> Index(string searchString)
        {
            var merchantId = HttpContext.Session.GetInt32("MerchantId");
            if (merchantId == null) return RedirectToAction("Login", "MerchantAccount");

            var products = _context.Products
                .Include(p => p.SubCategory)
                .Where(p => p.MerchantId == merchantId);

            if (!string.IsNullOrEmpty(searchString))
            {
                products = products.Where(p => p.Name.Contains(searchString));
            }

            return View(await products.ToListAsync());
        }

        // GET: Create
        public IActionResult Create()
        {
            var merchantId = HttpContext.Session.GetInt32("MerchantId");
            if (merchantId == null) return RedirectToAction("Login", "MerchantAccount");

            PopulateSubCategoryDropDown();
            return View();
        }

        // POST: Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product, IFormFile productImage)
        {
            var merchantId = HttpContext.Session.GetInt32("MerchantId");
            if (merchantId == null) return RedirectToAction("Login", "MerchantAccount");

            ModelState.Remove("MerchantId");

            if (ModelState.IsValid)
            {
                product.MerchantId = merchantId.Value;

                // Handle image upload
                if (productImage != null && productImage.Length > 0)
                {
                    var extension = Path.GetExtension(productImage.FileName).ToLower();
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ImageUrl", "Only JPG and PNG are allowed.");
                        PopulateSubCategoryDropDown(product.SubCategoryId);
                        return View(product);
                    }

                    if (productImage.Length > 2 * 1024 * 1024)
                    {
                        ModelState.AddModelError("ImageUrl", "Maximum file size is 2MB.");
                        PopulateSubCategoryDropDown(product.SubCategoryId);
                        return View(product);
                    }

                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var path = Path.Combine(_hostEnvironment.WebRootPath, "images/product", fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(path));

                    using (var stream = new FileStream(path, FileMode.Create))
                    {
                        await productImage.CopyToAsync(stream);
                    }

                    product.ImageUrl = "/images/product/" + fileName;
                }

                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            PopulateSubCategoryDropDown(product.SubCategoryId);
            return View(product);
        }

        // GET: Edit
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            var merchantId = HttpContext.Session.GetInt32("MerchantId");
            if (merchantId == null) return RedirectToAction("Login", "MerchantAccount");

            if (id == null) return NotFound();

            var product = await _context.Products.FindAsync(id);
            if (product == null || product.MerchantId != merchantId) return Unauthorized();

            PopulateSubCategoryDropDown(product.SubCategoryId);
            return View(product);
        }

        // POST: Edit
        // POST: Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Product product, IFormFile productImage)
        {
            var merchantId = HttpContext.Session.GetInt32("MerchantId");
            if (merchantId == null) return RedirectToAction("Login", "MerchantAccount");

            if (id != product.ProductId)
            {
                return NotFound();
            }

            var productInDb = await _context.Products.FindAsync(id);
            if (productInDb == null || productInDb.MerchantId != merchantId) return Unauthorized();

            // Remove MerchantId from ModelState validation since we're setting it manually
            ModelState.Remove("MerchantId");

            if (ModelState.IsValid)
            {
                try
                {
                    // Update the product properties manually
                    productInDb.Name = product.Name;
                    productInDb.Description = product.Description;
                    productInDb.Price = product.Price;
                    productInDb.Stock = product.Stock;
                    productInDb.SubCategoryId = product.SubCategoryId;

                    // Handle new image upload
                    if (productImage != null && productImage.Length > 0)
                    {
                        var extension = Path.GetExtension(productImage.FileName).ToLower();
                        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };

                        if (!allowedExtensions.Contains(extension))
                        {
                            ModelState.AddModelError("ImageUrl", "Only JPG and PNG are allowed.");
                            PopulateSubCategoryDropDown(productInDb.SubCategoryId);
                            return View(productInDb);
                        }

                        if (productImage.Length > 2 * 1024 * 1024)
                        {
                            ModelState.AddModelError("ImageUrl", "Maximum file size is 2MB.");
                            PopulateSubCategoryDropDown(productInDb.SubCategoryId);
                            return View(productInDb);
                        }

                        // Delete old image if it exists
                        if (!string.IsNullOrEmpty(productInDb.ImageUrl))
                        {
                            var oldImagePath = Path.Combine(_hostEnvironment.WebRootPath, productInDb.ImageUrl.TrimStart('/'));
                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }

                        var fileName = $"{Guid.NewGuid()}{extension}";
                        var path = Path.Combine(_hostEnvironment.WebRootPath, "images/product", fileName);
                        Directory.CreateDirectory(Path.GetDirectoryName(path));

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await productImage.CopyToAsync(stream);
                        }

                        productInDb.ImageUrl = "/images/product/" + fileName;
                    }

                    _context.Update(productInDb);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Product updated successfully!";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.ProductId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            PopulateSubCategoryDropDown(productInDb.SubCategoryId);
            return View(productInDb);
        }

        // Helper method to check if product exists
        private bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
        // GET: Delete
        public async Task<IActionResult> Delete(int? id)
        {
            var merchantId = HttpContext.Session.GetInt32("MerchantId");
            if (merchantId == null) return RedirectToAction("Login", "MerchantAccount");

            if (id == null) return NotFound();

            var product = await _context.Products
                .Include(p => p.SubCategory)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null || product.MerchantId != merchantId) return Unauthorized();

            return View(product);
        }

        // POST: DeleteConfirmed
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var merchantId = HttpContext.Session.GetInt32("MerchantId");
            if (merchantId == null) return RedirectToAction("Login", "MerchantAccount");

            var product = await _context.Products.FindAsync(id);
            if (product == null || product.MerchantId != merchantId) return Unauthorized();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
