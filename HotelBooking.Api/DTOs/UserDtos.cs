using System.ComponentModel.DataAnnotations;
using HotelBooking.Api.Domain;

namespace HotelBooking.Api.DTOs;

public sealed record UserListItemResponse(
    Guid Id,
    string FullName,
    string Email,
    UserRole Role,
    int BookingsCount,
    DateTime CreatedAtUtc);

public sealed record UpdateUserRoleRequest(
    [property: EnumDataType(typeof(UserRole))] UserRole Role);
