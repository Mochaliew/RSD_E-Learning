using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using RSD_E_Learning.ViewModels;

namespace RSD_E_Learning.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentController : Controller
    {
        private readonly DB _db;

        //   constructor injection
        public StudentController(DB db)
        {
            _db = db;
        }

        // ================== DASHBOARD ==================
        public IActionResult Dashboard()
        {
            return View();
        }

        // ================== PROFILE ==================
        public async Task<IActionResult> Profile()
        {
            var email = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == email);

            if (student == null)
                return NotFound();

            return View(student);
        }

        // ================== EDIT PROFILE ==================
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var email = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == email);

            if (student == null)
                return NotFound();

            var vm = new StudentEditProfileVm
            {
                StudentId = student.StudentId,
                FullName = student.User!.FullName,
                Email = student.User.Email,
                ClassName = student.ClassName
            };

            return View(vm);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProfile(StudentEditProfileVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var email = User.Identity!.Name;

            var student = await _db.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.User!.Email == email);

            if (student == null)
                return NotFound();

            student.User!.FullName = model.FullName;
            student.ClassName = model.ClassName;

            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = "Profile updated successfully";

            return RedirectToAction(nameof(EditProfile));
        }


    }
}
