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
[Route("api/reviews")]
public sealed class ReviewsController(HotelDbContext db) : ControllerBase
{
    [Authorize(Policy = AuthorizationPolicies.CustomerOnly)]
    [HttpPost]
    public async Task<ActionResult> Create(ReviewRequest request, CancellationToken cancellationToken)
    {
        var userId = User.GetUserId();
        var booking = await db.Bookings.Include(x => x.Review)
            .SingleOrDefaultAsync(x => x.Id == request.BookingId && x.UserId == userId, cancellationToken)
            ?? throw new ApiException(404, "Booking was not found.");
        if (booking.Status != BookingStatus.Completed)
            throw new ApiException(409, "You can review only a completed stay.");
        if (booking.Review != null)
            throw new ApiException(409, "This booking has already been reviewed.");

        var review = new Review
        {
            BookingId = booking.Id,
            UserId = userId,
            Rating = request.Rating,
            Comment = request.Comment?.Trim(),
            IsVisible = false
        };
        db.Reviews.Add(review);
        await db.SaveChangesAsync(cancellationToken);
        return StatusCode(201, new { review.Id });
    }

    [HttpGet("room-type/{roomTypeId:guid}")]
    public async Task<ActionResult<PagedResult<ReviewResponse>>> GetForRoomType(
        Guid roomTypeId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var source = db.Reviews.AsNoTracking()
            .Where(x => x.IsVisible && x.Booking.Room.RoomTypeId == roomTypeId);
        var total = await source.CountAsync(cancellationToken);
        var items = await source.OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new ReviewResponse(x.Id, x.Rating, x.Comment,
                x.User.FullName, x.Booking.Room.RoomType.Name, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);
        return Ok(new PagedResult<ReviewResponse>(items, page, pageSize, total));
    }

    [Authorize(Policy = AuthorizationPolicies.Management)]
    [HttpPatch("{id:guid}/visibility")]
    public async Task<IActionResult> SetVisibility(
        Guid id, UpdateReviewVisibilityRequest request, CancellationToken cancellationToken)
    {
        var review = await db.Reviews.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new ApiException(404, "Review was not found.");
        review.IsVisible = request.IsVisible;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
