using HotelBooking.Api.Data;
using HotelBooking.Api.DTOs;
using HotelBooking.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Controllers;

[ApiController]
[Route("api/project")]
[Authorize(Policy = AuthorizationPolicies.Management)]
public sealed class ProjectController(HotelDbContext db) : ControllerBase
{
    [HttpGet("database-summary")]
    [ProducesResponseType<DatabaseSummaryResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<DatabaseSummaryResponse>> GetDatabaseSummary(
        CancellationToken cancellationToken)
    {
        var migrations = await db.Database.GetAppliedMigrationsAsync(cancellationToken);
        var response = new DatabaseSummaryResponse(
            db.Database.ProviderName ?? "Unknown",
            db.Model.GetEntityTypes().Count(),
            await db.Users.CountAsync(cancellationToken),
            await db.RoomTypes.CountAsync(cancellationToken),
            await db.Rooms.CountAsync(cancellationToken),
            await db.Bookings.CountAsync(cancellationToken),
            await db.Amenities.CountAsync(cancellationToken),
            migrations.Count());

        return Ok(response);
    }
}
