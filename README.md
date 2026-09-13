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

حدّث `ConnectionStrings:DefaultConnection` إذا كنت تستخدم SQL Server مختلفًا، وغيّر `Jwt:Key` إلى قيمة طويلة وعشوائية قبل أي استخدام خارج بيئة التطوير.

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

عنوان الـ API مضبوط حاليًا داخل `frontend/src/app/core/api.service.ts` على `https://localhost:44330/api` ليتوافق مع IIS Express. عند تشغيل الـ API باستخدام `dotnet run` على `http://localhost:5000`، حدّث قيمة `base` في هذا الملف إلى `http://localhost:5000/api`.

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
| `POST` | `/api/auth/login` | تسجيل الدخول وإرجاع JWT |
| `POST` | `/api/auth/register` | إنشاء مستخدم جديد |
| `GET` | `/api/products` | استعراض المنتجات |
| `POST` | `/api/products` | إضافة منتج (Admin / Manager) |
| `GET` | `/api/categories` | استعراض التصنيفات |
| `POST` | `/api/categories` | إضافة تصنيف (Admin / Manager) |
| `GET` | `/api/warehouses` | استعراض المخازن |
| `POST` | `/api/warehouses` | إضافة مخزن (Admin / Manager) |
| `POST` | `/api/stock/movement` | تسجيل حركة مخزون |
| `GET` | `/api/stock/low-stock` | استعراض الأصناف منخفضة المخزون |

## Testing

لتشغيل اختبارات الوحدة:

```bash
dotnet test
```

## Security Notes

- لا تستخدم مفتاح JWT الموجود في إعدادات التطوير في الإنتاج.
- لا تضع كلمات المرور أو مفاتيح JWT الحقيقية داخل Git.
- استخدم User Secrets أو متغيرات البيئة لإعدادات الإنتاج.
- راجع سياسة CORS قبل النشر؛ إعداد التطوير الحالي يسمح بكل المصادر.

## Status

المشروع في مرحلة التطوير، ويمكن توسيعه بإضافة إدارة الموردين، تقارير المخزون، سجل التدقيق، وإدارة المستخدمين من الواجهة.

## License

لم يتم تحديد ترخيص للمشروع بعد. أضف ملف `LICENSE` قبل إعادة استخدام المشروع أو توزيعه بشكل عام.
