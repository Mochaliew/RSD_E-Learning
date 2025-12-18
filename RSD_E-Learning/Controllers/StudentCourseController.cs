using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentCourseController : Controller
    {
        private readonly DB _db;

        public StudentCourseController(DB db)
        {
            _db = db;
        }

        // ================== VIEW PUBLISHED COURSES ==================
        public async Task<IActionResult> Index()
        {
            var courses = await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Where(c => c.IsApproved && c.IsPublished)
                .ToListAsync();

            return View(courses);
        }

        // ================== COURSE DETAILS ==================
        public async Task<IActionResult> Details(int id)
        {
            var course = await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Include(c => c.Lessons)
                .FirstOrDefaultAsync(c => c.CourseId == id);

            if (course == null)
                return NotFound();

            return View(course);
        }

        // ================== ENROLL COURSE ==================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(int courseId)
        {
            var userEmail = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

            if (student == null)
                return Unauthorized();

            var alreadyEnrolled = await _db.Enrollments.AnyAsync(e =>
                e.StudentId == student.StudentId &&
                e.CourseId == courseId);

            if (alreadyEnrolled)
            {
                TempData["Message"] = "You are already enrolled in this course.";
                return RedirectToAction("Details", new { id = courseId });
            }

            var enrollment = new DB.Enrollment
            {
                StudentId = student.StudentId,
                CourseId = courseId,
                PaymentStatus = false
            };

            _db.Enrollments.Add(enrollment);
            await _db.SaveChangesAsync();

            TempData["Message"] = "Enrollment successful!";
            return RedirectToAction("MyCourses");
        }

        public async Task<IActionResult> MyCourses()
        {
            var userEmail = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == userEmail);

            if (student == null)
                return Unauthorized();

            var courses = await _db.Enrollments
                .Where(e => e.StudentId == student.StudentId)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Category)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Teacher)
                        .ThenInclude(t => t.User)
                .Select(e => e.Course)
                .Where(c => c != null)   // ⬅️ CRITICAL
                .ToListAsync();

            // ⬅️ ABSOLUTE SAFETY
            return View(courses ?? new List<DB.Course>());
        }





        //FAKEDATA
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ByCategory(int categoryId)
        {
            var courses = await _db.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                    .ThenInclude(t => t.User)
                .Where(c =>
                    c.CategoryId == categoryId &&
                    c.IsApproved &&
                    c.IsPublished)
                .ToListAsync();

            return View(courses);
        }

    }
}
