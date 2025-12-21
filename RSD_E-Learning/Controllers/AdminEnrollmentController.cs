using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminEnrollmentController : Controller
    {
        private readonly DB _db;

        public AdminEnrollmentController(DB db)
        {
            _db = db;
        }

        // ================= LIST =================
        public async Task<IActionResult> Index()
        {
            var enrollments = await _db.Enrollments
                .Include(e => e.Student).ThenInclude(s => s.User)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            return View(enrollments);
        }

        // ================= AJAX FILTER =================
        [HttpGet]
        public async Task<IActionResult> Filter(string? search, bool? paymentStatus)
        {
            var query = _db.Enrollments
                .Include(e => e.Student).ThenInclude(s => s.User)
                .Include(e => e.Course)
                .AsQueryable();

            // SEARCH: student name / email / course
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(e =>
                    e.Student!.User!.FullName.Contains(search) ||
                    e.Student.User.Email.Contains(search) ||
                    e.Course!.Title.Contains(search));
            }

            // FILTER: payment status
            if (paymentStatus.HasValue)
            {
                query = query.Where(e => e.PaymentStatus == paymentStatus.Value);
            }

            var enrollments = await query
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            return PartialView("_EnrollmentTable", enrollments);
        }
    }
}
