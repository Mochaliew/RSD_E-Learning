
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using RSD_E_Learning.Models;


namespace RSD_E_Learning.Controllers
{
    public class AccountController : Controller
    {
        private readonly DB _db;
        private readonly IEmailService _emailService;

        public AccountController(DB db, IEmailService emailService)
        {
            _db = db;
            _emailService = emailService;
        }


        // -------------------- REGISTER --------------------
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(StudentRegisterVm model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // check existing email
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError("Email", "Email already registered.");
                return View(model);
            }

            // create user first
            var user = new DB.User
            {
                FullName = model.FullName,
                Email = model.Email,
                Role = DB.UserRole.Student,
                PasswordHash = HashPassword(model.Password),
                CreatedAt = DateTime.UtcNow
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // create student profile linked to user
            var student = new DB.Student
            {
                UserId = user.Id,
                EnrollmentDate = DateTime.UtcNow
            };

            _db.Students.Add(student);
            await _db.SaveChangesAsync();

            TempData["Success"] = "Registration successful! Please login.";
            return RedirectToAction("Login");
        }


        // -------------------- LOGIN --------------------
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Role == DB.UserRole.Student);

            if (user == null || !VerifyPassword(password, user.PasswordHash))
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            var student = await _db.Students.FirstOrDefaultAsync(s => s.UserId == user.Id);

            if (student == null)
            {
                ModelState.AddModelError("", "Invalid email or password.");
                return View();
            }

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
            {
                ModelState.AddModelError(
                    "",
                    "Your account has been locked. Please contact administrator."
                );
                return View();
            }   

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim("FullName", user.FullName),
                new Claim("UserId", user.Id.ToString()),
                new Claim("StudentId", student.StudentId.ToString()),
                new Claim(ClaimTypes.Role, "Student")

            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(identity)
            );

            return RedirectToAction("Dashboard", "Student");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }



        // -------------------- LOGOUT --------------------
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            return RedirectToAction("Index", "Home");
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

        // -------------------- FORGOT PASSWORD --------------------
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                ModelState.AddModelError("", "Email is required.");
                return View();
            }

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.Role == DB.UserRole.Student);

            // Security: do not reveal existence
            if (user == null)
            {
                TempData["StudentLoginSuccess"] =
                "A reset link has been sent to your email.";

                return RedirectToAction("Login");
            }

            // Generate token
            string token = Guid.NewGuid().ToString("N");

            var resetToken = new DB.PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddMinutes(30),
                IsUsed = false
            };

            _db.PasswordResetTokens.Add(resetToken);
            await _db.SaveChangesAsync();

            //Build reset link
            var resetLink = Url.Action(
                "ResetPassword",
                "Account",
                new { token },
                Request.Scheme
            );

            // SEND EMAIL (USING YOUR EmailService)
            await _emailService.SendAsync(
                user.Email,
                "Reset Your Password",
                $@"
            <p>Hello {user.FullName},</p>
            <p>You requested to reset your password.</p>
            <p>
                <a href='{resetLink}'>Click here to reset your password</a>
            </p>
            <p>This link will expire in 30 minutes.</p>
            <p>If you did not request this, please ignore this email.</p>
        "
            );

            TempData["Success"] =
                "If the email exists, a password reset link has been sent.";

            return RedirectToAction("Login");
        }


        // -------------------- RESET PASSWORD --------------------
        [HttpGet]
        public async Task<IActionResult> ResetPassword(string token)
        {
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login");

            var resetToken = await _db.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == token &&
                    !t.IsUsed &&
                    t.ExpiryDate > DateTime.UtcNow);

            if (resetToken == null)
            {
                TempData["Error"] = "Invalid or expired reset link.";
                return RedirectToAction("Login");
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(
    string token,
    string newPassword,
    string confirmPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ModelState.AddModelError("", "All fields are required.");
                ViewBag.Token = token;
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Passwords do not match.");
                ViewBag.Token = token;
                return View();
            }

            var resetToken = await _db.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t =>
                    t.Token == token &&
                    !t.IsUsed &&
                    t.ExpiryDate > DateTime.UtcNow);

            if (resetToken == null)
            {
                TempData["Error"] = "Invalid or expired reset link.";
                return RedirectToAction("Login");
            }

            resetToken.User.PasswordHash = HashPassword(newPassword);
            resetToken.IsUsed = true;

            await _db.SaveChangesAsync();

            TempData["Success"] = "Password reset successful. Please login.";
            return RedirectToAction("Login");
        }

        private async Task SendEmailAsync(string to, string subject, string body)
        {
            await Task.Run(() =>
            {
                Console.WriteLine("===== PASSWORD RESET EMAIL =====");
                Console.WriteLine($"To: {to}");
                Console.WriteLine($"Subject: {subject}");
                Console.WriteLine(body);
                Console.WriteLine("================================");
            });
        }

        private bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}


