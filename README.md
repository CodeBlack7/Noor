# Noor — Modest Atelier

An editorial ecommerce application for modest wear: a burqa-first catalogue that scales to abayas, hijabs, accessories, and any future collection. It pairs a complete Angular storefront with an atelier admin desk for products, collections, orders, clients, and coupons.

The application is a working demo for the Indian market: SQL Server LocalDB holds the catalogue and orders, the .NET API issues real JWTs, prices are in INR, and checkout is cash on delivery only (no payment processor is contacted).

---

## Stack

| Layer | Technology |
| --- | --- |
| Frontend | Angular 20.3, standalone components, signals, SCSS, lazy routes, zone-based change detection |
| Backend | .NET 8 Web API, EF Core 8.0.11, JWT + rotating refresh tokens, PBKDF2 password hashing |
| Database | SQL Server LocalDB (`NoorDb`), migration `20261029160334_InitialCreate` |
| Tests | xUnit (`Noor.Tests`) for slugs, coupons, password hashing, and JWT issuance |
| Tooling | Portable Node 22.23.4 at `C:\Users\rahul\tools\node22` |

### Solution layout

```
Noor.sln
backend/
  Noor.Core/            entities, DTOs, options, exceptions, security interfaces
  Noor.Infrastructure/  EF Core context, migration, seeder, services, helpers
  Noor.Api/             controllers, middleware, JSON config, static assets, Swagger
  Noor.Tests/           xUnit unit tests
frontend/
  noor-web/             Angular application
tools/
  generate-art.mjs      script that produced the bespoke SVG art
scripts/                PowerShell helpers for running and building
```

---

## Quick start

### Prerequisites

- .NET 8 SDK
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`)
- Node.js 22 and npm (portable install: `C:\Users\rahul\tools\node22`)

### 1. Restore tools and packages

```powershell
dotnet tool restore
dotnet restore Noor.sln
```

### 2. Database

The development profile enables `Database:AutoMigrate`, so the first API run creates `NoorDb` and seeds demo data automatically. To do it explicitly:

```powershell
dotnet ef database update `
  --project backend\Noor.Infrastructure `
  --startup-project backend\Noor.Api
```

To start over from the seed:

```powershell
.\scripts\reset-db.ps1
```

### 3. Run

```powershell
# Terminal 1 — API on http://localhost:5080 (Swagger at /swagger)
.\scripts\run-api.ps1

# Terminal 2 — Angular on http://localhost:4200
.\scripts\run-web.ps1
```

Or open both in new windows:

```powershell
.\scripts\run-all.ps1
```

The Angular dev server proxies `/api`, `/images`, and `/uploads` to `http://localhost:5080`.

### 4. Build and test

```powershell
.\scripts\build.ps1   # dotnet build + npm production build
.\scripts\test.ps1    # dotnet test (xUnit)
```

---

## Demo accounts

| Role | Email | Password |
| --- | --- | --- |
| Admin | `admin@noor.store` | `Admin@12345` |
| Customer | `demo@noor.store` | `Demo@12345` |
| Customer | `sara@noor.store` | `Sara@12345` |
| Customer | `leila@noor.store` | `Leila@12345` |
| Customer | `yasmin@noor.store` | `Yasmin@12345` |

Seeded coupons:

| Code | Offer | Condition |
| --- | --- | --- |
| `WELCOME10` | 10% off | none |
| `NOOR500` | ₹500 off | subtotal ≥ ₹4,999 |
| `EID20` | 20% off | subtotal ≥ ₹4,999 |

The seed also creates 20 products, 6 collections, 9 orders, reviews, addresses, and wishlist data.

---

## Features

### Storefront

- Editorial home page with hero, featured pieces, new arrivals, bestsellers, collections, and brand story
- Shop with search, collection, price, size, and colour filters plus sorting and pagination
- Product detail with gallery, colour/size selection, stock awareness, reviews, related pieces, wishlist, and add-to-bag
- Guest cart using an `X-Cart-Key`; the guest cart merges into the account cart on sign-in
- Slide-out cart drawer, full bag page, free-shipping progress, and coupon entry
- Account area: profile, password change, addresses, orders, order detail, wishlist
- Checkout with saved/new addresses, standard or express shipping, coupon discounts, and cash-on-delivery payment (India only)
- Information pages (story, contact, shipping & returns, size guide) and a styled 404

### Admin

