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
            try
            {
                // Get current logged-in user id from claims
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
                if (userIdClaim == null)
                {
                    TempData["ErrorMessage"] = "User not found. Please login again.";
                    return RedirectToAction("TeacherLogin");
                }

                int userId = int.Parse(userIdClaim.Value);

                // Get the Teacher record linked to this User
                var teacher = await _context.Teachers.FirstOrDefaultAsync(t => t.UserId == userId);
                if (teacher == null)
                {
                    TempData["ErrorMessage"] = "Teacher profile not found.";
                    return RedirectToAction("TeacherLogin");
                }

                // Create new course with the current teacher's ID
                var course = new DB.Course
                {
                    Title = model.Title,
                    CategoryId = model.CategoryId,
                    Description = model.Description,
                    TeacherId = teacher.TeacherId,

                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                // Process course materials
                /*
                if (materialFile != null && materialFile.Count > 0)
                {
                    for (int i = 0; i < materialFile.Count; i++)
                    {
                        var file = materialFile[i];

                        if (file != null && file.Length > 0)
                        {
                            var type = i < materialType.Count ? materialType[i] : "document";
                            var title = i < materialTitle.Count && !string.IsNullOrWhiteSpace(materialTitle[i])
                                ? materialTitle[i]
                                : file.FileName;

                            // Save file and create records
                            /*
                            var filePath = await SaveCourseFileAsync(file, course.CourseId, type);

                            if (!string.IsNullOrEmpty(filePath))
                            {
                                // Create Lesson record
                                var lesson = new Lesson
                                {
                                    CourseId = course.CourseId,
                                    Title = title,
                                    FilePath = filePath,
                                    Content = $"Material Type: {type}"
                                };
                                _context.Lessons.Add(lesson);

                                // Create CourseFile record
                                var courseFile = new CourseFile
                                {
                                    CourseId = course.CourseId,
                                    //TeacherId = teacherId.Value,//
                                    FileName = file.FileName,
                                    FilePath = filePath,
                                    IsActive = true,
                                    UpdateAt = DateTime.UtcNow
                                };
                                _context.CourseFiles.Add(courseFile);
                            } 
                            
                            
                        } 
                
                    }
                    

                    await _context.SaveChangesAsync();
                } */

                TempData["SuccessMessage"] = "Course created successfully!";
                return RedirectToAction("CreateCourse");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating course");
                TempData["ErrorMessage"] = ex.ToString();
                return RedirectToAction("CreateCourse");
            } 
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