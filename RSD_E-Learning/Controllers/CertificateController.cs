using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

public class CertificateController : Controller
{
    private readonly DB _db;

    public CertificateController(DB db)
    {
        _db = db;
    }

    // ================= STUDENT: MY CERTIFICATES =================
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyCertificates()
    {
        var studentId = int.Parse(User.FindFirst("StudentId")!.Value);

        var certs = await _db.Certificates
            .Include(c => c.Course).ThenInclude(c => c!.Category)
            .Include(c => c.Course).ThenInclude(c => c!.Teacher).ThenInclude(t => t!.User)
            .Where(c => c.StudentId == studentId)
            .OrderByDescending(c => c.IssuedDate)
            .ToListAsync();

        return View(certs);
    }

    // ================= VIEW CERTIFICATE =================
    [AllowAnonymous]
    public async Task<IActionResult> View(int id)
    {
        var cert = await _db.Certificates
            .Include(c => c.Student).ThenInclude(s => s!.User)
            .Include(c => c.Course).ThenInclude(c => c!.Category)
            .Include(c => c.Course).ThenInclude(c => c!.Teacher).ThenInclude(t => t!.User)
            .FirstOrDefaultAsync(c => c.CertificateId == id);

        if (cert == null) return NotFound();

        var settings = await _db.SystemSettings.FirstAsync();
        ViewBag.Template = settings.CertificateTemplatePath ?? "Default";

        ViewBag.QRCodeUrl = GenerateQRCodeUrl(
            Url.Action("Verify", "Certificate", new { id }, Request.Scheme)!
        );

        return View(cert);
    }

    // ================= VERIFY =================
    [AllowAnonymous]
    public async Task<IActionResult> Verify(int id)
    {
        var cert = await _db.Certificates
            .Include(c => c.Student).ThenInclude(s => s!.User)
            .Include(c => c.Course).ThenInclude(c => c!.Category)
            .Include(c => c.Course).ThenInclude(c => c!.Teacher).ThenInclude(t => t!.User)
            .FirstOrDefaultAsync(c => c.CertificateId == id);

        ViewBag.IsValid = cert != null;
        ViewBag.Message = cert != null
            ? "This certificate is valid and authentic."
            : "Certificate not found.";

        return View(cert);
    }

    // ================= QR =================
    private string GenerateQRCodeUrl(string url)
    {
        return $"https://chart.googleapis.com/chart?cht=qr&chs=200x200&chl={Uri.EscapeDataString(url)}";
    }
}
