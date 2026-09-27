using System.ComponentModel.DataAnnotations;

namespace HotelBooking.Api.DTOs;

public sealed record RegisterRequest(
    [Required, MinLength(3), MaxLength(120)] string FullName,
    [Required, EmailAddress, MaxLength(180)] string Email,
    [Required, MinLength(8), MaxLength(100)] string Password);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public sealed record AuthResponse(
    string AccessToken, DateTime ExpiresAtUtc, Guid UserId,
    string FullName, string Email, string Role);
