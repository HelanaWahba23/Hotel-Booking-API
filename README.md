# Nileora Hotel Booking API

A RESTful API for managing hotel rooms, reservations, payments, customers, and reviews. Built with ASP.NET Core 8 and Entity Framework Core using a Code First approach.

## Features

- Customer registration and login
- Room availability search by date, capacity, and price
- Booking creation with date-conflict validation
- Booking status, cancellation, and payment management
- Room types, amenities, prices, and promo codes
- Customer reviews with manager approval
- Role-based access for customers, staff, managers, and admins
- Pagination, request validation, and centralized error handling
- Swagger/OpenAPI documentation

## Tech Stack

- C# and ASP.NET Core 8 Web API
- Entity Framework Core 8
- SQL Server / LocalDB
- Code First Migrations
- LINQ
- Swagger / OpenAPI
- Bearer token authentication

## Project Structure

```text
HotelBooking.Api/
├── Controllers/   API endpoints
├── DTOs/          Request and response models
├── Domain/        Entities and enums
├── Data/          DbContext and database seeding
├── Services/      Booking business logic
├── Security/      Authentication and authorization
├── Middleware/    Error handling and security headers
└── Migrations/    Code First migrations
```

## Getting Started

### Requirements

- .NET 8 SDK
- SQL Server LocalDB or SQL Server

### Setup

1. Create `HotelBooking.Api/appsettings.Development.json` from `HotelBooking.Api/appsettings.Development.example.json`.
2. Replace the example connection string, token key, and seed account values.
3. Restore and run the project:

```powershell
dotnet restore
dotnet run --project HotelBooking.Api
```

The database is created and updated automatically from the included migrations.

## API Documentation

After running the project, open:

- Swagger UI: `http://localhost:5131/swagger`
- Health check: `http://localhost:5131/api/health`

Use `POST /api/auth/login` to receive an access token. In Swagger, select **Authorize** and enter:

```text
Bearer your-access-token
```

## Main Endpoints

| Method | Endpoint | Description |
|---|---|---|
| `POST` | `/api/auth/register` | Register a customer |
| `POST` | `/api/auth/login` | Login and receive an access token |
| `GET` | `/api/rooms/search` | Search available rooms |
| `POST` | `/api/bookings` | Create a booking |
| `GET` | `/api/bookings/mine` | Get the current user's bookings |
| `PATCH` | `/api/bookings/{id}/cancel` | Cancel a booking |
| `PATCH` | `/api/bookings/{id}/status` | Update booking status |
| `PATCH` | `/api/bookings/{id}/payment` | Update payment status |
| `POST` | `/api/rooms` | Add a room |
| `PATCH` | `/api/room-types/{id}/price` | Update a room price |
| `POST` | `/api/reviews` | Add a customer review |
| `GET` | `/api/dashboard` | Get management statistics |

Additional request examples are available in `HotelBooking.Api/HotelBooking.Api.http`.

## Database

The database includes users, room types, rooms, bookings, payments, reviews, amenities, promo codes, and hotel settings. Relationships, indexes, constraints, and column configuration are defined in `HotelDbContext`.
