# المهارات الظاهرة في المشروع

هذا المشروع مناسب لعرض مستوى Junior .NET Developer؛ لأنه يطبق الأساسيات بشكل مباشر وقابل للشرح في مقابلة العمل.

## 1. Database وSQL Server

- قاعدة البيانات تحتوي جداول للمستخدمين وأنواع الغرف والغرف والحجوزات والمدفوعات والتقييمات والمرافق والخصومات.
- تم استخدام Primary Keys وForeign Keys وUnique Indexes وCheck Constraints.
- إعدادات الجداول والعلاقات موجودة في `Data/HotelDbContext.cs`.

## 2. Entity Framework Core

- كل جدول له Entity داخل `Domain/Entities.cs`.
- كل Entity متاحة من خلال `DbSet` داخل `HotelDbContext`.
- الاستعلامات تستخدم LINQ و`Include` و`AsNoTracking` وPagination.
- توجد علاقات One-to-Many وOne-to-One وMany-to-Many.

## 3. Code First

- تم إنشاء التصميم من C# Entities.
- مجلد `Migrations` يحتوي تاريخ إنشاء وتطوير قاعدة البيانات.
- عند تشغيل المشروع يتم تطبيق الـMigrations تلقائيًا بواسطة `Database.MigrateAsync()`.

أوامر مهمة يمكن شرحها أو استخدامها:

```powershell
dotnet ef migrations add MigrationName --project HotelBooking.Api
dotnet ef database update --project HotelBooking.Api
dotnet ef migrations list --project HotelBooking.Api
```

## 4. REST API

- Controllers داخل مجلد `Controllers`.
- DTOs منفصلة عن Entities داخل مجلد `DTOs`.
- استخدام GET وPOST وPUT وPATCH وDELETE.
- Validation عن طريق Data Annotations.
- تسجيل الدخول يرجع Access Token بسيط، وAuthorization باستخدام Roles.
- Swagger لتجربة وتوثيق الـEndpoints.

## علاقات سهلة للشرح

- المستخدم الواحد يمكن أن يملك عدة حجوزات.
- نوع الغرفة يحتوي عدة غرف.
- الغرفة يمكن أن تدخل في عدة حجوزات بأوقات مختلفة.
- كل حجز له عملية دفع واحدة.
- نوع الغرفة والمرافق بينهما Many-to-Many باستخدام `RoomTypeAmenity`.

## تجربة وعرض المشروع

شغّل المشروع ثم افتح `/swagger`. ابدأ بـ`POST /api/auth/login`، وانسخ الـAccess Token إلى زر **Authorize**، وبعدها جرّب Endpoints الغرف والحجوزات والإدارة.
