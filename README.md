# Northwind Traders

A full-stack internal management system for the Northwind Traders company.  
Built with **ASP.NET Core 8** (backend) and **Vue 3 + Vite** (frontend).

---

## Repository Structure

```
Northwind_Traders/
├── Northwind_Traders_BackEnd/    # ASP.NET Core 8 REST API
│   ├── src/
│   │   ├── NorthwindTraders.API/
│   │   ├── NorthwindTraders.Application/
│   │   ├── NorthwindTraders.Domain/
│   │   └── NorthwindTraders.Infrastructure/
│   └── READEME.md               # Backend documentation
│
└── Northwind_Traders_FronEnd/    # Vue 3 SPA
    ├── src/
    └── README.md                 # Frontend documentation
```

---

## Tech Stack

### Backend

| Layer          | Technology                    |
| -------------- | ----------------------------- |
| Framework      | ASP.NET Core 8                |
| ORM            | Entity Framework Core 8       |
| Database       | SQL Server                    |
| Auth           | JWT Bearer Tokens             |
| PDF Generation | QuestPDF                      |
| Excel Export   | ClosedXML                     |
| Geocoding      | Google Maps Geocoding API     |
| Architecture   | Clean Architecture (4 layers) |

### Frontend

| Layer      | Technology              |
| ---------- | ----------------------- |
| Framework  | Vue 3 (Composition API) |
| Build Tool | Vite                    |
| State      | Pinia                   |
| Routing    | Vue Router              |
| HTTP       | Axios                   |
| Charts     | Chart.js + vue-chartjs  |
| Styling    | Sass                    |
| Maps       | Google Maps JS API      |

---

## Getting Started

### 1 — Backend

```bash
cd Northwind_Traders_BackEnd/src/NorthwindTraders.API

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=YOUR_SERVER;Database=Northwind;Trusted_Connection=True;"
dotnet user-secrets set "Jwt:Key" "your-secret-key-min-32-chars"
dotnet user-secrets set "GoogleMaps:ApiKey" "your-google-maps-api-key"

dotnet run
```

API runs at `https://localhost:5272` — Swagger at `https://localhost:5272/swagger`

### 2 — Frontend

```bash
cd Northwind_Traders_FronEnd

# Create .env.local
echo "VITE_API_BASE_URL=http://localhost:5272/api/v1" > .env.local
echo "VITE_GOOGLE_MAPS_KEY=your-google-maps-api-key" >> .env.local

npm install
npm run dev
```

App runs at `http://localhost:5173`

---

## Running with Docker

The full stack (backend API + Vue frontend served by Nginx) can be started with a single command.

### Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine + Compose plugin)
- A **SQL Server** instance reachable from the Docker host
- A **Google Maps API key** with the Geocoding and Maps JavaScript APIs enabled

### 1 — Create a `.env` file

Create a `.env` file in the repository root (next to `docker-compose.yml`):

```env
DB_CONNECTION_STRING=Server=host.docker.internal;Database=Northwind;User Id=sa;Password=YourPass;TrustServerCertificate=True;
JWT_KEY=your-long-random-secret-min-32-chars
GOOGLE_MAPS_API_KEY=your-google-maps-api-key
```

> ⚠️ Never commit `.env` — it is in `.gitignore`.

### 2 — Build and start

```bash
docker compose up --build
```

| Service  | URL                   |
| -------- | --------------------- |
| Frontend | http://localhost      |
| Backend  | http://localhost:5272 |

### 3 — Stop

```bash
docker compose down
```

---

## Test Credentials

| Name             | Email                          | Title                    | Password       |
| ---------------- | ------------------------------ | ------------------------ | -------------- |
| Nancy Davolio    | nancy.davolio@northwind.com    | Sales Representative     | Northwind2025! |
| Andrew Fuller    | andrew.fuller@northwind.com    | Vice President, Sales ⭐ | Northwind2025! |
| Janet Leverling  | janet.leverling@northwind.com  | Sales Representative     | Northwind2025! |
| Margaret Peacock | margaret.peacock@northwind.com | Sales Representative     | Northwind2025! |
| Steven Buchanan  | steven.buchanan@northwind.com  | Sales Manager ⭐         | Northwind2025! |
| Michael Suyama   | michael.suyama@northwind.com   | Sales Representative     | Northwind2025! |
| Robert King      | robert.king@northwind.com      | Sales Representative     | Northwind2025! |
| Laura Callahan   | laura.callahan@northwind.com   | Inside Sales Coordinator | Northwind2025! |
| Anne Dodsworth   | anne.dodsworth@northwind.com   | Sales Representative     | Northwind2025! |

> ⭐ These accounts have access to the **Employees management** module.

---

## Features

- 🔐 JWT Authentication — all endpoints and routes protected
- 📦 Full Order Management — create, update, filter, export PDF & Excel
- 🗺️ Google Maps Integration — geocoded order pins on customer and order detail
- 📊 Dashboard — stat cards + charts (donut, bar) with date range filter
- 👥 Employee Management — title and profile editing for managers/VPs
- 🛍️ Product Catalog — card grid with category filter and low stock alerts
- 📄 PDF Export — download any order as a PDF invoice
- 📊 Excel Export — export full orders table
- 🌙 Dark Mode — toggle persisted in localStorage
- 📱 Fully Responsive — mobile-first with CSS Grid + Flexbox

---

## Architecture

### Backend — Clean Architecture

```
API → Application → Domain ← Infrastructure
```

- **Domain** — entities, interfaces. Zero dependencies.
- **Application** — DTOs only.
- **Infrastructure** — EF Core, repositories, geocoding, PDF, Excel services.
- **API** — controllers, Program.cs, JWT config.

### Frontend — Feature-based

```
views → stores → axiosInstance/services → API
```

- **Views** — pages, call stores
- **Stores** (Pinia) — business logic, one per domain
- **Services** (axiosInstance) — one file per API resource, single responsibility
- **Components** — reusable UI split by domain

---

## Documentation

- 📖 [Backend README](./Northwind_Traders_BackEnd)
- 📖 [Frontend README](./Northwind_Traders_FrontEnd)
