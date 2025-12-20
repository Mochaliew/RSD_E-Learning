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

        // ================= FULL PAGE =================
        public async Task<IActionResult> Index()
        {
            var logs = await _db.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .Take(200)
                .ToListAsync();

            return View(logs);
        }

        // ================= AJAX FILTER =================
        [HttpGet]
        public async Task<IActionResult> Filter(string module)
        {
            var logsQuery = _db.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(module))
            {
                logsQuery = module switch
                {
                    "Teacher" => logsQuery.Where(l => l.Action.ToLower().Contains("teacher")),
                    "Student" => logsQuery.Where(l => l.Action.ToLower().Contains("student")),
                    "Course" => logsQuery.Where(l => l.Action.ToLower().Contains("course")),
                    "Category" => logsQuery.Where(l => l.Action.ToLower().Contains("category")),
                    _ => logsQuery
                };
            }

            var logs = await logsQuery
                .OrderByDescending(l => l.Timestamp)
                .Take(200)
                .ToListAsync();

            return PartialView("_AuditLogTable", logs);
        }
    }
}
