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

        public async Task<IActionResult> Index()
        {
            var logs = await _db.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .Take(200) // prevent overload
                .ToListAsync();

            return View(logs);
        }
    }
}