using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminAuditLogController : Controller
    {
        private readonly DB _db;

        public AdminAuditLogController(DB db)
        {
            _db = db;
        }

        // ================== FULL AUDIT LOG ==================
        public async Task<IActionResult> Index(string module)
        {
            var logsQuery = _db.AuditLogs.AsQueryable();

            // ✅ Filter by module using action text
            if (!string.IsNullOrEmpty(module))
            {
                logsQuery = module switch
                {
                    "Teacher" => logsQuery.Where(l => l.Action.Contains("teacher")),
                    "Category" => logsQuery.Where(l => l.Action.Contains("category")),
                    "Course" => logsQuery.Where(l => l.Action.Contains("course")),
                    "Student" => logsQuery.Where(l => l.Action.Contains("student")),
                    _ => logsQuery
                };
            }

            var logs = await logsQuery
                .OrderByDescending(l => l.Timestamp)
                .Take(200) // safety limit
                .ToListAsync();

            ViewBag.SelectedModule = module;
            return View(logs);
        }
    }
}
