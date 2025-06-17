using BCrypt.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using sdrproj.Models;
using System;
using System.Linq;

namespace sdrproj.Controllers
{
    public class AdminAccountController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminAccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Login
        public IActionResult Login()
        {
            if (HttpContext.Session.GetInt32("AdminId") != null)
            {
                return RedirectToAction("Index", "AdminDashboard");
            }

            ViewBag.SuccessMessage = TempData["SuccessMessage"];
            ViewBag.LogoutMessage = TempData["LogoutMessage"];

            return View();
        }

        // POST: Admin/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(string email, string password)
        {
            if (HttpContext.Session.GetInt32("AdminId") != null)
            {
                return RedirectToAction("Index", "AdminDashboard");
            }

            try
            {
                var admin = _context.Admins.SingleOrDefault(a => a.Email == email);

                if (admin != null && BCrypt.Net.BCrypt.Verify(password, admin.Password))
                {
                    HttpContext.Session.SetInt32("AdminId", admin.AdminId);
                    HttpContext.Session.SetString("AdminEmail", admin.Email);

                    TempData["WelcomeMessage"] = "Welcome to Admin Dashboard!";
                    return RedirectToAction("Index", "AdminDashboard");
                }

                ViewBag.Error = "Invalid admin credentials.";
            }
            catch (Exception)
            {
                ViewBag.Error = "An error occurred during login. Please try again.";
            }

            return View();
        }

        // Admin Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["LogoutMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Login", "AdminAccount");
        }

        // Helper Methods
        public static bool IsAdminLoggedIn(HttpContext context)
        {
            return context.Session.GetInt32("AdminId") != null;
        }

        public static int? GetCurrentAdminId(HttpContext context)
        {
            return context.Session.GetInt32("AdminId");
        }

        public static string GetCurrentAdminEmail(HttpContext context)
        {
            return context.Session.GetString("AdminEmail");
        }
    }
}
