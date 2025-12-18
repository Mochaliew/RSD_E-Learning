using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

[Authorize(Roles = "Admin")]
public class AdminSystemSettingsController : Controller
{
    private readonly DB _db;

    public AdminSystemSettingsController(DB db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var settings = await _db.SystemSettings.FirstOrDefaultAsync();

        if (settings == null)
        {
            return NotFound("System settings not initialized.");
        }

        var vm = new SystemSettingsVm
        {
            SystemSettingId = settings.SystemSettingId,
            PlatformName = settings.PlatformName,
            PrimaryColor = settings.PrimaryColor,
            StorageType = settings.StorageType,
            MaxUploadSizeMB = settings.MaxUploadSizeMB,
            AllowedFileTypes = settings.AllowedFileTypes,
            EnableEmailNotification = settings.EnableEmailNotification,
            SmtpHost = settings.SmtpHost,
            SmtpPort = settings.SmtpPort,
            SenderEmail = settings.SenderEmail
        };

        return View(vm);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(SystemSettingsVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var s = await _db.SystemSettings.FindAsync(vm.SystemSettingId);

        s!.PlatformName = vm.PlatformName;
        s.PrimaryColor = vm.PrimaryColor;
        s.SmtpHost = vm.SmtpHost;
        s.SmtpPort = vm.SmtpPort;
        s.SenderEmail = vm.SenderEmail;
        s.SmtpPassword = vm.SmtpPassword;
        s.EnableEmailNotification = vm.EnableEmailNotification;
        s.StorageType = vm.StorageType;
        s.MaxUploadSizeMB = vm.MaxUploadSizeMB;
        s.AllowedFileTypes = vm.AllowedFileTypes;
        s.CertificateTemplatePath = vm.CertificateTemplatePath;

        await _db.SaveChangesAsync();

        TempData["Success"] = "System settings updated successfully.";
        return RedirectToAction(nameof(Index));
    }
}
