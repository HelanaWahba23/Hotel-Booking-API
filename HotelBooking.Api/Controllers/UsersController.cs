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
[Route("api/users")]
[Authorize(Policy = AuthorizationPolicies.Management)]
public sealed class UsersController(HotelDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PagedResult<UserListItemResponse>>> GetAll(
        [FromQuery] UserRole? role = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var users = db.Users.AsNoTracking().AsQueryable();

        if (role.HasValue)
            users = users.Where(x => x.Role == role.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            users = users.Where(x => x.FullName.Contains(term) || x.Email.Contains(term));
        }

        var total = await users.CountAsync(cancellationToken);
        var items = await users.OrderByDescending(x => x.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new UserListItemResponse(
                x.Id, x.FullName, x.Email, x.Role, x.Bookings.Count, x.CreatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(new PagedResult<UserListItemResponse>(items, page, pageSize, total));
    }

    [HttpPatch("{id:guid}/role")]
    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    public async Task<IActionResult> UpdateRole(
        Guid id, UpdateUserRoleRequest request, CancellationToken cancellationToken)
    {
        var user = await db.Users.FindAsync([id], cancellationToken)
            ?? throw new ApiException(404, "User was not found.");

        if (user.Id == User.GetUserId() && request.Role != UserRole.Admin)
            throw new ApiException(409, "You cannot remove your own administrator role.");

        if (user.Role == UserRole.Admin && request.Role != UserRole.Admin &&
            await db.Users.CountAsync(x => x.Role == UserRole.Admin, cancellationToken) == 1)
            throw new ApiException(409, "The system must keep at least one administrator.");

        user.Role = request.Role;
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
