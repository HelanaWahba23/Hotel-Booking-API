using HotelBooking.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Api.Migrations;

[DbContext(typeof(HotelDbContext))]
[Migration("20260819010000_CommercialFeatures")]
public sealed class CommercialFeatures : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Amenities",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Name = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                Icon = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            }, constraints: table => table.PrimaryKey("PK_Amenities", x => x.Id));

        migrationBuilder.CreateTable(
            name: "HotelSettings",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                HotelName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: false),
                Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                Phone = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Email = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                CheckInTime = table.Column<TimeOnly>(type: "time", nullable: false),
                CheckOutTime = table.Column<TimeOnly>(type: "time", nullable: false),
                FreeCancellationHours = table.Column<int>(type: "int", nullable: false),
                TaxPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_HotelSettings", x => x.Id);
                table.CheckConstraint("CK_HotelSettings_Tax", "[TaxPercentage] >= 0 AND [TaxPercentage] <= 100");
            });

        migrationBuilder.CreateTable(
            name: "PromoCodes",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                DiscountPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                MaxDiscountAmount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                MinimumNights = table.Column<int>(type: "int", nullable: false),
                ValidFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                ValidUntilUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                MaxUses = table.Column<int>(type: "int", nullable: false),
                TimesUsed = table.Column<int>(type: "int", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_PromoCodes", x => x.Id);
                table.CheckConstraint("CK_PromoCodes_Discount", "[DiscountPercentage] > 0 AND [DiscountPercentage] <= 100");
            });

        migrationBuilder.CreateTable(
            name: "RefreshTokens",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TokenHash = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                ExpiresAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                RevokedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                table.ForeignKey("FK_RefreshTokens_Users_UserId", x => x.UserId,
                    "Users", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.AddColumn<DateTime>("CancelledAtUtc", "Bookings", "datetime2", nullable: true);
        migrationBuilder.AddColumn<string>("CancellationReason", "Bookings", "nvarchar(500)", maxLength: 500, nullable: true);
        migrationBuilder.AddColumn<decimal>("DiscountAmount", "Bookings", "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m);
        migrationBuilder.AddColumn<string>("GuestEmail", "Bookings", "nvarchar(180)", maxLength: 180, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<string>("GuestName", "Bookings", "nvarchar(120)", maxLength: 120, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<string>("GuestPhone", "Bookings", "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "");
        migrationBuilder.AddColumn<Guid>("PromoCodeId", "Bookings", "uniqueidentifier", nullable: true);
        migrationBuilder.AddColumn<string>("SpecialRequests", "Bookings", "nvarchar(1000)", maxLength: 1000, nullable: true);
        migrationBuilder.AddColumn<decimal>("Subtotal", "Bookings", "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m);
        migrationBuilder.AddColumn<decimal>("TaxAmount", "Bookings", "decimal(10,2)", precision: 10, scale: 2, nullable: false, defaultValue: 0m);

        migrationBuilder.Sql("UPDATE [Bookings] SET [Subtotal] = [TotalPrice], [GuestName] = 'Legacy Guest', [GuestEmail] = 'legacy@example.com', [GuestPhone] = 'N/A'");

        migrationBuilder.CreateTable(
            name: "RoomTypeAmenities",
            columns: table => new
            {
                RoomTypeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                AmenityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_RoomTypeAmenities", x => new { x.RoomTypeId, x.AmenityId });
                table.ForeignKey("FK_RoomTypeAmenities_Amenities_AmenityId", x => x.AmenityId,
                    "Amenities", "Id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey("FK_RoomTypeAmenities_RoomTypes_RoomTypeId", x => x.RoomTypeId,
                    "RoomTypes", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Payments",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BookingId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Amount = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                Method = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                TransactionReference = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                PaidAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                CreatedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
            }, constraints: table =>
            {
                table.PrimaryKey("PK_Payments", x => x.Id);
                table.ForeignKey("FK_Payments_Bookings_BookingId", x => x.BookingId,
                    "Bookings", "Id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.Sql("INSERT INTO [Payments] ([Id], [BookingId], [Amount], [Method], [Status], [CreatedAtUtc]) SELECT NEWID(), [Id], [TotalPrice], 'CashAtHotel', 'Pending', SYSUTCDATETIME() FROM [Bookings]");

        migrationBuilder.CreateIndex("IX_Amenities_Name", "Amenities", "Name", unique: true);
        migrationBuilder.CreateIndex("IX_Bookings_PromoCodeId", "Bookings", "PromoCodeId");
        migrationBuilder.CreateIndex("IX_Payments_BookingId", "Payments", "BookingId", unique: true);
        migrationBuilder.CreateIndex("IX_PromoCodes_Code", "PromoCodes", "Code", unique: true);
        migrationBuilder.CreateIndex("IX_RefreshTokens_TokenHash", "RefreshTokens", "TokenHash", unique: true);
        migrationBuilder.CreateIndex("IX_RefreshTokens_UserId", "RefreshTokens", "UserId");
        migrationBuilder.CreateIndex("IX_RoomTypeAmenities_AmenityId", "RoomTypeAmenities", "AmenityId");

        migrationBuilder.AddForeignKey("FK_Bookings_PromoCodes_PromoCodeId", "Bookings", "PromoCodeId",
            "PromoCodes", principalColumn: "Id", onDelete: ReferentialAction.SetNull);
        migrationBuilder.AddCheckConstraint("CK_Bookings_Dates", "Bookings", "[CheckOutDate] > [CheckInDate]");
        migrationBuilder.AddCheckConstraint("CK_Bookings_Guests", "Bookings", "[Guests] > 0");
        migrationBuilder.AddCheckConstraint("CK_Bookings_Amounts", "Bookings", "[Subtotal] >= 0 AND [DiscountAmount] >= 0 AND [TaxAmount] >= 0 AND [TotalPrice] >= 0");
        migrationBuilder.AddCheckConstraint("CK_Reviews_Rating", "Reviews", "[Rating] BETWEEN 1 AND 5");
        migrationBuilder.AddCheckConstraint("CK_RoomTypes_Capacity", "RoomTypes", "[Capacity] > 0");
        migrationBuilder.AddCheckConstraint("CK_RoomTypes_Price", "RoomTypes", "[PricePerNight] > 0");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey("FK_Bookings_PromoCodes_PromoCodeId", "Bookings");
        migrationBuilder.DropCheckConstraint("CK_Bookings_Dates", "Bookings");
        migrationBuilder.DropCheckConstraint("CK_Bookings_Guests", "Bookings");
        migrationBuilder.DropCheckConstraint("CK_Bookings_Amounts", "Bookings");
        migrationBuilder.DropCheckConstraint("CK_Reviews_Rating", "Reviews");
        migrationBuilder.DropCheckConstraint("CK_RoomTypes_Capacity", "RoomTypes");
        migrationBuilder.DropCheckConstraint("CK_RoomTypes_Price", "RoomTypes");
        migrationBuilder.DropTable("HotelSettings");
        migrationBuilder.DropTable("Payments");
        migrationBuilder.DropTable("RefreshTokens");
        migrationBuilder.DropTable("RoomTypeAmenities");
        migrationBuilder.DropTable("Amenities");
        migrationBuilder.DropTable("PromoCodes");
        migrationBuilder.DropColumn("CancelledAtUtc", "Bookings");
        migrationBuilder.DropColumn("CancellationReason", "Bookings");
        migrationBuilder.DropColumn("DiscountAmount", "Bookings");
        migrationBuilder.DropColumn("GuestEmail", "Bookings");
        migrationBuilder.DropColumn("GuestName", "Bookings");
        migrationBuilder.DropColumn("GuestPhone", "Bookings");
        migrationBuilder.DropColumn("PromoCodeId", "Bookings");
        migrationBuilder.DropColumn("SpecialRequests", "Bookings");
        migrationBuilder.DropColumn("Subtotal", "Bookings");
        migrationBuilder.DropColumn("TaxAmount", "Bookings");
    }
}
