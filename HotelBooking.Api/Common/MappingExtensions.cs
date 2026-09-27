using HotelBooking.Api.Domain;
using HotelBooking.Api.DTOs;

namespace HotelBooking.Api.Common;

public static class MappingExtensions
{
    public static RoomResponse ToResponse(this Room room) => new(
        room.Id, room.RoomNumber, room.Floor, room.IsActive,
        room.RoomTypeId, room.RoomType.Name, room.RoomType.Capacity, room.RoomType.PricePerNight);

    public static BookingResponse ToResponse(this Booking booking) => new(
        booking.Id, booking.ReferenceNumber, booking.CheckInDate, booking.CheckOutDate,
        booking.Guests, booking.CheckOutDate.DayNumber - booking.CheckInDate.DayNumber,
        booking.GuestName, booking.GuestEmail, booking.GuestPhone, booking.SpecialRequests,
        booking.Subtotal, booking.DiscountAmount, booking.TaxAmount, booking.TotalPrice,
        booking.Status, booking.Payment?.Method ?? PaymentMethod.CashAtHotel,
        booking.Payment?.Status ?? PaymentStatus.Pending, booking.RoomId, booking.Room.RoomNumber,
        booking.Room.RoomType.Name, booking.UserId, booking.User.FullName, booking.CreatedAtUtc);
}
