using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using System.Security.Claims;
using System.Text;


namespace RSD_E_Learning.Controllers
{
    public class TeacherController : Controller
    {
        private readonly DB _context;
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<TeacherController> _logger;

        public TeacherController(
            DB context,
            IWebHostEnvironment environment,
            ILogger<TeacherController> logger)
        {
            _context = context;
            _environment = environment;
            _logger = logger;
        }

        // -------------------- LOGIN --------------------
        [HttpGet]
        public IActionResult TeacherLogin()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> TeacherLogin(string email, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email && u.Role == DB.UserRole.Teacher);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == user.Id);

            if (teacher == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            if (!teacher.IsActive)
            {
                ModelState.AddModelError(
                    "",
                    "Your account has been deactivated. Please contact administrator."
                );
                return View();
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("UserId", user.Id.ToString()),
                new Claim("TeacherId", teacher.TeacherId.ToString()),
                new Claim("Role", "Teacher")
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            return RedirectToAction("TeacherIndex", "Teacher");
        }



        // -------------------- PASSWORD HASHING --------------------
        private string HashPassword(string password)
        {
            byte[] salt = Encoding.UTF8.GetBytes("STATIC-SALT-CHANGE-LATER");

            return Convert.ToBase64String(
                KeyDerivation.Pbkdf2(
                    password,
                    salt,
                    KeyDerivationPrf.HMACSHA256,
                    10000,
                    32
                )
            );
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }


        public IActionResult TeacherIndex()
        {
            return View();
        }

        [HttpGet]

        public async Task<IActionResult> CreateCourse()
        {
            var model = new CreateCourseVm
            {
                CategoryList = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCourse(CreateCourseVm model)
        {
            if (!ModelState.IsValid)
            {
                model.CategoryList = await _context.Categories
                    .OrderBy(c => c.Name)
                    .Select(c => new SelectListItem
                    {
                        Value = c.CategoryId.ToString(),
                        Text = c.Name
                    })
                    .ToListAsync();

                return View(model);
            }

            // Get current logged-in user id from claims
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
            if (userIdClaim == null)
            {
                TempData["ErrorMessage"] = "User not found. Please login again.";
                return RedirectToAction("TeacherLogin");
            }

            int userId = int.Parse(userIdClaim.Value);

            // Get teacher profile
            var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
            if (teacher == null)
            {
                TempData["ErrorMessage"] = "Teacher profile not found.";
                return RedirectToAction("TeacherLogin");
            }

            var course = new DB.Course
            {
                Title = model.Title,
                CategoryId = model.CategoryId,
                Description = model.Description,
                TeacherId = teacher.TeacherId,

                IsApproved = false,
                IsPublished = false
            };


            _context.Courses.Add(course);
            await _context.SaveChangesAsync();   // ✅ COURSE SAVED SUCCESSFULLY

            TempData["SuccessMessage"] =
                "Course submitted successfully and pending admin approval.";

            return RedirectToAction("UploadMaterial",
    "TeacherController",            
    new { courseId = course.CourseId });
        }

        [HttpGet]
        public async Task<IActionResult> UploadMaterial(int courseId)
        {
            var course = await _context.Courses.FindAsync(courseId);

            if (course == null || !course.IsApproved)
            {
                return Unauthorized();
            }

            var vm = new UploadCourseFileVm
            {
                CourseId = courseId
            };

            ViewBag.CourseTitle = course.Title;
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadMaterial(UploadCourseFileVm model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var teacherIdClaim = User.FindFirst("TeacherId");
            if (teacherIdClaim == null)
                return Unauthorized();

            int teacherId = int.Parse(teacherIdClaim.Value);

            // 🔐 Ensure course belongs to this teacher
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseId == model.CourseId &&
                                          c.TeacherId == teacherId &&
                                          c.IsApproved);

            if (course == null)
                return Unauthorized();

            // ================= FILE SAVE =================
            string uploadsFolder = Path.Combine(_environment.WebRootPath, "coursefiles");
            Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid() + Path.GetExtension(model.materialFile.FileName);
            string physicalPath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(physicalPath, FileMode.Create))
            {
                await model.materialFile.CopyToAsync(stream);
            }

            // ================= DATABASE SAVE =================
            var courseFile = new DB.CourseFile
            {
                CourseId = model.CourseId,
                FileName = model.materialTitle,
                FilePath = "/coursefiles/" + uniqueFileName,
                FileType = Path.GetExtension(model.materialFile.FileName),
                IsActive = true,
                UpdateAt = DateTime.UtcNow
            };

            _context.CourseFiles.Add(courseFile);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Material uploaded successfully.";
            return RedirectToAction("ViewCourse", new { id = model.CourseId });
        }


        public IActionResult ViewCourse()
        {
            return View();
        }

        public IActionResult CreateAssessment()
        {
            return View();
        }

        public IActionResult ViewAssessment()
        {
            return View();
        }

        public IActionResult TeacherDashboard()
        {
            return View();
        }

    }
}