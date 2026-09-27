using System.Data;
using HotelBooking.Api.Common;
using HotelBooking.Api.Data;
using HotelBooking.Api.Domain;
using HotelBooking.Api.DTOs;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Services;

public interface IBookingService
{
    Task<Booking> CreateAsync(Guid userId, CreateBookingRequest request, CancellationToken cancellationToken);
}

public sealed class BookingService(HotelDbContext db) : IBookingService
{
    public async Task<Booking> CreateAsync(
        Guid userId, CreateBookingRequest request, CancellationToken cancellationToken)
    {
        if (request.CheckInDate < DateOnly.FromDateTime(DateTime.UtcNow))
            throw new ApiException(400, "Check-in cannot be in the past.");
        if (request.CheckOutDate <= request.CheckInDate)
            throw new ApiException(400, "Check-out must be after check-in.");

        var strategy = db.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await db.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);

            var customer = await db.Users.SingleOrDefaultAsync(x => x.Id == userId, cancellationToken)
                ?? throw new ApiException(404, "Customer was not found.");

            var room = await db.Rooms.Include(x => x.RoomType)
                .SingleOrDefaultAsync(x => x.Id == request.RoomId && x.IsActive, cancellationToken)
                ?? throw new ApiException(404, "Room was not found.");

            if (request.Guests > room.RoomType.Capacity)
                throw new ApiException(400, "The selected room cannot accommodate this number of guests.");

            var overlaps = await db.Bookings.AnyAsync(x =>
                x.RoomId == room.Id &&
                x.Status != BookingStatus.Cancelled &&
                x.CheckInDate < request.CheckOutDate &&
                request.CheckInDate < x.CheckOutDate, cancellationToken);
            if (overlaps)
                throw new ApiException(409, "The room is no longer available for these dates.");

            var nights = request.CheckOutDate.DayNumber - request.CheckInDate.DayNumber;
            var settings = await db.HotelSettings.AsNoTracking().SingleAsync(cancellationToken);
            var subtotal = nights * room.RoomType.PricePerNight;
            PromoCode? promo = null;
            decimal discount = 0;
            if (!string.IsNullOrWhiteSpace(request.PromoCode))
            {
                var code = request.PromoCode.Trim().ToUpperInvariant();
                promo = await db.PromoCodes.SingleOrDefaultAsync(x => x.Code == code, cancellationToken)
                    ?? throw new ApiException(400, "Promo code is invalid.");
                var now = DateTime.UtcNow;
                if (!promo.IsActive || promo.ValidFromUtc > now || promo.ValidUntilUtc < now ||
                    promo.TimesUsed >= promo.MaxUses || nights < promo.MinimumNights)
                    throw new ApiException(400, "Promo code is expired or not applicable to this booking.");
                discount = subtotal * promo.DiscountPercentage / 100m;
                if (promo.MaxDiscountAmount.HasValue)
                    discount = Math.Min(discount, promo.MaxDiscountAmount.Value);
                promo.TimesUsed++;
            }
            var taxableAmount = subtotal - discount;
            var tax = Math.Round(taxableAmount * settings.TaxPercentage / 100m, 2);
            var total = taxableAmount + tax;
            var booking = new Booking
            {
                ReferenceNumber = $"HB-{Guid.NewGuid():N}"[..11].ToUpperInvariant(),
                UserId = userId,
                RoomId = room.Id,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                Guests = request.Guests,
                GuestName = request.GuestName.Trim(),
                GuestEmail = request.GuestEmail.Trim().ToLowerInvariant(),
                GuestPhone = request.GuestPhone.Trim(),
                SpecialRequests = request.SpecialRequests?.Trim(),
                Subtotal = subtotal,
                DiscountAmount = discount,
                TaxAmount = tax,
                TotalPrice = total,
                Status = BookingStatus.Pending,
                Room = room,
                User = customer,
                PromoCode = promo,
                Payment = new Payment
                {
                    Amount = total,
                    Method = request.PaymentMethod,
                    Status = PaymentStatus.Pending
                }
            };
            db.Bookings.Add(booking);
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return booking;
        });
    }
}
