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

        public async Task<IActionResult> Index()
        {
            var enrollments = await _db.Enrollments
                .Include(e => e.Student).ThenInclude(s => s.User)
                .Include(e => e.Course)
                .OrderByDescending(e => e.EnrolledAt)
                .ToListAsync();

            return View(enrollments);
        }
    }
}
