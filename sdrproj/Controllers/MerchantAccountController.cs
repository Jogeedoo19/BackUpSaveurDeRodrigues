using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Hosting;
using sdrproj.Models;
using System.Linq;

public class MerchantAccountController : Controller
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly ApplicationDbContext _context;

    public MerchantAccountController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    // GET: Merchant/Register
    public IActionResult Register()
    {
        // Redirect to home if already logged in
        if (HttpContext.Session.GetString("MerchantId") != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    // POST: Merchant/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(Merchant Merchant, IFormFile profileImage)
    {
        // Redirect to home if already logged in
        if (HttpContext.Session.GetString("MerchantId") != null)
        {
            return RedirectToAction("Index", "Home");
        }

        try
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (_context.Merchants.Any(x => x.Email == Merchant.Email))
                {
                    ViewBag.Error = "Email already exists";
                    return View(Merchant);
                }

                // Profile Picture Upload - OPTIONAL
                if (profileImage != null && profileImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(profileImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ProfilePicture", "Only JPG and PNG files are allowed.");
                        return View(Merchant);
                    }

                    if (profileImage.Length > 2 * 1024 * 1024) // 2MB max
                    {
                        ModelState.AddModelError("ProfilePicture", "Maximum file size is 2MB.");
                        return View(Merchant);
                    }

                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(_hostEnvironment.WebRootPath, "images/Merchants", fileName);
                    var directory = Path.GetDirectoryName(filePath);

                    if (directory != null)
                    {
                        Directory.CreateDirectory(directory);
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }

                    Merchant.ProfileImagePath = "/images/Merchants/" + fileName;
                }

                // Hash password and set Merchant properties
                Merchant.Password = BCrypt.Net.BCrypt.HashPassword(Merchant.Password);
                Merchant.CreatedAt = DateTime.Now;
                Merchant.Status = "active";

                // Add Merchant to context and save
                _context.Merchants.Add(Merchant);
                await _context.SaveChangesAsync();

                // Add success message and redirect
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction("Login", "MerchantAccount");
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = "An error occurred during registration. Please try again.";
        }

        return View(Merchant);
    }

    // GET: Merchant/Login
    public IActionResult Login()
    {
        // Redirect to home if already logged in
        if (HttpContext.Session.GetString("MerchantId") != null)
        {
            return RedirectToAction("Index", "Home");
        }

        if (TempData["SuccessMessage"] != null)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
        }
        return View();
    }

    // POST: Merchant/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string email, string password)
    {
        // Redirect to home if already logged in
        if (HttpContext.Session.GetString("MerchantId") != null)
        {
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var Merchant = _context.Merchants.SingleOrDefault(u => u.Email == email && u.Status == "active");

            if (Merchant != null && BCrypt.Net.BCrypt.Verify(password, Merchant.Password))
            {
                // Set session variables
                HttpContext.Session.SetString("MerchantId", Merchant.MerchantId.ToString());
                HttpContext.Session.SetString("MerchantRole", "Merchant");
                HttpContext.Session.SetString("MerchantEmail", Merchant.Email);

                // Store Merchant's name if available (adjust property names based on your Merchant model)
                if (!string.IsNullOrEmpty(Merchant.Name))
                {
                    HttpContext.Session.SetString("Name", Merchant.Name);
                }


                TempData["WelcomeMessage"] = "Welcome back!";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Invalid credentials";
        }
        catch (Exception ex)
        {
            ViewBag.Error = "An error occurred during login. Please try again.";
        }

        return View();
    }

    // Logout action
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        TempData["LogoutMessage"] = "You have been logged out successfully.";
        return RedirectToAction("Login", "MerchantAccount");
    }

    // Check if Merchant is logged in (helper method for other controllers)
    public static bool IsMerchantLoggedIn(HttpContext context)
    {
        return !string.IsNullOrEmpty(context.Session.GetString("MerchantId"));
    }
}