using System.Security.Claims;
using HotelBooking.Api.Common;

namespace HotelBooking.Api.Security;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id)
            ? id
            : throw new ApiException(StatusCodes.Status401Unauthorized, "Invalid access token.");
    }

    public static bool IsHotelEmployee(this ClaimsPrincipal principal) =>
        principal.IsInRole(HotelRoles.Staff) ||
        principal.IsInRole(HotelRoles.Manager) ||
        principal.IsInRole(HotelRoles.Admin);
}
