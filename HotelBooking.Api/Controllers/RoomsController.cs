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
[Route("api/rooms")]
public sealed class RoomsController(HotelDbContext db) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<PagedResult<RoomResponse>>> Search(
        [FromQuery] RoomSearchQuery query, CancellationToken cancellationToken)
    {
        if (query.CheckIn < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ApiException(400, "Check-in cannot be in the past.");
        if (query.CheckOut <= query.CheckIn)
            throw new ApiException(400, "Check-out must be after check-in.");

        var rooms = db.Rooms.AsNoTracking()
            .Include(x => x.RoomType)
            .Where(x => x.IsActive && x.RoomType.Capacity >= query.Guests)
            .Where(x => !x.Bookings.Any(b =>
                b.Status != BookingStatus.Cancelled &&
                b.CheckInDate < query.CheckOut && query.CheckIn < b.CheckOutDate));

        if (query.MaxPricePerNight.HasValue)
            rooms = rooms.Where(x => x.RoomType.PricePerNight <= query.MaxPricePerNight.Value);

        var total = await rooms.CountAsync(cancellationToken);
        var entities = await rooms.OrderBy(x => x.RoomType.PricePerNight).ThenBy(x => x.RoomNumber)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);
        return Ok(new PagedResult<RoomResponse>(
            entities.Select(x => x.ToResponse()).ToList(), query.Page, query.PageSize, total));
    }

    [Authorize(Policy = AuthorizationPolicies.FrontDesk)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<RoomResponse>>> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var query = db.Rooms.AsNoTracking().Include(x => x.RoomType);
        var total = await query.CountAsync(cancellationToken);
        var rooms = await query.OrderBy(x => x.RoomNumber)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return Ok(new PagedResult<RoomResponse>(
            rooms.Select(x => x.ToResponse()).ToList(), page, pageSize, total));
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPost]
    public async Task<ActionResult> Create(RoomRequest request, CancellationToken cancellationToken)
    {
        var roomNumber = request.RoomNumber.Trim();
        if (await db.Rooms.AnyAsync(x => x.RoomNumber == roomNumber, cancellationToken))
            throw new ApiException(409, "Room number already exists.");
        if (!await db.RoomTypes.AnyAsync(x => x.Id == request.RoomTypeId, cancellationToken))
            throw new ApiException(404, "Room type was not found.");
        var room = new Room
        {
            RoomNumber = roomNumber,
            Floor = request.Floor,
            RoomTypeId = request.RoomTypeId,
            IsActive = request.IsActive
        };
        db.Rooms.Add(room);
        await db.SaveChangesAsync(cancellationToken);
        return Created($"api/rooms/{room.Id}", new { room.Id });
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, RoomRequest request, CancellationToken cancellationToken)
    {
        var room = await db.Rooms.FindAsync([id], cancellationToken)
            ?? throw new ApiException(404, "Room was not found.");
        var roomNumber = request.RoomNumber.Trim();
        if (await db.Rooms.AnyAsync(x => x.Id != id && x.RoomNumber == roomNumber, cancellationToken))
            throw new ApiException(409, "Room number already exists.");
        if (!await db.RoomTypes.AnyAsync(x => x.Id == request.RoomTypeId, cancellationToken))
            throw new ApiException(404, "Room type was not found.");
        room.RoomNumber = roomNumber;
        room.Floor = request.Floor;
        room.RoomTypeId = request.RoomTypeId;
        room.IsActive = request.IsActive;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
