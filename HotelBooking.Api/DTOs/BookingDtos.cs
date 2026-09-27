using System.ComponentModel.DataAnnotations;
using HotelBooking.Api.Domain;

namespace HotelBooking.Api.DTOs;

public sealed record CreateBookingRequest(
    Guid RoomId,
    [Required] DateOnly CheckInDate,
    [Required] DateOnly CheckOutDate,
    [Range(1, 20)] int Guests,
    [Required, MaxLength(120)] string GuestName,
    [Required, EmailAddress, MaxLength(180)] string GuestEmail,
    [Required, Phone, MaxLength(30)] string GuestPhone,
    [MaxLength(1000)] string? SpecialRequests,
    [MaxLength(40)] string? PromoCode,
    PaymentMethod PaymentMethod = PaymentMethod.CashAtHotel);

public sealed record BookingResponse(
    Guid Id, string ReferenceNumber, DateOnly CheckInDate, DateOnly CheckOutDate,
    int Guests, int Nights, string GuestName, string GuestEmail, string GuestPhone,
    string? SpecialRequests, decimal Subtotal, decimal DiscountAmount, decimal TaxAmount,
    decimal TotalPrice, BookingStatus Status, PaymentMethod PaymentMethod, PaymentStatus PaymentStatus,
    Guid RoomId, string RoomNumber, string RoomType, Guid CustomerId,
    string CustomerName, DateTime CreatedAtUtc);

public sealed class BookingListQuery
{
    public BookingStatus? Status { get; init; }
    [Range(1, 100)] public int Page { get; init; } = 1;
    [Range(1, 50)] public int PageSize { get; init; } = 10;
}

public sealed record UpdateBookingStatusRequest(BookingStatus Status);
public sealed record CancelBookingRequest([MaxLength(500)] string? Reason);

public sealed record UpdatePaymentRequest(
    PaymentStatus Status,
    [MaxLength(150)] string? TransactionReference);

public sealed record ReviewRequest(
    Guid BookingId,
    [Range(1, 5)] int Rating,
    [MaxLength(1000)] string? Comment);

public sealed record UpdateReviewVisibilityRequest(bool IsVisible);

public sealed record ReviewResponse(
    Guid Id, int Rating, string? Comment, string CustomerName,
    string RoomType, DateTime CreatedAtUtc);

public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public sealed record PromoCodeRequest(
    [Required, MaxLength(40)] string Code,
    [Range(0.01, 100)] decimal DiscountPercentage,
    [Range(0, 1000000)] decimal? MaxDiscountAmount,
    [Range(1, 100)] int MinimumNights,
    DateTime ValidFromUtc,
    DateTime ValidUntilUtc,
    [Range(1, 1000000)] int MaxUses,
    bool IsActive = true);

public sealed record PromoCodeResponse(
    Guid Id, string Code, decimal DiscountPercentage, decimal? MaxDiscountAmount,
    int MinimumNights, DateTime ValidFromUtc, DateTime ValidUntilUtc,
    int MaxUses, int TimesUsed, bool IsActive);

public sealed record HotelSettingsRequest(
    [Required, MaxLength(150)] string HotelName,
    [MaxLength(1500)] string Description,
    [Required, MaxLength(300)] string Address,
    [Required, Phone, MaxLength(30)] string Phone,
    [Required, EmailAddress, MaxLength(180)] string Email,
    [Required, StringLength(3, MinimumLength = 3)] string Currency,
    TimeOnly CheckInTime,
    TimeOnly CheckOutTime,
    [Range(0, 720)] int FreeCancellationHours,
    [Range(0, 100)] decimal TaxPercentage,
    [RegularExpression("^#[0-9A-Fa-f]{6}$")] string PrimaryColor,
    [RegularExpression("^#[0-9A-Fa-f]{6}$")] string AccentColor,
    [Required, MaxLength(500), Url] string HomeHeroImageUrl,
    [Required, MaxLength(500), Url] string RoomsHeroImageUrl,
    [Required, MaxLength(500), Url] string AccountHeroImageUrl);

public sealed record HotelSettingsResponse(
    Guid Id, string HotelName, string Description, string Address, string Phone,
    string Email, string Currency, TimeOnly CheckInTime, TimeOnly CheckOutTime,
    int FreeCancellationHours, decimal TaxPercentage, string PrimaryColor, string AccentColor,
    string HomeHeroImageUrl, string RoomsHeroImageUrl, string AccountHeroImageUrl);

public sealed record DashboardResponse(
    int TotalRooms, int ActiveBookings, int PendingBookings, int Customers,
    decimal PaidRevenue, decimal OccupancyRate,
    IReadOnlyList<BookingResponse> RecentBookings);
