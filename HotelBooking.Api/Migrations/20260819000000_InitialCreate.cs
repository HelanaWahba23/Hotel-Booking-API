using HotelBooking.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Api.Migrations;

[DbContext(typeof(HotelDbContext))]
[Migration("20260819000000_InitialCreate")]
public sealed class InitialCreate : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "RoomTypes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Description = table.Column<string>(type: "nvarchar(700)", maxLength: 700, nullable: false),
                Capacity = table.Column<int>(type: "int", nullable: false),
                PricePerNight = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                ImageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_RoomTypes", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                FullName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                Email = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: false),
                Role = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table => table.PrimaryKey("PK_Users", x => x.Id));

        migrationBuilder.CreateTable(
            name: "Rooms",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RoomNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                Floor = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                RoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Rooms", x => x.Id);
                table.ForeignKey("FK_Rooms_RoomTypes_RoomTypeId", x => x.RoomTypeId,
                    "RoomTypes", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Bookings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                ReferenceNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                CheckInDate = table.Column<DateOnly>(type: "date", nullable: false),
                CheckOutDate = table.Column<DateOnly>(type: "date", nullable: false),
                Guests = table.Column<int>(type: "int", nullable: false),
                TotalPrice = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Bookings", x => x.Id);
                table.ForeignKey("FK_Bookings_Rooms_RoomId", x => x.RoomId,
                    "Rooms", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Bookings_Users_UserId", x => x.UserId,
                    "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Reviews",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Rating = table.Column<int>(type: "int", nullable: false),
                Comment = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Reviews", x => x.Id);
                table.ForeignKey("FK_Reviews_Bookings_BookingId", x => x.BookingId,
                    "Bookings", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_Reviews_Users_UserId", x => x.UserId,
                    "Users", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_Bookings_ReferenceNumber", "Bookings", "ReferenceNumber", unique: true);
        migrationBuilder.CreateIndex("IX_Bookings_RoomId", "Bookings", "RoomId");
        migrationBuilder.CreateIndex("IX_Bookings_UserId", "Bookings", "UserId");
        migrationBuilder.CreateIndex("IX_Reviews_BookingId", "Reviews", "BookingId", unique: true);
        migrationBuilder.CreateIndex("IX_Reviews_UserId", "Reviews", "UserId");
        migrationBuilder.CreateIndex("IX_Rooms_RoomNumber", "Rooms", "RoomNumber", unique: true);
        migrationBuilder.CreateIndex("IX_Rooms_RoomTypeId", "Rooms", "RoomTypeId");
        migrationBuilder.CreateIndex("IX_RoomTypes_Name", "RoomTypes", "Name", unique: true);
        migrationBuilder.CreateIndex("IX_Users_Email", "Users", "Email", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable("Reviews");
        migrationBuilder.DropTable("Bookings");
        migrationBuilder.DropTable("Rooms");
        migrationBuilder.DropTable("Users");
        migrationBuilder.DropTable("RoomTypes");
    }
}
