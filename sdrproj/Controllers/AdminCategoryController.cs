using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using sdrproj.Models;

namespace sdrproj.Controllers
{
    public class AdminCategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminCategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin Dashboard - Category Management
        public IActionResult Index()
        {
            var categories = _context.Categories
                .Include(c => c.SubCategories)
                .OrderBy(c => c.Name)
                .ToList();
            return View(categories);
        }

        // GET: Category/Create
        public IActionResult CreateCategory()
        {
            return View("CreateCategory");
        }

        // POST: Category/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                // Check if category name already exists
                if (_context.Categories.Any(c => c.Name.ToLower() == category.Name.ToLower()))
                {
                    ModelState.AddModelError("Name", "Category with this name already exists");
                    return View(category);
                }

                _context.Categories.Add(category);
                _context.SaveChanges();
                TempData["Success"] = "Category created successfully!";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Category/Edit/5
        public IActionResult EditCategory(int id)
        {
            var category = _context.Categories.Find(id);
            if (category == null)
            {
                TempData["Error"] = "Category not found";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // POST: Category/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                // Check if category name already exists (excluding current category)
                if (_context.Categories.Any(c => c.Name.ToLower() == category.Name.ToLower() && c.CategoryId != category.CategoryId))
                {
                    ModelState.AddModelError("Name", "Category with this name already exists");
                    return View(category);
                }

                _context.Categories.Update(category);
                _context.SaveChanges();
                TempData["Success"] = "Category updated successfully!";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Category/Delete/5
        public IActionResult DeleteCategory(int id)
        {
            var category = _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category == null)
            {
                TempData["Error"] = "Category not found";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // POST: Category/Delete/5
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteCategoryConfirmed(int id)
        {
            var category = _context.Categories
                .Include(c => c.SubCategories)
                .FirstOrDefault(c => c.CategoryId == id);

            if (category != null)
            {
                // Check if category has subcategories
                if (category.SubCategories.Any())
                {
                    TempData["Error"] = "Cannot delete category that has subcategories. Please delete subcategories first.";
                    return RedirectToAction("Index");
                }

                _context.Categories.Remove(category);
                _context.SaveChanges();
                TempData["Success"] = "Category deleted successfully!";
            }
            return RedirectToAction("Index");
        }

        // GET: SubCategory/Create
        public IActionResult CreateSubCategory()
        {
            try
            {
                var categories = _context.Categories.OrderBy(c => c.Name).ToList();
                ViewBag.Categories = categories;

                // Debug: Check if categories exist
                Console.WriteLine($"Categories loaded: {categories.Count}");
                foreach (var cat in categories)
                {
                    Console.WriteLine($"Category: {cat.CategoryId} - {cat.Name}");
                }

                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateSubCategory GET: {ex.Message}");
                TempData["Error"] = "Error loading categories.";
                return RedirectToAction("Index");
            }
        }

        // POST: SubCategory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CreateSubCategory(SubCategory subCategory)
        {
            Console.WriteLine("=== CreateSubCategory POST Method Called ===");
            Console.WriteLine($"Received SubCategory - Name: '{subCategory?.Name}', CategoryId: {subCategory?.CategoryId}");

            // Always reload categories for ViewBag in case we need to return the view
            ViewBag.Categories = _context.Categories.OrderBy(c => c.Name).ToList();

            try
            {
                // Check if subCategory object is null
                if (subCategory == null)
                {
                    Console.WriteLine("SubCategory object is null!");
                    TempData["Error"] = "SubCategory data is null";
                    return View();
                }

                // Manual validation check
                if (string.IsNullOrWhiteSpace(subCategory.Name))
                {
                    Console.WriteLine("Name is null or empty");
                    ModelState.AddModelError("Name", "SubCategory name is required");
                }

                if (subCategory.CategoryId <= 0)
                {
                    Console.WriteLine("CategoryId is invalid");
                    ModelState.AddModelError("CategoryId", "Please select a valid category");
                }

                // Check ModelState
                Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

                if (!ModelState.IsValid)
                {
                    Console.WriteLine("ModelState Errors:");
                    foreach (var error in ModelState)
                    {
                        Console.WriteLine($"  Key: {error.Key}");
                        foreach (var err in error.Value.Errors)
                        {
                            Console.WriteLine($"    Error: {err.ErrorMessage}");
                        }
                    }

                    TempData["Error"] = "Please fix the validation errors below";
                    return View(subCategory);
                }

                // Check if category exists
                var categoryExists = _context.Categories.Any(c => c.CategoryId == subCategory.CategoryId);
                if (!categoryExists)
                {
                    Console.WriteLine($"Category with ID {subCategory.CategoryId} does not exist");
                    ModelState.AddModelError("CategoryId", "Selected category does not exist");
                    return View(subCategory);
                }

                // Check for duplicate name in same category
                var duplicate = _context.SubCategories
                    .Any(sc => sc.Name.ToLower().Trim() == subCategory.Name.ToLower().Trim()
                            && sc.CategoryId == subCategory.CategoryId);

                if (duplicate)
                {
                    Console.WriteLine($"Duplicate subcategory name found");
                    ModelState.AddModelError("Name", "SubCategory with this name already exists in the selected category");
                    return View(subCategory);
                }

                // Clean the name
                subCategory.Name = subCategory.Name.Trim();
                subCategory.CreatedAt = DateTime.Now;

                Console.WriteLine("Attempting to add subcategory to database...");
                _context.SubCategories.Add(subCategory);

                var saveResult = _context.SaveChanges();
                Console.WriteLine($"SaveChanges completed. Rows affected: {saveResult}");

                if (saveResult > 0)
                {
                    TempData["Success"] = $"SubCategory '{subCategory.Name}' created successfully!";
                    Console.WriteLine("SubCategory created successfully, redirecting to Index");
                    return RedirectToAction("Index");
                }
                else
                {
                    Console.WriteLine("SaveChanges returned 0 - no rows affected");
                    TempData["Error"] = "Failed to save subcategory to database";
                    return View(subCategory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occurred: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                TempData["Error"] = $"Error creating subcategory: {ex.Message}";
                return View(subCategory);
            }
        }

        // GET: SubCategory/Edit/5
        public IActionResult EditSubCategory(int id)
        {
            var subCategory = _context.SubCategories
                .Include(sc => sc.Category)
                .FirstOrDefault(sc => sc.SubCategoryId == id);

            if (subCategory == null)
            {
                TempData["Error"] = "SubCategory not found";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = _context.Categories.OrderBy(c => c.Name).ToList();
            return View(subCategory);
        }

        // POST: SubCategory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EditSubCategory(SubCategory subCategory)
        {
            if (ModelState.IsValid)
            {
                // Check if subcategory name already exists within the same category (excluding current subcategory)
                if (_context.SubCategories.Any(sc => sc.Name.ToLower() == subCategory.Name.ToLower()
                    && sc.CategoryId == subCategory.CategoryId
                    && sc.SubCategoryId != subCategory.SubCategoryId))
                {
                    ModelState.AddModelError("Name", "SubCategory with this name already exists in the selected category");
                    ViewBag.Categories = _context.Categories.OrderBy(c => c.Name).ToList();
                    return View(subCategory);
                }

                _context.SubCategories.Update(subCategory);
                _context.SaveChanges();
                TempData["Success"] = "SubCategory updated successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.Categories = _context.Categories.OrderBy(c => c.Name).ToList();
            return View(subCategory);
        }

        // GET: SubCategory/Delete/5
        public IActionResult DeleteSubCategory(int id)
        {
            var subCategory = _context.SubCategories
                .Include(sc => sc.Category)
                .FirstOrDefault(sc => sc.SubCategoryId == id);

            if (subCategory == null)
            {
                TempData["Error"] = "SubCategory not found";
                return RedirectToAction("Index");
            }
            return View(subCategory);
        }

        // POST: SubCategory/Delete/5
        [HttpPost, ActionName("DeleteSubCategory")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteSubCategoryConfirmed(int id)
        {
            var subCategory = _context.SubCategories.Find(id);
            if (subCategory != null)
            {
                _context.SubCategories.Remove(subCategory);
                _context.SaveChanges();
                TempData["Success"] = "SubCategory deleted successfully!";
            }
            return RedirectToAction("Index");
        }

        // API endpoint to get subcategories by category ID (for AJAX calls)
        [HttpGet]
        public JsonResult GetSubCategoriesByCategory(int categoryId)
        {
            var subCategories = _context.SubCategories
                .Where(sc => sc.CategoryId == categoryId)
                .Select(sc => new { sc.SubCategoryId, sc.Name })
                .OrderBy(sc => sc.Name)
                .ToList();

            return Json(subCategories);
        }

    

    }
}