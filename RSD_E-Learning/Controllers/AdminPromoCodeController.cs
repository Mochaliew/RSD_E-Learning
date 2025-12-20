using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RSD_E_Learning.Models;

[Authorize(Roles = "Admin")]
public class AdminPromoCodeController : Controller
{
    private readonly DB _db;

    public AdminPromoCodeController(DB db)
    {
        _db = db;
    }

    // ===================== INDEX =====================
    public IActionResult Index()
    {
        return View();
    }

    // ===================== CREATE [GET] =====================
    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    // ===================== CREATE [POST] =====================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PromoCodeCreateVm model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var promo = new DB.PromoCode
        {
            Code = model.Code,
            DiscountPercent = model.DiscountPercent,
            StartDate = model.StartDate,
            ExpiryDate = model.ExpiryDate,
            MaxUsage = model.MaxUsage,   // REQUIRED (1–1000)
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.PromoCodes.Add(promo);

        _db.AuditLogs.Add(new DB.AuditLog
        {
            Action = $"Created promo code: {promo.Code}",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // ===================== AJAX LIST =====================
    [HttpGet]
    public async Task<IActionResult> AjaxList(string? search, string? status)
    {
        var today = DateTime.UtcNow.Date;
        var query = _db.PromoCodes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Code.Contains(search));

        switch (status)
        {
            case "Active":
                query = query.Where(p => p.IsActive && p.StartDate <= today && p.ExpiryDate >= today);
                break;
            case "Inactive":
                query = query.Where(p => !p.IsActive && p.ExpiryDate >= today);
                break;
            case "Upcoming":
                query = query.Where(p => p.StartDate > today);
                break;
            case "Expired":
                query = query.Where(p => p.ExpiryDate < today);
                break;
        }

        var list = await query
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
            .ToListAsync();

        return PartialView("_PromoCodeTable", list);
    }

    // ===================== TOGGLE =====================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAjax([FromBody] ToggleVm vm)
    {
        var promo = await _db.PromoCodes.FindAsync(vm.Id);
        if (promo == null)
            return Json(new { success = false });

        if (promo.ExpiryDate < DateTime.UtcNow.Date)
            return Json(new { success = false, message = "Expired promo codes cannot be activated." });

        promo.IsActive = !promo.IsActive;

        _db.AuditLogs.Add(new DB.AuditLog
        {
            Action = promo.IsActive
                ? $"Activated promo code: {promo.Code}"
                : $"Deactivated promo code: {promo.Code}",
            Timestamp = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
        return Json(new { success = true });
    }
}
