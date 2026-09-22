# Inventory Management System

نظام ويب لإدارة المخزون مبني باستخدام **ASP.NET Core Web API** و **Angular**. يوفّر النظام إدارة المنتجات والتصنيفات والمخازن وحركات المخزون، مع متابعة الأصناف منخفضة المخزون ولوحة معلومات مختصرة.

> **Project summary:** A full-stack inventory management system with a .NET 8 Web API, Angular 18 frontend, JWT authentication, role-based authorization, SQL Server persistence, and seeded sample data.

## Features

- تسجيل الدخول وإنشاء الحسابات باستخدام JWT.
- صلاحيات مبنية على الأدوار: `Admin` و`Manager` و`Cashier`.
- إضافة واستعراض المنتجات مع بيانات SKU والأسعار والحد الأدنى للمخزون.
- إدارة التصنيفات والمخازن.
- تسجيل حركات المخزون، بما في ذلك الإضافة والصرف والتعديل.
- عرض الأصناف منخفضة المخزون.
- لوحة معلومات تعرض إجمالي المنتجات وقيمة المخزون والتنبيهات.
- Swagger لتجربة وتوثيق واجهات الـ API.
- حفظ البيانات باستخدام Entity Framework Core وSQL Server.
- تسجيل الطلبات والأخطاء باستخدام Serilog وmiddleware مركزي للاستثناءات.
- بيانات تجريبية تلقائية للتطوير المحلي.

## Technology Stack

### Backend

- .NET 8 / ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server / LocalDB
- ASP.NET Core Identity
- JWT Bearer Authentication
- MediatR وCQRS للعمليات الأساسية
- FluentValidation
- Serilog
- Swagger / OpenAPI

### Frontend

- Angular 18
- TypeScript 5.5
- RxJS
- Reactive Forms
- SCSS

## Architecture

المشروع مقسّم إلى طبقات لتسهيل التطوير والصيانة:

```text
src/
├── InventorySystem.API            # Controllers, middleware, API startup
├── InventorySystem.Application    # Commands, queries, DTOs, interfaces
├── InventorySystem.Domain         # Entities and domain models
└── InventorySystem.Infrastructure  # EF Core, Identity, JWT, persistence

frontend/
└── src/app                        # Angular pages, routes, and API service
```

## Requirements

- .NET SDK 8
- Node.js 18 أو أحدث
- npm
- SQL Server LocalDB أو SQL Server
- Visual Studio 2022 أو VS Code (اختياري)

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/RamezSameh/InventoryManagementSystem.git
cd InventoryManagementSystem
```

### 2. Configure the backend

راجع الملف:

```text
src/InventorySystem.API/appsettings.json
```

حدّث `ConnectionStrings:DefaultConnection` إذا كنت تستخدم SQL Server مختلفًا.

> **مفتاح JWT:** التطبيق يرفض التشغيل خارج بيئة التطوير إذا لم يتم ضبط `Jwt:Key` بقيمة حقيقية. اضبطه عبر متغير البيئة `Jwt__Key` (أو User Secrets) بقيمة عشوائية لا تقل عن 32 حرفًا. في بيئة التطوير يعمل النظام بمفتاح مؤقت مع تحذير في السجل.

مثال (PowerShell):

```powershell
$env:Jwt__Key = "your-random-256-bit-secret-here"
```

يمكن أيضًا تقييد CORS عبر `Cors:AllowedOrigins` (قيم مفصولة بفواصل، الافتراضي `http://localhost:4200`).

### 3. Run the backend

```bash
dotnet restore
dotnet run --project src/InventorySystem.API
```

بعد التشغيل، افتح Swagger من الرابط الذي يظهر في الطرفية، وغالبًا سيكون:

```text
http://localhost:5000/swagger
```

إذا شغّلت المشروع باستخدام IIS Express من Visual Studio فسيكون الرابط:

```text
https://localhost:44330/swagger
```

### 4. Run the frontend

في نافذة طرفية أخرى:

```bash
cd frontend
npm install
npm start
```

واجهة Angular تعمل افتراضيًا على:

```text
http://localhost:4200
```

عنوان الـ API يُضبط الآن من ملف `frontend/src/environments/environment.ts` (قيمة `apiUrl`) بدل الكتابة اليدوية داخل `api.service.ts`. القيمة الافتراضية `https://localhost:44330/api` لتوافق IIS Express؛ عند تشغيل الـ API باستخدام `dotnet run` على `http://localhost:5000`، غيّرها إلى `http://localhost:5000/api`.

