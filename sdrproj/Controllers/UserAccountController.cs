using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Hosting;
using sdrproj.Models;
using System.Linq;

public class UserAccountController : Controller
{
    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly ApplicationDbContext _context;

    public UserAccountController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
    {
        _context = context;
        _hostEnvironment = hostEnvironment;
    }

    // GET: User/Register
    public IActionResult Register()
    {
        // Redirect to home if already logged in
        if (HttpContext.Session.GetString("UserId") != null)
        {
            return RedirectToAction("Index", "Home");
        }
        return View();
    }

    // POST: User/Register
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(User user, IFormFile profileImage)
    {
        // Redirect to home if already logged in
        if (HttpContext.Session.GetString("UserId") != null)
        {
            return RedirectToAction("Index", "Home");
        }

        try
        {
            if (ModelState.IsValid)
            {
                // Check if email already exists
                if (_context.Users.Any(x => x.Email == user.Email))
                {
                    ViewBag.Error = "Email already exists";
                    return View(user);
                }

                // Profile Picture Upload - OPTIONAL
                if (profileImage != null && profileImage.Length > 0)
                {
                    var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                    var extension = Path.GetExtension(profileImage.FileName).ToLower();

                    if (!allowedExtensions.Contains(extension))
                    {
                        ModelState.AddModelError("ProfilePicture", "Only JPG and PNG files are allowed.");
                        return View(user);
                    }

                    if (profileImage.Length > 2 * 1024 * 1024) // 2MB max
                    {
                        ModelState.AddModelError("ProfilePicture", "Maximum file size is 2MB.");
                        return View(user);
                    }

                    var fileName = $"{Guid.NewGuid()}{extension}";
                    var filePath = Path.Combine(_hostEnvironment.WebRootPath, "images/users", fileName);
                    var directory = Path.GetDirectoryName(filePath);

                    if (directory != null)
                    {
                        Directory.CreateDirectory(directory);
                    }

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await profileImage.CopyToAsync(stream);
                    }

                    user.ProfileImagePath = "/images/users/" + fileName;
                }

                // Hash password and set user properties
                user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                user.CreatedAt = DateTime.Now;
                user.Status = "active";

                // Add user to context and save
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                // Add success message and redirect
                TempData["SuccessMessage"] = "Registration successful! Please log in.";
                return RedirectToAction("Login", "UserAccount");
            }
        }
        catch (Exception ex)
        {
            ViewBag.Error = "An error occurred during registration. Please try again.";
        }

        return View(user);
    }

    // GET: User/Login
    public IActionResult Login()
    {
        // Redirect to home if already logged in
        if (HttpContext.Session.GetString("UserId") != null)
        {
            return RedirectToAction("Index", "Home");
        }

        if (TempData["SuccessMessage"] != null)
        {
            ViewBag.SuccessMessage = TempData["SuccessMessage"];
        }
        return View();
    }

    // POST: User/Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(string email, string password)
    {
        // Redirect to home if already logged in
        if (HttpContext.Session.GetString("UserId") != null)
        {
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var user = _context.Users.SingleOrDefault(u => u.Email == email && u.Status == "active");

            if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                // Set session variables
                HttpContext.Session.SetString("UserId", user.UserId.ToString());
                HttpContext.Session.SetString("UserRole", "User");
                HttpContext.Session.SetString("UserEmail", user.Email);

                // Store user's name if available (adjust property names based on your User model)
                if (!string.IsNullOrEmpty(user.Name))
                {
                    HttpContext.Session.SetString("Name", user.Name);
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
        return RedirectToAction("Login", "UserAccount");
    }

    // Check if user is logged in (helper method for other controllers)
    public static bool IsUserLoggedIn(HttpContext context)
    {
        return !string.IsNullOrEmpty(context.Session.GetString("UserId"));
    }
}