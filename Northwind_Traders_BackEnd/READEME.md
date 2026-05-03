# Northwind Traders — Backend API

A RESTful API built with **ASP.NET Core 8** and **Entity Framework Core** for the Northwind Traders internal management system.  
Employees can manage orders, customers, products, suppliers, categories, and shippers — all secured with JWT authentication.

---

## Tech Stack

| Layer          | Technology                    |
| -------------- | ----------------------------- |
| Framework      | ASP.NET Core 8                |
| ORM            | Entity Framework Core 8       |
| Database       | SQL Server                    |
| Auth           | JWT Bearer Tokens             |
| PDF Generation | QuestPDF                      |
| Geocoding      | Google Maps Geocoding API     |
| Architecture   | Clean Architecture (4 layers) |

---

## Project Structure

```
Northwind_Traders_BackEnd/
└── src/
    ├── NorthwindTraders.API/              # Controllers, Program.cs, appsettings
    ├── NorthwindTraders.Application/      # DTOs
    ├── NorthwindTraders.Domain/           # Entities, Interfaces, Common (Result<T>)
    └── NorthwindTraders.Infrastructure/   # Repositories, Services, Persistence (EF Core)
```

---

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- SQL Server (local or Azure)
- Google Maps API Key (for geocoding)

### 1 — Clone the repository

```bash
git clone https://github.com/cgeguizabal/Northwind_Traders.git
cd Northwind_Traders/Northwind_Traders_BackEnd
```

### 2 — Set up User Secrets

Secrets are never stored in files. Use the .NET User Secrets manager:

```bash
cd src/NorthwindTraders.API

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=Northwind;Trusted_Connection=True;"
dotnet user-secrets set "Jwt:Key" "your-local-dev-secret-key-min-32-chars"
dotnet user-secrets set "GoogleMaps:ApiKey" "your-google-maps-api-key"
```

### 3 — Run the API

```bash
dotnet run --project src/NorthwindTraders.API
```

Swagger UI will be available at:

```
https://localhost:{port}/swagger
```

---

## Authentication

All endpoints except `POST /api/v1/auth/login` require a JWT bearer token.

### Login

```
POST /api/v1/auth/login
```

```json
{
  "username": "your-username",
  "password": "your-password"
}
```

### Test Credentials

All employees share the same default password:

| Name             | Email                          | Password       |
| ---------------- | ------------------------------ | -------------- |
| Nancy Davolio    | nancy.davolio@northwind.com    | Northwind2025! |
| Andrew Fuller    | andrew.fuller@northwind.com    | Northwind2025! |
| Janet Leverling  | janet.leverling@northwind.com  | Northwind2025! |
| Margaret Peacock | margaret.peacock@northwind.com | Northwind2025! |
| Steven Buchanan  | steven.buchanan@northwind.com  | Northwind2025! |
| Michael Suyama   | michael.suyama@northwind.com   | Northwind2025! |
| Robert King      | robert.king@northwind.com      | Northwind2025! |
| Laura Callahan   | laura.callahan@northwind.com   | Northwind2025! |
| Anne Dodsworth   | anne.dodsworth@northwind.com   | Northwind2025! |

Returns:

```json
{
  "token": "eyJhbGci..."
}
```

Use this token in Swagger via the **Authorize** button, or in requests:

```
Authorization: Bearer eyJhbGci...
```

---

## API Endpoints

### Auth

| Method | Endpoint           | Description        |
| ------ | ------------------ | ------------------ |
| POST   | /api/v1/auth/login | Login, returns JWT |

### Customers

| Method | Endpoint                   | Description                     |
| ------ | -------------------------- | ------------------------------- |
| GET    | /api/v1/customers          | Get all customers               |
| GET    | /api/v1/customers/{id}     | Get customer detail with orders |
| GET    | /api/v1/customers/{id}/map | Get geocoded order pins for map |

### Employees

| Method | Endpoint                     | Description                     |
| ------ | ---------------------------- | ------------------------------- |
| GET    | /api/v1/employees            | Get all employees               |
| GET    | /api/v1/employees/{id}       | Get employee detail with orders |
| GET    | /api/v1/employees/{id}/photo | Get employee photo (binary)     |

