using HotelBooking.Api.Common;
using HotelBooking.Api.Data;
using HotelBooking.Api.Domain;
using HotelBooking.Api.DTOs;
using HotelBooking.Api.Security;
using HotelBooking.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/bookings")]
public sealed class BookingsController(HotelDbContext db, IBookingService bookings) : ControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.CanCreateBooking)]
    [HttpPost]
    public async Task<ActionResult<BookingResponse>> Create(
        CreateBookingRequest request, CancellationToken cancellationToken)
    {
        var booking = await bookings.CreateAsync(User.GetUserId(), request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = booking.Id }, booking.ToResponse());
    }

    [Authorize(Policy = AuthorizationPolicies.CanManageBooking)]
    [HttpGet("mine")]
    public async Task<ActionResult<PagedResult<BookingResponse>>> Mine(
        [FromQuery] BookingListQuery query, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var source = FullQuery().Where(x => x.UserId == userId);
        if (query.Status.HasValue)
            source = source.Where(x => x.Status == query.Status);
        return Ok(await ToPageAsync(source, query, cancellationToken));
    }

    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BookingResponse>> Get(Guid id, CancellationToken cancellationToken)
    {
        var booking = await FullQuery().SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ApiException(404, "Booking was not found.");
        if (!User.IsHotelEmployee() && booking.UserId != User.GetUserId())
            throw new ApiException(403, "You cannot access this booking.");
        return Ok(booking.ToResponse());
    }

    [Authorize(Policy = AuthorizationPolicies.CanManageBooking)]
    [HttpPatch("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(
        Guid id, CancelBookingRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var booking = await db.Bookings.SingleOrDefaultAsync(
            x => x.Id == id, cancellationToken)
            ?? throw new ApiException(404, "Booking was not found.");
        var isEmployee = User.IsHotelEmployee();
        if (!isEmployee && booking.UserId != userId)
            throw new ApiException(403, "You cannot cancel this booking.");
        if (booking.Status is BookingStatus.Cancelled or BookingStatus.Completed)
            throw new ApiException(409, "This booking cannot be cancelled.");
        if (!isEmployee && booking.Status == BookingStatus.Confirmed)
            throw new ApiException(409, "A confirmed booking can be cancelled only by hotel staff.");
        booking.Status = BookingStatus.Cancelled;
        booking.CancelledAtUtc = DateTime.UtcNow;
        booking.CancellationReason = request.Reason?.Trim();
        var payment = await db.Payments.SingleOrDefaultAsync(x => x.BookingId == id, cancellationToken);
        if (payment?.Status == PaymentStatus.Paid)
            payment.Status = PaymentStatus.Refunded;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.FrontDesk)]
    [HttpGet]
    public async Task<ActionResult<PagedResult<BookingResponse>>> GetAll(
        [FromQuery] BookingListQuery query, CancellationToken cancellationToken)
    {
        var source = FullQuery();
        if (query.Status.HasValue)
            source = source.Where(x => x.Status == query.Status);
        return Ok(await ToPageAsync(source, query, cancellationToken));
    }

    [Authorize(Policy = AuthorizationPolicies.FrontDesk)]
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id, UpdateBookingStatusRequest request, CancellationToken cancellationToken)
    {
        if (request.Status == BookingStatus.Pending)
            throw new ApiException(400, "A booking cannot be moved back to Pending.");
        var booking = await db.Bookings.FindAsync([id], cancellationToken)
            ?? throw new ApiException(404, "Booking was not found.");
        if (booking.Status == BookingStatus.Cancelled)
            throw new ApiException(409, "A cancelled booking cannot be changed.");
        if (booking.Status == BookingStatus.Completed && request.Status != BookingStatus.Completed)
            throw new ApiException(409, "A completed booking cannot be changed.");
        booking.Status = request.Status;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [Authorize(Policy = AuthorizationPolicies.FrontDesk)]
    [HttpPatch("{id:guid}/payment")]
    public async Task<IActionResult> UpdatePayment(
        Guid id, UpdatePaymentRequest request, CancellationToken cancellationToken)
    {
        var payment = await db.Payments.SingleOrDefaultAsync(x => x.BookingId == id, cancellationToken)
            ?? throw new ApiException(404, "Payment was not found.");
        payment.Status = request.Status;
        payment.TransactionReference = request.TransactionReference?.Trim();
        payment.PaidAtUtc = request.Status == PaymentStatus.Paid ? DateTime.UtcNow : payment.PaidAtUtc;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private IQueryable<Booking> FullQuery() => db.Bookings.AsNoTracking()
        .Include(x => x.User).Include(x => x.Payment)
        .Include(x => x.Room).ThenInclude(x => x.RoomType);

    private static async Task<PagedResult<BookingResponse>> ToPageAsync(
        IQueryable<Booking> source, BookingListQuery query, CancellationToken cancellationToken)
    {
        var total = await source.CountAsync(cancellationToken);
        var items = await source.OrderByDescending(x => x.CreatedAtUtc)
            .Skip((query.Page - 1) * query.PageSize).Take(query.PageSize)
            .ToListAsync(cancellationToken);
        return new PagedResult<BookingResponse>(
            items.Select(x => x.ToResponse()).ToList(), query.Page, query.PageSize, total);
    }
}
