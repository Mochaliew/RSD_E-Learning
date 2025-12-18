using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using System.Text;
using static RSD_E_Learning.Models.DB;



var builder = WebApplication.CreateBuilder(args);

// Register Certificate Service
builder.Services.AddScoped<RSD_E_Learning.Services.ICertificateService,
                           RSD_E_Learning.Services.CertificateService>();

// Email
builder.Services.AddScoped<IEmailService, EmailService>();


// Add services
builder.Services.AddControllersWithViews();

// Register DbContext
builder.Services.AddDbContext<DB>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied"; // ⭐ THIS WAS MISSING
        options.ExpireTimeSpan = TimeSpan.FromHours(24);
    });

// Add API Controllers support if not already added
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
    });



var app = builder.Build(); // ✅ now in the correct place

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<DB>();

    // Ensure database is created
    try
    {
        db.Database.Migrate();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Migration error: " + ex.Message);
    }

    // Seed admin
    if (!db.Users.Any(u => u.Email == "admin@elearning.com"))
    {
        var adminUser = new DB.User
        {
            FullName = "System Administrator",
            Email = "admin@elearning.com",
            PasswordHash = Convert.ToBase64String(
                Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivation.Pbkdf2(
                    "admin123",
                    Encoding.UTF8.GetBytes("STATIC-SALT-CHANGE-LATER"),
                    Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA256,
                    10000,
                    32)),
            Role = DB.UserRole.Admin,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(adminUser);
        db.SaveChanges();

        db.Admins.Add(new DB.Admin
        {
            UserId = adminUser.Id
        });

        db.SaveChanges();
    }

    // Seed teacher if not exists
    if (!db.Users.Any(u => u.Email == "teacher@elearning.com"))
    {
        var teacherUser = new DB.User
        {
            FullName = "Default Teacher",
            Email = "teacher@elearning.com",
            PasswordHash = Convert.ToBase64String(
                Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivation.Pbkdf2(
                    "teacher123",
                    Encoding.UTF8.GetBytes("STATIC-SALT-CHANGE-LATER"),
                    Microsoft.AspNetCore.Cryptography.KeyDerivation.KeyDerivationPrf.HMACSHA256,
                    10000,
                    32)),
            Role = DB.UserRole.Teacher,
            CreatedAt = DateTime.UtcNow
        };

        db.Users.Add(teacherUser);
        db.SaveChanges(); // needed to generate UserId

        db.Teachers.Add(new DB.Teacher
        {
            UserId = teacherUser.Id,
            // add more fields if Teacher has them
            // e.g. Department = "IT",
            // Phone = "0123456789"
        });

        db.SaveChanges();
    }
    // Seed IT categories if none exist
    if (!db.Categories.Any())
    {
        var categories = new List<DB.Category>
    {
        new DB.Category
        {
            Name = "Software Development",
            Description = "Covers programming, application development, and software engineering concepts."
        },
        new DB.Category
        {
            Name = "Networking & Security",
            Description = "Focuses on computer networks, cybersecurity fundamentals, and system protection."
        },
        new DB.Category
        {
            Name = "Data & AI",
            Description = "Includes data analysis, machine learning, and artificial intelligence topics."
        }
    };

        db.Categories.AddRange(categories);
        db.SaveChanges();
    }

    // Seed System Settings
    if (!db.SystemSettings.Any())
    {
        db.SystemSettings.Add(new SystemSetting
        {
            PlatformName = "RSD E-Learning",
            PrimaryColor = "#0d6efd",
            StorageType = "Local",
            MaxUploadSizeMB = 50,
            AllowedFileTypes = ".pdf,.jpg,.png",
            EnableEmailNotification = false,
            SmtpPort = 587,
            CertificateTemplatePath = "Templates/Certificate/Default.pdf"
        });

        db.SaveChanges();
    }

}


// Enable API Controllers
app.MapControllers();

// Enable CORS if configuredS
// app.UseCors("AllowAll");

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