## Development Seed Data

عند تشغيل الـ API لأول مرة، ينشئ النظام قاعدة البيانات وبيانات تجريبية تشمل التصنيفات والمخازن والمنتجات وحركات المخزون.

حساب المدير الافتراضي للتطوير:

```text
Email:    admin@inventory.com
Password: Admin@123
```

هذه البيانات مخصصة للتطوير المحلي فقط. غيّر كلمة المرور واحذف أو عطّل بيانات الحساب الافتراضية قبل النشر.

## Main API Endpoints

| Method | Endpoint | Description |
| --- | --- | --- |
| `POST` | `/api/auth/login` | تسجيل الدخول وإرجاع JWT + refresh token |
| `POST` | `/api/auth/register` | إنشاء مستخدم جديد (الدور الافتراضي Cashier) |
| `POST` | `/api/auth/refresh` | تجديد الـ JWT باستخدام refresh token |
| `POST` | `/api/auth/revoke` | إلغاء refresh token |
| `GET` | `/api/products` | استعراض المنتجات |
| `POST` | `/api/products` | إضافة منتج (Admin / Manager) |
| `GET` | `/api/products/{id}` | تفاصيل منتج |
| `PUT` | `/api/products/{id}` | تعديل منتج (Admin / Manager) |
| `DELETE` | `/api/products/{id}` | حذف ناعم لمنتج (Admin / Manager) |
| `GET` | `/api/categories` | استعراض التصنيفات |
| `POST` | `/api/categories` | إضافة تصنيف (Admin / Manager) |
| `GET` | `/api/categories/{id}` | تفاصيل تصنيف |
| `PUT` | `/api/categories/{id}` | تعديل تصنيف (Admin / Manager) |
| `DELETE` | `/api/categories/{id}` | حذف ناعم لتصنيف (Admin / Manager) |
| `GET` | `/api/warehouses` | استعراض المخازن |
| `POST` | `/api/warehouses` | إضافة مخزن (Admin / Manager) |
| `GET` | `/api/warehouses/{id}` | تفاصيل مخزن |
| `PUT` | `/api/warehouses/{id}` | تعديل مخزن (Admin / Manager) |
| `DELETE` | `/api/warehouses/{id}` | حذف ناعم لمخزن (Admin / Manager) |
| `GET` | `/api/suppliers` | استعراض الموردين |
| `POST` | `/api/suppliers` | إضافة مورد (Admin / Manager) |
| `GET` | `/api/suppliers/{id}` | تفاصيل مورد |
| `PUT` | `/api/suppliers/{id}` | تعديل مورد (Admin / Manager) |
| `DELETE` | `/api/suppliers/{id}` | حذف مورد (Admin / Manager) |
| `POST` | `/api/stock/movement` | تسجيل حركة مخزون (تسجيل دخول مطلوب) |
| `GET` | `/api/stock/movements` | سجل حركات المخزون مع فلاتر (منتج، مخزن، نوع، تاريخ) |
| `GET` | `/api/stock/low-stock` | استعراض الأصناف منخفضة المخزون |
| `GET` | `/api/dashboard/summary` | ملخص لوحة المعلومات (تسجيل دخول مطلوب) |

## Testing

لتشغيل اختبارات الوحدة:

```bash
dotnet test
```

## Security Notes

- التطبيق يرفض التشغيل خارج بيئة التطوير إذا لم يتم ضبط `Jwt:Key` بقيمة حقيقية (متغير البيئة `Jwt__Key`).
- لا تضع كلمات المرور أو مفاتيح JWT الحقيقية داخل Git.
- استخدم User Secrets أو متغيرات البيئة لإعدادات الإنتاج.
- بيانات التطوير التجريبية (بما فيها حساب `admin@inventory.com`) تُزرع في بيئة التطوير فقط.
- راجع `Cors:AllowedOrigins` قبل النشر؛ القيمة الافتراضية تسمح بـ `http://localhost:4200` فقط.

## Status

النظام يشمل الآن: إدارة المنتجات والتصنيفات والمخازن والموردين (CRUD كامل)، حركات المخزون مع سجل قابل للفلترة، التحويل بين المخازن بقيد مزدوج، ملخص لوحة المعلومات، refresh tokens، واختبارات وحدة وتكامل.

## License

لم يتم تحديد ترخيص للمشروع بعد. أضف ملف `LICENSE` قبل إعادة استخدام المشروع أو توزيعه بشكل عام.
