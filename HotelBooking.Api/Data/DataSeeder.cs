using HotelBooking.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Data;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        await db.Database.MigrateAsync();

        if (!await db.Users.AnyAsync(x => x.Role == UserRole.Admin))
        {
            var adminEmail = configuration["SeedAdmin:Email"]
                ?? throw new InvalidOperationException("SeedAdmin:Email is required for first startup.");
            var adminPassword = configuration["SeedAdmin:Password"]
                ?? throw new InvalidOperationException("SeedAdmin:Password is required for first startup.");
            var admin = new User
            {
                FullName = configuration["SeedAdmin:FullName"] ?? "Hotel Administrator",
                Email = adminEmail.Trim().ToLowerInvariant(),
                Role = UserRole.Admin
            };
            admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, adminPassword);
            db.Users.Add(admin);
        }

        await EnsureRoleUserAsync(db, configuration, "SeedStaff", UserRole.Staff);
        await EnsureRoleUserAsync(db, configuration, "SeedManager", UserRole.Manager);

        if (!await db.RoomTypes.AnyAsync())
        {
            var standard = new RoomType
            {
                Name = "غرفة كلاسيك",
                Description = "غرفة هادئة بسرير مزدوج وتجهيزات متكاملة لإقامة مريحة.",
                Capacity = 2,
                PricePerNight = 1200
            };
            var deluxe = new RoomType
            {
                Name = "غرفة ديلوكس",
                Description = "غرفة واسعة بسرير كبير وإطلالة مميزة على المدينة.",
                Capacity = 3,
                PricePerNight = 2100
            };
            var family = new RoomType
            {
                Name = "جناح عائلي",
                Description = "جناح من غرفتين يوفر الخصوصية والراحة المناسبة للعائلات.",
                Capacity = 5,
                PricePerNight = 3500
            };
            db.RoomTypes.AddRange(standard, deluxe, family);
            db.Rooms.AddRange(
                new Room { RoomNumber = "101", Floor = 1, RoomType = standard },
                new Room { RoomNumber = "102", Floor = 1, RoomType = standard },
                new Room { RoomNumber = "201", Floor = 2, RoomType = deluxe },
                new Room { RoomNumber = "202", Floor = 2, RoomType = deluxe },
                new Room { RoomNumber = "301", Floor = 3, RoomType = family });
        }

        await db.SaveChangesAsync();

        if (!await db.HotelSettings.AnyAsync())
        {
            db.HotelSettings.Add(new HotelSettings
            {
                HotelName = "فندق نيلورا",
                Description = "تجربة إقامة مصرية معاصرة تجمع بين الهدوء والضيافة الراقية في قلب القاهرة.",
                Address = "Cairo, Egypt",
                Phone = "+20 100 000 0000",
                Email = "info@nileviewhotel.com",
                Currency = "EGP",
                CheckInTime = new TimeOnly(14, 0),
                CheckOutTime = new TimeOnly(12, 0),
                FreeCancellationHours = 48,
                TaxPercentage = 14,
                PrimaryColor = "#102c27",
                AccentColor = "#c7a05b",
                HomeHeroImageUrl = "https://images.unsplash.com/photo-1564501049412-61c2a3083791?auto=format&fit=crop&w=2000&q=88",
                RoomsHeroImageUrl = "https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=2000&q=85",
                AccountHeroImageUrl = "https://images.unsplash.com/photo-1540541338287-41700207dee6?auto=format&fit=crop&w=1300&q=85"
            });
        }

        if (!await db.Amenities.AnyAsync())
        {
            var wifi = new Amenity { Name = "واي فاي مجاني", Icon = "wifi" };
            var airConditioning = new Amenity { Name = "تكييف", Icon = "snowflake" };
            var breakfast = new Amenity { Name = "إفطار", Icon = "coffee" };
            var cityView = new Amenity { Name = "إطلالة على المدينة", Icon = "building" };
            db.Amenities.AddRange(wifi, airConditioning, breakfast, cityView);

            var types = await db.RoomTypes.ToListAsync();
            foreach (var type in types)
            {
                type.RoomTypeAmenities.Add(new RoomTypeAmenity { Amenity = wifi });
                type.RoomTypeAmenities.Add(new RoomTypeAmenity { Amenity = airConditioning });
                if (type.Name != "غرفة كلاسيك")
                    type.RoomTypeAmenities.Add(new RoomTypeAmenity { Amenity = breakfast });
                if (type.Name == "غرفة ديلوكس")
                    type.RoomTypeAmenities.Add(new RoomTypeAmenity { Amenity = cityView });
            }
        }

        if (!await db.PromoCodes.AnyAsync())
        {
            db.PromoCodes.Add(new PromoCode
            {
                Code = "WELCOME10",
                DiscountPercentage = 10,
                MaxDiscountAmount = 500,
                MinimumNights = 2,
                ValidFromUtc = DateTime.UtcNow.AddDays(-1),
                ValidUntilUtc = DateTime.UtcNow.AddYears(1),
                MaxUses = 1000
            });
        }

        await LocalizeDemoDataAsync(db);

        await db.SaveChangesAsync();
    }

    private static async Task EnsureRoleUserAsync(
        HotelDbContext db, IConfiguration configuration, string section, UserRole role)
    {
        if (await db.Users.AnyAsync(x => x.Role == role))
            return;

        var email = configuration[$"{section}:Email"];
        var password = configuration[$"{section}:Password"];
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return;

        var user = new User
        {
            FullName = configuration[$"{section}:FullName"] ?? role.ToString(),
            Email = email.Trim().ToLowerInvariant(),
            Role = role
        };
        user.PasswordHash = new PasswordHasher<User>().HashPassword(user, password);
        db.Users.Add(user);
    }

    private static async Task LocalizeDemoDataAsync(HotelDbContext db)
    {
        var roomTypes = await db.RoomTypes.ToListAsync();
        foreach (var roomType in roomTypes)
        {
            (roomType.Name, roomType.Description) = roomType.Name switch
            {
                "Standard" => ("غرفة كلاسيك", "غرفة هادئة بسرير مزدوج وتجهيزات متكاملة لإقامة مريحة."),
                "Deluxe" => ("غرفة ديلوكس", "غرفة واسعة بسرير كبير وإطلالة مميزة على المدينة."),
                "Family Suite" => ("جناح عائلي", "جناح من غرفتين يوفر الخصوصية والراحة المناسبة للعائلات."),
                _ => (roomType.Name, roomType.Description)
            };
        }

        var amenityTranslations = new Dictionary<string, string>
        {
            ["Free Wi-Fi"] = "واي فاي مجاني",
            ["Air Conditioning"] = "تكييف",
            ["Breakfast"] = "إفطار",
            ["City View"] = "إطلالة على المدينة"
        };
        var amenities = await db.Amenities.ToListAsync();
        foreach (var amenity in amenities)
        {
            if (amenityTranslations.TryGetValue(amenity.Name, out var translatedName))
                amenity.Name = translatedName;
        }

        var settings = await db.HotelSettings.SingleOrDefaultAsync();
        if (settings?.HotelName == "Nile View Hotel")
        {
            settings.HotelName = "فندق نيلورا";
            settings.Description = "تجربة إقامة مصرية معاصرة تجمع بين الهدوء والضيافة الراقية في قلب القاهرة.";
        }
    }
}
