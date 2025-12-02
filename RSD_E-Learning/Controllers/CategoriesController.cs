using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly DB _db;

        public CategoriesController(DB db)
        {
            _db = db;
        }

        // GET: Categories (View all categories)
        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories
                .Include(c => c.Courses)
                .ToListAsync();

            return View(categories);
        }

        // GET: Categories/Courses/5 (View courses in a specific category)
        public async Task<IActionResult> Courses(int id)
        {
            var category = await _db.Categories
                .Include(c => c.Courses)
                    .ThenInclude(course => course.Teacher)
                        .ThenInclude(teacher => teacher.User)
                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }
    }
}
