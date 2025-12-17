using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminDashboardController : Controller
    {
        private readonly DB _db;

        public AdminDashboardController(DB db)
        {
            _db = db;
        }

        // ===================== DASHBOARD =====================
        public async Task<IActionResult> Index()
        {
            var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
            var vm = new AdminDashboardVm
            {
                // Core statistics
                TotalStudents = await _db.Students.CountAsync(),
                TotalTeachers = await _db.Teachers.CountAsync(),
                TotalCourses = await _db.Courses.CountAsync(),

                // New Student
                NewStudentRegistrations = await _db.Students.CountAsync(s => s.EnrollmentDate >= sevenDaysAgo),

                // Course approval statistics
                PendingCourses = await _db.Courses.CountAsync(c => !c.IsApproved && !c.IsRejected),
                ApprovedCourses = await _db.Courses.CountAsync(c => c.IsApproved),
                RejectedCourses = await _db.Courses.CountAsync(c => c.IsRejected),  

                // activity
                LatestActivities = await _db.AuditLogs
                    .OrderByDescending(a => a.Timestamp)
                    .Take(5)
                    .ToListAsync(),

                LastUpdated = DateTime.UtcNow
            };

            return View(vm);
        }
    }
}
