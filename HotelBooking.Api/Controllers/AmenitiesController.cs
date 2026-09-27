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
[Route("api/amenities")]
public sealed class AmenitiesController(HotelDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AmenityResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await db.Amenities.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new AmenityResponse(x.Id, x.Name, x.Icon, x.IsActive))
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPost]
    public async Task<ActionResult> Create(AmenityRequest request, CancellationToken cancellationToken)
    {
        var amenity = new Amenity
        {
            Name = request.Name.Trim(),
            Icon = request.Icon?.Trim(),
            IsActive = request.IsActive
        };
        db.Amenities.Add(amenity);
        await db.SaveChangesAsync(cancellationToken);
        return StatusCode(201, new AmenityResponse(amenity.Id, amenity.Name, amenity.Icon, amenity.IsActive));
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, AmenityRequest request, CancellationToken cancellationToken)
    {
        var amenity = await db.Amenities.FindAsync([id], cancellationToken)
            ?? throw new ApiException(404, "Amenity was not found.");
        amenity.Name = request.Name.Trim();
        amenity.Icon = request.Icon?.Trim();
        amenity.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
