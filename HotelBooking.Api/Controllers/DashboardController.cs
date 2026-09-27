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
[Route("api/dashboard")]
public sealed class DashboardController(HotelDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DashboardResponse>> Get(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var totalRooms = await db.Rooms.CountAsync(x => x.IsActive, cancellationToken);
        var occupiedRooms = await db.Bookings.CountAsync(x =>
            x.Status == BookingStatus.Confirmed && x.CheckInDate <= today && x.CheckOutDate > today,
            cancellationToken);
        var activeBookings = await db.Bookings.CountAsync(x =>
            x.Status == BookingStatus.Confirmed || x.Status == BookingStatus.Pending,
            cancellationToken);
        var pendingBookings = await db.Bookings.CountAsync(
            x => x.Status == BookingStatus.Pending, cancellationToken);
        var customers = await db.Users.CountAsync(x => x.Role == UserRole.Customer, cancellationToken);
        var paidRevenue = await db.Payments.Where(x => x.Status == PaymentStatus.Paid)
            .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0;
        var recent = await db.Bookings.AsNoTracking()
            .Include(x => x.User).Include(x => x.Payment)
            .Include(x => x.Room).ThenInclude(x => x.RoomType)
            .OrderByDescending(x => x.CreatedAtUtc).Take(5).ToListAsync(cancellationToken);

        var occupancyRate = totalRooms == 0 ? 0 : Math.Round(occupiedRooms * 100m / totalRooms, 2);
        return Ok(new DashboardResponse(totalRooms, activeBookings, pendingBookings,
            customers, paidRevenue, occupancyRate,
            recent.Select(x => x.ToResponse()).ToList()));
    }
}
