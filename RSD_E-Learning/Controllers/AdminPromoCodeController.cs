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

    // ===================== MAIN PAGE =====================
    public IActionResult Index()
    {
        return View();
    }

    // ===================== AJAX LIST =====================
    [HttpGet]
    public async Task<IActionResult> AjaxList(string? search, string? status)
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

            case "Inactive":
                query = query.Where(p =>
                    !p.IsActive &&
                    p.ExpiryDate >= today);
                break;

            case "Expired":
                query = query.Where(p => p.ExpiryDate < today);
                break;

            case "Upcoming":
                query = query.Where(p => p.StartDate > today);
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

    // ===================== AJAX TOGGLE =====================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleAjax([FromBody] ToggleVm vm)
    {
        var promo = await _db.PromoCodes.FindAsync(vm.Id);
        if (promo == null)
            return Json(new { success = false });

        var today = DateTime.UtcNow.Date;

        // Do not allow expired promo to toggle
        if (promo.ExpiryDate < today)
        {
            return Json(new
            {
                success = false,
                message = "Expired promo codes cannot be activated."
            });
        }

        promo.IsActive = !promo.IsActive;

        _db.AuditLogs.Add(new DB.AuditLog
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
