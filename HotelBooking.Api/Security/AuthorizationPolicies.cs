using Microsoft.AspNetCore.Authorization;

namespace HotelBooking.Api.Security;

public static class HotelRoles
{
    public const string Customer = nameof(Domain.UserRole.Customer);
    public const string Staff = nameof(Domain.UserRole.Staff);
    public const string Manager = nameof(Domain.UserRole.Manager);
    public const string Admin = nameof(Domain.UserRole.Admin);

    public const string FrontDeskRoles = $"{Staff},{Manager},{Admin}";
    public const string ManagementRoles = $"{Manager},{Admin}";
}

public static class AuthorizationPolicies
{
    public const string CustomerOnly = nameof(CustomerOnly);
    public const string CanCreateBooking = nameof(CanCreateBooking);
    public const string CanManageBooking = nameof(CanManageBooking);
    public const string FrontDesk = nameof(FrontDesk);
    public const string Management = nameof(Management);
    public const string AdminOnly = nameof(AdminOnly);

    public static void Configure(AuthorizationOptions options)
    {
        options.AddPolicy(CustomerOnly,
            policy => policy.RequireRole(HotelRoles.Customer));
        options.AddPolicy(CanCreateBooking,
            policy => policy.RequireRole(HotelRoles.Customer, HotelRoles.Staff));
        options.AddPolicy(CanManageBooking,
            policy => policy.RequireRole(
                HotelRoles.Customer, HotelRoles.Staff, HotelRoles.Manager, HotelRoles.Admin));
        options.AddPolicy(FrontDesk,
            policy => policy.RequireRole(HotelRoles.Staff, HotelRoles.Manager, HotelRoles.Admin));
        options.AddPolicy(Management,
            policy => policy.RequireRole(HotelRoles.Manager, HotelRoles.Admin));
        options.AddPolicy(AdminOnly,
            policy => policy.RequireRole(HotelRoles.Admin));
    }
}
