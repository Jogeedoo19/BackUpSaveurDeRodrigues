using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Scripting;
using sdrproj.Models;
using System.Linq;

public class UserAccountController : Controller
{
    private readonly ApplicationDbContext _context;

    public UserAccountController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: User/Register
    public IActionResult Register() => View();

    // POST: User/Register
    [HttpPost]
    public IActionResult Register(User user)
    {
        if (ModelState.IsValid)
        {
            if (_context.Users.Any(x => x.Email == user.Email))
            {
                ViewBag.Error = "Email already exists";
                return View(user);
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            _context.Users.Add(user);
            _context.SaveChanges();
            return RedirectToAction("Login");
        }

        return View(user);
    }

    // GET: User/Login
    public IActionResult Login() => View();

    // POST: User/Login
    [HttpPost]
    public IActionResult Login(string email, string password)
    {
        var user = _context.Users.SingleOrDefault(u => u.Email == email);
        if (user != null && BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            HttpContext.Session.SetString("UserId", user.UserId.ToString());
            HttpContext.Session.SetString("UserRole", "User");
            return RedirectToAction("Index", "Home");
        }

        ViewBag.Error = "Invalid credentials";
        return View();
    }
}
