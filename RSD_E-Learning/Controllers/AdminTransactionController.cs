using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminTransactionController : Controller
    {
        private readonly DB _db;

        public AdminTransactionController(DB db)
        {
            _db = db;
        }

        // ================= TRANSACTION LIST + FILTER =================
        public async Task<IActionResult> Index(
            int? courseId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _db.Enrollments
                .Include(e => e.Student).ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Where(e => e.PaymentStatus)
                .AsQueryable();

            // FILTER: Course
            if (courseId.HasValue)
                query = query.Where(e => e.CourseId == courseId);

            // FILTER: Date Range
            if (startDate.HasValue)
                query = query.Where(e => e.EnrolledAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.EnrolledAt <= endDate.Value);

            var transactions = await query
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            ViewBag.Courses = await _db.Courses.ToListAsync();

            return View(transactions);
        }
    }
}
