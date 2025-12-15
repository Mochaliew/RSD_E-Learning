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

        public async Task<IActionResult> Index()
        {
            var model = new AdminDashboardVm
            {
                TotalTeachers = await _db.Teachers.CountAsync(),
                TotalStudents = await _db.Students.CountAsync(),
                TotalCourses = await _db.Courses.CountAsync(),
                NewStudentRegistrations = await _db.Students
                    .CountAsync(s => s.EnrollmentDate >= DateTime.UtcNow.AddDays(-7)),

                LastUpdated = DateTime.Now,

                LatestActivities = await _db.AuditLogs
                    .OrderByDescending(a => a.Timestamp)
                    .Take(5)
                    .ToListAsync()
            };

            return View(model);
        }
    }
}
