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

    public async Task<IActionResult> Edit(int id)
    {

        var UserId = HttpContext.Session.GetString("UserId");
        if (UserId == null) return RedirectToAction("Login", "UserAccount");


        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        // Clear password fields for editing
        user.Password = string.Empty;
        user.ConfirmPassword = string.Empty;

        return View(user);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("UserId,Name,Email,Password,ConfirmPassword,Phone,Address,PostalCode,Status")] User user)
    {
        if (id != user.UserId)
            return NotFound();

        var existingUser = await _context.Users.FindAsync(id);
        if (existingUser == null)
            return NotFound();

        // Remove validation for Password if empty (unchanged)
        if (string.IsNullOrEmpty(user.Password))
        {
            ModelState.Remove("Password");
            ModelState.Remove("ConfirmPassword");
        }

        if (ModelState.IsValid)
        {
            try
            {
                // Update fields
                existingUser.Name = user.Name;
                existingUser.Email = user.Email;
                existingUser.Phone = user.Phone;
                existingUser.Address = user.Address;
                existingUser.PostalCode = user.PostalCode;
                existingUser.Status = user.Status;

                // If new password entered, hash and save it
                if (!string.IsNullOrEmpty(user.Password))
                {
                    // Replace this with your real hashing method
                    existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
                }

                // Handle profile image upload


                _context.Update(existingUser);
                await _context.SaveChangesAsync();
                return RedirectToAction("Edit", new { id = user.UserId });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Update failed: " + ex.Message);
            }
        }

        return View(user);
    }
}