### Orders

| Method | Endpoint                         | Description                            |
| ------ | -------------------------------- | -------------------------------------- |
| GET    | /api/v1/orders                   | Get all orders                         |
| GET    | /api/v1/orders/{id}              | Get full order detail                  |
| GET    | /api/v1/orders/{id}/pdf          | Download order as PDF                  |
| GET    | /api/v1/orders/customer/{id}     | Get all orders for a customer          |
| GET    | /api/v1/orders/status/{statusId} | Get orders filtered by shipment status |
| PUT    | /api/v1/orders/{id}/status       | Update order shipment status           |
| POST   | /api/v1/orders/{id}/geocode      | Geocode a single order                 |
| POST   | /api/v1/orders/geocode-all       | Geocode all pending orders             |

### Products

| Method | Endpoint                       | Description                         |
| ------ | ------------------------------ | ----------------------------------- |
| GET    | /api/v1/products               | Get all products                    |
| GET    | /api/v1/products/{id}          | Get product detail + low stock flag |
| GET    | /api/v1/products/category/{id} | Get products by category            |
| GET    | /api/v1/products/active        | Get only non-discontinued products  |

### Suppliers

| Method | Endpoint               | Description                          |
| ------ | ---------------------- | ------------------------------------ |
| GET    | /api/v1/suppliers      | Get all suppliers with product count |
| GET    | /api/v1/suppliers/{id} | Get supplier detail with products    |

### Categories

| Method | Endpoint                | Description                           |
| ------ | ----------------------- | ------------------------------------- |
| GET    | /api/v1/categories      | Get all categories with product count |
| GET    | /api/v1/categories/{id} | Get category detail with products     |

### Shippers

| Method | Endpoint              | Description                       |
| ------ | --------------------- | --------------------------------- |
| GET    | /api/v1/shippers      | Get all shippers with order count |
| GET    | /api/v1/shippers/{id} | Get shipper detail with orders    |

### Dashboard

| Method | Endpoint          | Description         |
| ------ | ----------------- | ------------------- |
| GET    | /api/v1/dashboard | Get dashboard stats |

---

## Environment Variables (Production)

In production, set these as environment variables on your server.  
Double underscore `__` represents nested keys.

```
ConnectionStrings__DefaultConnection   = your-production-connection-string
Jwt__Key                               = your-long-random-production-key
Jwt__Issuer                            = NorthwindTraders.API
Jwt__Audience                          = NorthwindTraders.Client
Jwt__ExpiryMinutes                     = 60
GoogleMaps__ApiKey                     = your-google-maps-api-key
AllowedOrigins__0                      = https://your-frontend-domain.com
```

Generate a secure production JWT key:

```bash
openssl rand -base64 32
```

---

## Architecture

This project follows **Clean Architecture** with strict dependency rules:

```
API → Application → Domain ← Infrastructure
```

- **Domain** — entities and interfaces. No dependencies.
- **Application** — DTOs. Depends only on Domain.
- **Infrastructure** — EF Core, repositories, services. Depends on Domain.
- **API** — controllers, Program.cs. Depends on all layers.

### Design Patterns Applied

- **Repository Pattern** — all data access behind interfaces
- **Dependency Injection** — all dependencies injected via constructor
- **DTO Pattern** — domain entities never exposed directly
- **Result Pattern** — `Result<T>` used in geocoding to avoid exceptions for expected failures
- **SOLID Principles** — single responsibility per class, open/closed via `IRepository<T>`, dependency inversion throughout

---

## Features

- 🔐 JWT Authentication — all endpoints protected
- 📄 PDF Generation — download any order as a PDF invoice
- 🗺️ Geocoding — Google Maps integration to geocode order addresses
- 📍 Map Pins — customer order locations returned as lat/lng coordinates
- 📊 Dashboard — aggregated stats (total orders, revenue, pending shipments)
- 🔄 Bulk Geocoding — geocode all pending orders in one request with rate limiting
