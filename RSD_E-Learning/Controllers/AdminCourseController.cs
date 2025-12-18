using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCourseController : Controller
    {
        private readonly DB _db;

        public AdminCourseController(DB db)
        {
            _db = db;
        }

        // =========================
        // LIST + FILTER COURSES
        // =========================
        public async Task<IActionResult> Index(string filter = "all")
        {
            var query = _db.Courses
                .Include(c => c.Teacher).ThenInclude(t => t.User)
                .Include(c => c.Category)
                .AsQueryable();

            switch (filter.ToLower())
            {
                case "pending":
                    query = query.Where(c => !c.IsApproved && !c.IsRejected);
                    break;
                case "approved":
                    query = query.Where(c => c.IsApproved && !c.IsRejected);
                    break;
                case "rejected":
                    query = query.Where(c => c.IsRejected);
                    break;
            }

            ViewBag.Filter = filter;

            return View(await query.ToListAsync());
        }

        // =========================
        // PUBLISH / UNPUBLISH
        // =========================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TogglePublish(int id)
        {
            var course = await _db.Courses.FindAsync(id);
            if (course == null) return NotFound();

            // SAFETY CHECK
            if (!course.IsApproved || course.IsRejected)
            {
                TempData["Error"] = "Only approved courses can be published.";
                return RedirectToAction(nameof(Index));
            }

            course.IsPublished = !course.IsPublished;

            _db.AuditLogs.Add(new DB.AuditLog
            {
                Action = course.IsPublished
                    ? $"Published course: {course.Title}"
                    : $"Unpublished course: {course.Title}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
