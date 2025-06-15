using Microsoft.AspNetCore.Mvc;
using sdrproj.Models;

namespace sdrproj.Controllers
{
    public class ContactUsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ContactUsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ContactUs/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ContactUs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ContactUs contactUs)
        {
            if (ModelState.IsValid)
            {
                _context.Add(contactUs);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Message submitted successfully!";
                return RedirectToAction(nameof(Create));
            }

            return View(contactUs);
        }


    }
}
