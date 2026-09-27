using HotelBooking.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace HotelBooking.Api.Data;

public sealed class HotelDbContext(DbContextOptions<HotelDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RoomType> RoomTypes => Set<RoomType>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Amenity> Amenities => Set<Amenity>();
    public DbSet<RoomTypeAmenity> RoomTypeAmenities => Set<RoomTypeAmenity>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<HotelSettings> HotelSettings => Set<HotelSettings>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.FullName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(180).IsRequired();
            entity.Property(x => x.PasswordHash).IsRequired();
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        });

        modelBuilder.Entity<RoomType>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(700);
            entity.Property(x => x.PricePerNight).HasPrecision(10, 2);
            entity.Property(x => x.ImageUrl).HasMaxLength(500);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_RoomTypes_Capacity", "[Capacity] > 0");
                t.HasCheckConstraint("CK_RoomTypes_Price", "[PricePerNight] > 0");
            });
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasIndex(x => x.RoomNumber).IsUnique();
            entity.Property(x => x.RoomNumber).HasMaxLength(20).IsRequired();
            entity.HasOne(x => x.RoomType).WithMany(x => x.Rooms)
                .HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasIndex(x => x.ReferenceNumber).IsUnique();
            entity.Property(x => x.ReferenceNumber).HasMaxLength(20).IsRequired();
            entity.Property(x => x.TotalPrice).HasPrecision(10, 2);
            entity.Property(x => x.Subtotal).HasPrecision(10, 2);
            entity.Property(x => x.DiscountAmount).HasPrecision(10, 2);
            entity.Property(x => x.TaxAmount).HasPrecision(10, 2);
            entity.Property(x => x.GuestName).HasMaxLength(120).IsRequired();
            entity.Property(x => x.GuestEmail).HasMaxLength(180).IsRequired();
            entity.Property(x => x.GuestPhone).HasMaxLength(30).IsRequired();
            entity.Property(x => x.SpecialRequests).HasMaxLength(1000);
            entity.Property(x => x.CancellationReason).HasMaxLength(500);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.HasOne(x => x.User).WithMany(x => x.Bookings)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.Room).WithMany(x => x.Bookings)
                .HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(x => x.PromoCode).WithMany(x => x.Bookings)
                .HasForeignKey(x => x.PromoCodeId).OnDelete(DeleteBehavior.SetNull);
            entity.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Bookings_Dates", "[CheckOutDate] > [CheckInDate]");
                t.HasCheckConstraint("CK_Bookings_Guests", "[Guests] > 0");
                t.HasCheckConstraint("CK_Bookings_Amounts", "[Subtotal] >= 0 AND [DiscountAmount] >= 0 AND [TaxAmount] >= 0 AND [TotalPrice] >= 0");
            });
        });

        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasIndex(x => x.BookingId).IsUnique();
            entity.Property(x => x.Comment).HasMaxLength(1000);
            entity.HasOne(x => x.Booking).WithOne(x => x.Review)
                .HasForeignKey<Review>(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.User).WithMany(x => x.Reviews)
                .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(t => t.HasCheckConstraint("CK_Reviews_Rating", "[Rating] BETWEEN 1 AND 5"));
        });

        modelBuilder.Entity<Amenity>(entity =>
        {
            entity.HasIndex(x => x.Name).IsUnique();
            entity.Property(x => x.Name).HasMaxLength(80).IsRequired();
            entity.Property(x => x.Icon).HasMaxLength(80);
        });

        modelBuilder.Entity<RoomTypeAmenity>(entity =>
        {
            entity.HasKey(x => new { x.RoomTypeId, x.AmenityId });
            entity.HasOne(x => x.RoomType).WithMany(x => x.RoomTypeAmenities)
                .HasForeignKey(x => x.RoomTypeId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Amenity).WithMany(x => x.RoomTypeAmenities)
                .HasForeignKey(x => x.AmenityId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PromoCode>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
            entity.Property(x => x.DiscountPercentage).HasPrecision(5, 2);
            entity.Property(x => x.MaxDiscountAmount).HasPrecision(10, 2);
            entity.ToTable(t => t.HasCheckConstraint("CK_PromoCodes_Discount", "[DiscountPercentage] > 0 AND [DiscountPercentage] <= 100"));
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasIndex(x => x.BookingId).IsUnique();
            entity.Property(x => x.Amount).HasPrecision(10, 2);
            entity.Property(x => x.Method).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.TransactionReference).HasMaxLength(150);
            entity.HasOne(x => x.Booking).WithOne(x => x.Payment)
                .HasForeignKey<Payment>(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HotelSettings>(entity =>
        {
            entity.Property(x => x.HotelName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1500);
            entity.Property(x => x.Address).HasMaxLength(300);
            entity.Property(x => x.Phone).HasMaxLength(30);
            entity.Property(x => x.Email).HasMaxLength(180);
            entity.Property(x => x.Currency).HasMaxLength(3);
            entity.Property(x => x.TaxPercentage).HasPrecision(5, 2);
            entity.Property(x => x.PrimaryColor).HasMaxLength(7);
            entity.Property(x => x.AccentColor).HasMaxLength(7);
            entity.Property(x => x.HomeHeroImageUrl).HasMaxLength(500);
            entity.Property(x => x.RoomsHeroImageUrl).HasMaxLength(500);
            entity.Property(x => x.AccountHeroImageUrl).HasMaxLength(500);
            entity.ToTable(t => t.HasCheckConstraint("CK_HotelSettings_Tax", "[TaxPercentage] >= 0 AND [TaxPercentage] <= 100"));
        });
    }
}
