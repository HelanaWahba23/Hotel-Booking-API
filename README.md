# Nileora Hotel Booking REST API

مشروع **ASP.NET Core 8 Web API فقط** لإدارة حجز فندق أونلاين. لا يحتوي على MVC أو Razor Views أو Frontend أو Node.js.

المشروع مناسب لمستوى Junior .NET ويوضح المهارات الأساسية: REST API، Controllers، DTOs، Validation، EF Core Code First، SQL Server، LINQ، العلاقات، Migrations، وتسجيل الدخول بتوكن بسيط.

## الوظائف

- إنشاء حساب عميل وتسجيل الدخول.
- البحث عن الغرف المتاحة حسب التاريخ وعدد النزلاء والسعر.
- إنشاء الحجز ومنع تداخل الحجوزات على نفس الغرفة.
- عرض حجوزات العميل وإلغاؤها قبل التأكيد.
- إدارة حالة الحجز والدفع بواسطة موظف الاستقبال.
- إدارة الغرف وأنواعها والأسعار والسعة بواسطة المدير.
- إدارة المرافق وكوبونات الخصم وبيانات الفندق.
- كتابة التقييمات بعد الإقامة والتحكم في ظهورها.
- Dashboard API لإحصائيات الغرف والحجوزات والإيرادات.
- Pagination ومعالجة أخطاء موحدة وRate Limiting وCORS.

## التقنيات

- ASP.NET Core 8 Web API
- Entity Framework Core 8
- SQL Server / LocalDB
- Code First Migrations
- Swagger / OpenAPI
- Bearer Access Token

## تنظيم المشروع

```text
Controllers/   REST API endpoints
DTOs/          Request and response models
Domain/        Database entities and enums
Data/          DbContext, configuration and seeding
Services/      Booking business logic
Security/      Token creation and authorization policies
Middleware/    Error handling and security headers
Migrations/    Code First database history
```

## التشغيل

المتطلبات: .NET 8 SDK وSQL Server LocalDB.

```powershell
dotnet restore
dotnet run --project HotelBooking.Api
```

- معلومات الـAPI بصيغة JSON: `http://localhost:5131/`
- Swagger: `http://localhost:5131/swagger`
- Health Check: `http://localhost:5131/api/health`

## تجربة تسجيل الدخول

نفّذ `POST /api/auth/login` من Swagger. انسخ قيمة `accessToken`، ثم اضغط **Authorize** واكتب:

```text
Bearer access-token-here
```

حساب المدير التجريبي موجود في `appsettings.Development.json`. يجب تغيير بياناته ومفتاح التوكن قبل النشر.

## أهم Endpoints

| Method | Endpoint | الاستخدام |
|---|---|---|
| POST | `/api/auth/register` | إنشاء عميل |
| POST | `/api/auth/login` | تسجيل الدخول واستلام Access Token |
| GET | `/api/hotel` | معلومات الفندق |
| GET | `/api/rooms/search` | البحث عن غرفة متاحة |
| POST | `/api/bookings` | إنشاء حجز |
| GET | `/api/bookings/mine` | حجوزات المستخدم الحالي |
| PATCH | `/api/bookings/{id}/cancel` | إلغاء حجز |
| GET | `/api/bookings` | عرض الحجوزات للتشغيل |
| PATCH | `/api/bookings/{id}/status` | تحديث حالة الحجز |
| PATCH | `/api/bookings/{id}/payment` | تحديث الدفع |
| POST | `/api/rooms` | إضافة غرفة |
| PATCH | `/api/room-types/{id}/price` | تعديل السعر |
| GET | `/api/dashboard` | إحصائيات الإدارة |
| GET | `/api/users` | المستخدمون المسجلون |
| POST | `/api/reviews` | كتابة تقييم |
| GET | `/api/health` | حالة الخدمة |

طلبات جاهزة للتجربة موجودة في `HotelBooking.Api/HotelBooking.Api.http`.

## وصف مناسب للـCV

> Online Hotel Booking REST API built with ASP.NET Core 8, Entity Framework Core Code First, SQL Server, DTO Validation, LINQ, Swagger and role-based authorization.
