using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;
using static RSD_E_Learning.Models.DB;

[Authorize(Roles = "Admin")]
public class AdminPromoCodeController : Controller
{
    private readonly DB _db;

    public AdminPromoCodeController(DB db)
    {
        _db = db;
    }

    // ===================== LIST =====================
    public async Task<IActionResult> Index()
    {
        var promos = await _db.PromoCodes
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new PromoCodeListVm
            {
                PromoCodeId = p.PromoCodeId,
                Code = p.Code,
                DiscountPercent = p.DiscountPercent,
                ExpiryDate = p.ExpiryDate,
                IsActive = p.IsActive,
                UsedCount = p.UsedCount
            })
            .ToListAsync();

        return View(promos);
    }

    // ===================== CREATE (GET) =====================
    public IActionResult Create()
    {
        return View();
    }

    // ===================== CREATE (POST) =====================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PromoCodeCreateVm vm)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var promo = new PromoCode
        {
            Code = vm.Code.ToUpper(),
            DiscountPercent = vm.DiscountPercent,
            ExpiryDate = vm.ExpiryDate,
            MaxUsage = vm.MaxUsage,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.PromoCodes.Add(promo);

        //AUDIT LOG
        _db.AuditLogs.Add(new AuditLog
        {
            Action = $"Created promo code: {promo.Code}",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ===================== TOGGLE ACTIVE =====================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var promo = await _db.PromoCodes.FindAsync(id);
        if (promo == null)
            return NotFound();

        promo.IsActive = !promo.IsActive;

        //AUDIT LOG
        _db.AuditLogs.Add(new AuditLog
        {
            Action = promo.IsActive
                ? $"Activated promo code: {promo.Code}"
                : $"Deactivated promo code: {promo.Code}",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
