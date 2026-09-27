using HotelBooking.Api.Common;
using HotelBooking.Api.Data;
using HotelBooking.Api.Domain;
using HotelBooking.Api.DTOs;
using HotelBooking.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.Management)]
[Route("api/promo-codes")]
public sealed class PromoCodesController(HotelDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PromoCodeResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await db.PromoCodes.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new PromoCodeResponse(x.Id, x.Code, x.DiscountPercentage,
                x.MaxDiscountAmount, x.MinimumNights, x.ValidFromUtc, x.ValidUntilUtc,
                x.MaxUses, x.TimesUsed, x.IsActive))
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpPost]
    public async Task<ActionResult> Create(PromoCodeRequest request, CancellationToken cancellationToken)
    {
        ValidateDates(request);
        var item = new PromoCode();
        Apply(item, request);
        db.PromoCodes.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return StatusCode(201, new { item.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, PromoCodeRequest request, CancellationToken cancellationToken)
    {
        ValidateDates(request);
        var item = await db.PromoCodes.FindAsync([id], cancellationToken)
            ?? throw new ApiException(404, "Promo code was not found.");
        Apply(item, request);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static void ValidateDates(PromoCodeRequest request)
    {
        if (request.ValidUntilUtc <= request.ValidFromUtc)
            throw new ApiException(400, "Promo code end date must be after its start date.");
    }

    private static void Apply(PromoCode item, PromoCodeRequest request)
    {
        item.Code = request.Code.Trim().ToUpperInvariant();
        item.DiscountPercentage = request.DiscountPercentage;
        item.MaxDiscountAmount = request.MaxDiscountAmount;
        item.MinimumNights = request.MinimumNights;
        item.ValidFromUtc = request.ValidFromUtc.ToUniversalTime();
        item.ValidUntilUtc = request.ValidUntilUtc.ToUniversalTime();
        item.MaxUses = request.MaxUses;
        item.IsActive = request.IsActive;
    }
}
