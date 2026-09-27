using HotelBooking.Api.Data;
using HotelBooking.Api.DTOs;
using HotelBooking.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/hotel")]
public sealed class HotelController(HotelDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<HotelSettingsResponse>> Get(CancellationToken cancellationToken)
    {
        var item = await db.HotelSettings.AsNoTracking().SingleAsync(cancellationToken);
        return Ok(ToResponse(item));
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPut]
    public async Task<IActionResult> Update(HotelSettingsRequest request, CancellationToken cancellationToken)
    {
        var item = await db.HotelSettings.SingleAsync(cancellationToken);
        item.HotelName = request.HotelName.Trim();
        item.Description = request.Description.Trim();
        item.Address = request.Address.Trim();
        item.Phone = request.Phone.Trim();
        item.Email = request.Email.Trim().ToLowerInvariant();
        item.Currency = request.Currency.Trim().ToUpperInvariant();
        item.CheckInTime = request.CheckInTime;
        item.CheckOutTime = request.CheckOutTime;
        item.FreeCancellationHours = request.FreeCancellationHours;
        item.TaxPercentage = request.TaxPercentage;
        item.PrimaryColor = request.PrimaryColor.ToLowerInvariant();
        item.AccentColor = request.AccentColor.ToLowerInvariant();
        item.HomeHeroImageUrl = request.HomeHeroImageUrl.Trim();
        item.RoomsHeroImageUrl = request.RoomsHeroImageUrl.Trim();
        item.AccountHeroImageUrl = request.AccountHeroImageUrl.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private static HotelSettingsResponse ToResponse(Domain.HotelSettings x) => new(
        x.Id, x.HotelName, x.Description, x.Address, x.Phone, x.Email, x.Currency,
        x.CheckInTime, x.CheckOutTime, x.FreeCancellationHours, x.TaxPercentage,
        x.PrimaryColor, x.AccentColor, x.HomeHeroImageUrl, x.RoomsHeroImageUrl,
        x.AccountHeroImageUrl);
}
