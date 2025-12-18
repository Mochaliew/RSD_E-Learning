using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using System.Text;



var builder = WebApplication.CreateBuilder(args);

// Register Certificate Service
builder.Services.AddScoped<RSD_E_Learning.Services.ICertificateService,
                           RSD_E_Learning.Services.CertificateService>();

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

    // Seed admin if not exists
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
