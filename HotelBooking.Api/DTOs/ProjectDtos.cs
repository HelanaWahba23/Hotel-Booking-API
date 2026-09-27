namespace HotelBooking.Api.DTOs;

public sealed record DatabaseSummaryResponse(
    string Provider,
    int Entities,
    int Users,
    int RoomTypes,
    int Rooms,
    int Bookings,
    int Amenities,
    int AppliedMigrations);
