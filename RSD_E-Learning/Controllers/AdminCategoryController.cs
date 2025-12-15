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
        public async Task<IActionResult> Index()
        {
            var categories = await _db.Categories.ToListAsync();
            return View(categories);
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

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // -------------------- DELETE --------------------
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _db.Categories.FindAsync(id);
            if (category == null) return NotFound();

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
