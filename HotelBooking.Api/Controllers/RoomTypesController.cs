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
[Route("api/room-types")]
public sealed class RoomTypesController(HotelDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RoomTypeResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var items = await db.RoomTypes.AsNoTracking()
            .OrderBy(x => x.PricePerNight)
            .Select(x => new RoomTypeResponse(
                x.Id, x.Name, x.Description, x.Capacity, x.PricePerNight, x.ImageUrl,
                x.Rooms.Count(r => r.IsActive),
                x.Rooms.SelectMany(r => r.Bookings).Where(b => b.Review != null)
                    .Select(b => (double?)b.Review!.Rating).Average() ?? 0,
                x.RoomTypeAmenities.Where(a => a.Amenity.IsActive)
                    .Select(a => new AmenityResponse(a.Amenity.Id, a.Amenity.Name, a.Amenity.Icon, a.Amenity.IsActive)).ToList()))
            .ToListAsync(cancellationToken);
        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<RoomTypeResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var item = await db.RoomTypes.AsNoTracking().Where(x => x.Id == id)
            .Select(x => new RoomTypeResponse(
                x.Id, x.Name, x.Description, x.Capacity, x.PricePerNight, x.ImageUrl,
                x.Rooms.Count(r => r.IsActive),
                x.Rooms.SelectMany(r => r.Bookings).Where(b => b.Review != null)
                    .Select(b => (double?)b.Review!.Rating).Average() ?? 0,
                x.RoomTypeAmenities.Where(a => a.Amenity.IsActive)
                    .Select(a => new AmenityResponse(a.Amenity.Id, a.Amenity.Name, a.Amenity.Icon, a.Amenity.IsActive)).ToList()))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new ApiException(404, "Room type was not found.");
        return Ok(item);
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPost]
    public async Task<ActionResult> Create(RoomTypeRequest request, CancellationToken cancellationToken)
    {
        var item = new RoomType
        {
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Capacity = request.Capacity,
            PricePerNight = request.PricePerNight,
            ImageUrl = request.ImageUrl
        };
        var amenityIds = request.AmenityIds?.Distinct().ToList() ?? [];
        if (amenityIds.Count > 0 &&
            await db.Amenities.CountAsync(x => amenityIds.Contains(x.Id), cancellationToken) != amenityIds.Count)
            throw new ApiException(400, "One or more amenities are invalid.");
        item.RoomTypeAmenities = amenityIds
            .Select(id => new RoomTypeAmenity { AmenityId = id }).ToList();
        db.RoomTypes.Add(item);
        await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, new { item.Id });
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RoomTypeRequest request, CancellationToken cancellationToken)
    {
        var item = await db.RoomTypes.Include(x => x.RoomTypeAmenities)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ApiException(404, "Room type was not found.");
        item.Name = request.Name.Trim();
        item.Description = request.Description.Trim();
        item.Capacity = request.Capacity;
        item.PricePerNight = request.PricePerNight;
        item.ImageUrl = request.ImageUrl;
        var amenityIds = request.AmenityIds?.Distinct().ToList() ?? [];
        if (amenityIds.Count > 0 &&
            await db.Amenities.CountAsync(x => amenityIds.Contains(x.Id), cancellationToken) != amenityIds.Count)
            throw new ApiException(400, "One or more amenities are invalid.");
        db.RoomTypeAmenities.RemoveRange(item.RoomTypeAmenities);
        item.RoomTypeAmenities = amenityIds
            .Select(amenityId => new RoomTypeAmenity { RoomTypeId = id, AmenityId = amenityId }).ToList();
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPatch("{id:guid}/price")]
    public async Task<IActionResult> UpdatePrice(
        Guid id, UpdateRoomPriceRequest request, CancellationToken cancellationToken)
    {
        var item = await db.RoomTypes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ApiException(404, "Room type was not found.");
        item.PricePerNight = request.PricePerNight;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPatch("{id:guid}/details")]
    public async Task<IActionResult> UpdateDetails(
        Guid id, UpdateRoomTypeDetailsRequest request, CancellationToken cancellationToken)
    {
        var item = await db.RoomTypes.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ApiException(404, "Room type was not found.");
        item.PricePerNight = request.PricePerNight;
        item.Capacity = request.Capacity;
        item.ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? null : request.ImageUrl.Trim();
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var item = await db.RoomTypes.Include(x => x.Rooms)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ApiException(404, "Room type was not found.");
        if (item.Rooms.Count != 0)
            throw new ApiException(409, "Delete or move the rooms that use this type first.");
        db.RoomTypes.Remove(item);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
