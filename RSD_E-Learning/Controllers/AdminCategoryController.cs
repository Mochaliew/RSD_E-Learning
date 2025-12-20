using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminCategoryController : Controller
    {
        private readonly DB _db;

        public AdminCategoryController(DB db)
        {
            _db = db;
        }

        // -------------------- LIST --------------------
        public async Task<IActionResult> Index(string? search, bool showDeleted = false)
        {
            var query = _db.Categories.AsQueryable();

            query = showDeleted
                ? query.Where(c => c.IsDeleted)
                : query.Where(c => !c.IsDeleted);

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c => c.Name.Contains(search));
            }

            var vm = new CategoryFilterVm
            {
                Search = search,
                ShowDeleted = showDeleted,
                Categories = await query
                    .OrderBy(c => c.Name)
                    .ToListAsync()
            };

            return View(vm);
        }


        // -------------------- CREATE --------------------
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryCreateVm model)
        {
            if (!ModelState.IsValid) return View(model);

            var category = new DB.Category
            {
                Name = model.Name,
                Description = model.Description
            };

            _db.Categories.Add(category);

            //auditlog
            _db.AuditLogs.Add(new DB.AuditLog
            {
                Action = $"Created category: {category.Name}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // -------------------- EDIT --------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            var model = new CategoryEditVm
            {
                CategoryId = category.CategoryId,
                Name = category.Name,
                Description = category.Description
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryEditVm model)
        {
            if (!ModelState.IsValid) return View(model);

            var category = await _db.Categories.FindAsync(model.CategoryId);
            if (category == null) return NotFound();

            category.Name = model.Name;
            category.Description = model.Description;

            //auditlog
            _db.AuditLogs.Add(new DB.AuditLog
            {
                Action = $"Edited category: {category.Name}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // -------------------- DELETE --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.IsDeleted = true;

            _db.AuditLogs.Add(new DB.AuditLog
            {
                Action = $"Soft deleted category: {category.Name}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // -------------------- VIEW DELETED --------------------
        public async Task<IActionResult> Deleted()
        {
            var deleted = await _db.Categories
                .Where(c => c.IsDeleted)
                .ToListAsync();

            return View(deleted);
        }


        // -------------------- RESTORE --------------------
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            category.IsDeleted = false;

            _db.AuditLogs.Add(new DB.AuditLog
            {
                Action = $"Restored category: {category.Name}",
                Timestamp = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Deleted));
        }

    }
}
