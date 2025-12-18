using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    public class CoursesController : Controller
    {
        private readonly DB _db;

        public CoursesController(DB db)
        {
            _db = db;
        }

        // GET: Courses/Catalog (Student Course Catalogue)
        public async Task<IActionResult> Catalog()
        {

            var categories = await _db.Categories
                .Where(cat =>
                    cat.Courses.Any(course =>
                        course.IsApproved && course.IsPublished))
                .ToListAsync();

            return View(categories);
        }


        // GET: Courses/Details/5 (View single course details)
        public async Task<IActionResult> Details(int id)
        {
            var course = await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }
    }
}
