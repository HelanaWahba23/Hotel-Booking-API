using HotelBooking.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelBooking.Api.Migrations;

[DbContext(typeof(HotelDbContext))]
[Migration("20260901000000_ManagerSiteCustomization")]
public sealed class ManagerSiteCustomization : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "PrimaryColor",
            table: "HotelSettings",
            type: "nvarchar(7)",
            maxLength: 7,
            nullable: false,
            defaultValue: "#102c27");

        migrationBuilder.AddColumn<string>(
            name: "AccentColor",
            table: "HotelSettings",
            type: "nvarchar(7)",
            maxLength: 7,
            nullable: false,
            defaultValue: "#c7a05b");

        migrationBuilder.AddColumn<string>(
            name: "HomeHeroImageUrl",
            table: "HotelSettings",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "https://images.unsplash.com/photo-1564501049412-61c2a3083791?auto=format&fit=crop&w=2000&q=88");

        migrationBuilder.AddColumn<string>(
            name: "RoomsHeroImageUrl",
            table: "HotelSettings",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=2000&q=85");

        migrationBuilder.AddColumn<string>(
            name: "AccountHeroImageUrl",
            table: "HotelSettings",
            type: "nvarchar(500)",
            maxLength: 500,
            nullable: false,
            defaultValue: "https://images.unsplash.com/photo-1540541338287-41700207dee6?auto=format&fit=crop&w=1300&q=85");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "PrimaryColor", table: "HotelSettings");
        migrationBuilder.DropColumn(name: "AccentColor", table: "HotelSettings");
        migrationBuilder.DropColumn(name: "HomeHeroImageUrl", table: "HotelSettings");
        migrationBuilder.DropColumn(name: "RoomsHeroImageUrl", table: "HotelSettings");
        migrationBuilder.DropColumn(name: "AccountHeroImageUrl", table: "HotelSettings");
    }
}
