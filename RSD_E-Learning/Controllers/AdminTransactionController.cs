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

        // ================= MAIN PAGE =================
        public async Task<IActionResult> Index()
        {
            ViewBag.Courses = await _db.Courses.ToListAsync();
            return View();
        }

        // ================= AJAX LIST =================
        [HttpGet]
        public async Task<IActionResult> AjaxList(
            int? courseId,
            DateTime? startDate,
            DateTime? endDate)
        {
            var query = _db.Enrollments
                .Include(e => e.Student).ThenInclude(s => s.User)
                .Include(e => e.Course)
                .Where(e => e.PaymentStatus)
                .AsQueryable();

            if (courseId.HasValue)
                query = query.Where(e => e.CourseId == courseId);

            if (startDate.HasValue)
                query = query.Where(e => e.EnrolledAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.EnrolledAt <= endDate.Value);

            var transactions = await query
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            return PartialView("_TransactionTable", transactions);
        }
    }
}
