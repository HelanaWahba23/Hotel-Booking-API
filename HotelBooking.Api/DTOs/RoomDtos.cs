using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Api.DTOs;

public sealed record RoomTypeRequest(
    [Required, MaxLength(80)] string Name,
    [MaxLength(700)] string Description,
    [Range(1, 20)] int Capacity,
    [Range(1, 100000)] decimal PricePerNight,
    [Url, MaxLength(500)] string? ImageUrl,
    IReadOnlyList<Guid>? AmenityIds = null);

public sealed record RoomTypeResponse(
    Guid Id, string Name, string Description, int Capacity,
    decimal PricePerNight, string? ImageUrl, int RoomsCount, double AverageRating,
    IReadOnlyList<AmenityResponse> Amenities);

public sealed record UpdateRoomPriceRequest(
    [Range(1, 100000)] decimal PricePerNight);

public sealed record UpdateRoomTypeDetailsRequest(
    [Range(1, 100000)] decimal PricePerNight,
    [Range(1, 20)] int Capacity,
    [Url, MaxLength(500)] string? ImageUrl);

public sealed record RoomRequest(
    [Required, MaxLength(20)] string RoomNumber,
    [Range(0, 100)] int Floor,
    Guid RoomTypeId,
    bool IsActive = true);

public sealed record RoomResponse(
    Guid Id, string RoomNumber, int Floor, bool IsActive,
    Guid RoomTypeId, string RoomType, int Capacity, decimal PricePerNight);

public sealed record AmenityRequest(
    [Required, MaxLength(80)] string Name,
    [MaxLength(80)] string? Icon,
    bool IsActive = true);

public sealed record AmenityResponse(Guid Id, string Name, string? Icon, bool IsActive);

public sealed class RoomSearchQuery
{
    [Required] public DateOnly CheckIn { get; init; }
    [Required] public DateOnly CheckOut { get; init; }
    [Range(1, 20)] public int Guests { get; init; } = 1;
    [Range(1, 100000)] public decimal? MaxPricePerNight { get; init; }
    [Range(1, 100)] public int Page { get; init; } = 1;
    [Range(1, 50)] public int PageSize { get; init; } = 10;
}
