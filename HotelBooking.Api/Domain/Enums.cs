namespace HotelBooking.Api.Domain;

public enum UserRole { Customer, Staff, Manager, Admin }
public enum BookingStatus { Pending, Confirmed, Cancelled, Completed }
public enum PaymentMethod { CashAtHotel, BankTransfer, Card }
public enum PaymentStatus { Pending, Paid, Failed, Refunded }
