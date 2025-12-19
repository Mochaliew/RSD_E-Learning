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
    public async Task<IActionResult> Index(string? search, string? status)
    {
        var today = DateTime.UtcNow.Date;
        var query = _db.PromoCodes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Code.Contains(search));
        }

        switch (status)
        {
            case "Active":
                query = query.Where(p =>
                    p.IsActive &&
                    p.StartDate <= today &&
                    p.ExpiryDate >= today);
                break;

            case "Expired":
                query = query.Where(p => p.ExpiryDate < today);
                break;

            case "Upcoming":
                query = query.Where(p => p.StartDate > today);
                break;
        }

        var vm = new PromoCodeFilterVm
        {
            Search = search,
            Status = status,
            PromoCodes = await query
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PromoCodeListVm
                {
                    PromoCodeId = p.PromoCodeId,
                    Code = p.Code,
                    DiscountPercent = p.DiscountPercent,
                    StartDate = p.StartDate,
                    ExpiryDate = p.ExpiryDate,
                    IsActive = p.IsActive,
                    UsedCount = p.UsedCount
                })
                .ToListAsync()
        };

        return View(vm);
    }


    // ===================== CREATE =====================
    public IActionResult Create()
    {
        return View();
    }

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
            StartDate = vm.StartDate,
            ExpiryDate = vm.ExpiryDate,
            MaxUsage = vm.MaxUsage,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.PromoCodes.Add(promo);

        _db.AuditLogs.Add(new AuditLog
        {
            Action = $"Created promo code: {promo.Code}",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ===================== TOGGLE =====================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Toggle(int id)
    {
        var promo = await _db.PromoCodes.FindAsync(id);
        if (promo == null) return NotFound();

        promo.IsActive = !promo.IsActive;

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

    // ===================== AJAX =====================
    [HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> ToggleAjax([FromBody] ToggleVm vm)
{
    var promo = await _db.PromoCodes.FindAsync(vm.Id);
    if (promo == null)
        return Json(new { success = false });

    promo.IsActive = !promo.IsActive;

    _db.AuditLogs.Add(new AuditLog
    {
        Action = promo.IsActive
            ? $"Activated promo code: {promo.Code}"
            : $"Deactivated promo code: {promo.Code}",
        Timestamp = DateTime.UtcNow
    });

    await _db.SaveChangesAsync();

    return Json(new
    {
        success = true,
        isActive = promo.IsActive
    });
}


}