- Chrome-less admin shell with grouped navigation and mobile drawer
- Dashboard KPIs, 7-day sales chart, recent orders, top products, and low-stock alerts
- Products: search/filter table, create/edit form, image upload and URL editor with reordering, variant table, size-run generator, SKU generator, activation and featured flags
- Collections: inline create/edit, cover preview, visibility and sort order, guarded delete
- Orders: search by number/email/name, status filter, pagination, detail view with items, totals, addresses, notes, status/payment updates, carrier/tracking, cancel-with-restock, and print
- Clients: search, spend and order counts, marketing status, and deep links into filtered order history
- Coupons: create/edit, percentage or fixed discounts, minimums, redemption limits, schedules, live/scheduled/expired/paused/exhausted state, and delete

### Behaviour notes

- **Payments are cash on delivery only.** No card, UPI, or netbanking details are collected or stored. Orders stay `Pending` until an admin records cash collection. Online payments remain disabled until a secure payment partner is integrated.
- **India only.** Shipping addresses must be in India. Standard delivery is ₹99 (complimentary over ₹1,999) and express is ₹249. Returns are accepted within 7 days; COD refunds are issued by bank transfer.
- **Images.** Storefront art is bespoke SVG under `backend/Noor.Api/wwwroot/images`. Admin uploads accept JPG, PNG, or WebP up to 5 MB and are written to `wwwroot/uploads`.
- **Security.** Access tokens last 60 minutes; refresh tokens rotate and are stored as SHA-256 hashes. Passwords use PBKDF2-SHA512 with 120,000 iterations. Auth endpoints are rate-limited to 15 requests per minute per IP.
- **JSON contracts** are camelCase with string enums and UTC ISO dates.

---

## API overview

Base URL: `http://localhost:5080`. Swagger UI: `/swagger`.

| Area | Routes |
| --- | --- |
| Health | `GET /api/health` |
| Auth | `POST /api/auth/register`, `/login`, `/refresh`, `/logout`, `/change-password`; `GET /api/auth/me`; `PUT /api/auth/profile` |
| Catalogue | `GET /api/home`, `/api/categories`, `/api/products`, `/api/products/facets`, `/api/products/{slug}`, `/api/products/{id}/reviews`; `POST /api/products/{id}/reviews` |
| Cart | `GET /api/cart`, `POST /api/cart/items`, `PUT/DELETE /api/cart/items/{id}`, `POST /api/cart/coupon`, `DELETE /api/cart/coupon`, `POST /api/cart/merge` |
| Orders | `POST /api/orders`, `GET /api/orders`, `GET /api/orders/{id}`, `POST /api/orders/{id}/cancel` |
| Account | `GET/POST /api/addresses`, `PUT/DELETE /api/addresses/{id}`, `POST /api/addresses/{id}/default`, `GET /api/wishlist`, `POST /api/wishlist/{productId}` (toggles), `POST /api/newsletter` |
| Admin | `/api/admin/dashboard`, `/products`, `/categories`, `/orders`, `/coupons`, `/customers`, `/uploads` |

Admin routes require an `Admin` role token. Customer routes require authentication except for catalogue and guest cart access.

---

## Configuration

`backend/Noor.Api/appsettings.json` holds the LocalDB connection string, JWT issuer/audience/lifetimes, store economics, CORS origins, and `Database:AutoMigrate`.

`appsettings.Development.json` contains the local JWT signing secret and enables auto-migration/seeding. Before deploying anywhere real:

- Replace `Jwt:Secret` with a securely stored value of at least 32 characters.
- Point `ConnectionStrings:DefaultConnection` at your SQL Server.
- Restrict `Cors:AllowedOrigins`.
- Disable `Database:AutoMigrate` and apply migrations deliberately.
- Integrate a secure payment partner before enabling online payments; keep cash on delivery available until then.

---

## Scripts

| Script | Purpose |
| --- | --- |
| `scripts/run-api.ps1` | Runs the API with the `http` profile on port 5080 |
| `scripts/run-web.ps1` | Adds portable Node to `PATH` and runs `npm start` |
| `scripts/run-all.ps1` | Opens API and web servers in separate PowerShell windows |
| `scripts/build.ps1` | Builds the solution and the Angular production bundle |
| `scripts/test.ps1` | Runs the xUnit suite |
| `scripts/reset-db.ps1` | Drops `NoorDb` after confirmation; next API run recreates and seeds it |

The run-web script accepts a custom Node location:

```powershell
.\scripts\run-web.ps1 -NodeHome 'C:\Program Files\nodejs'
```
