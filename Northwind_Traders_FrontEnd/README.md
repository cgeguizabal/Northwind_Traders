# Northwind Traders — Frontend

A modern internal management SPA built with **Vue 3**, **Vite**, and **Sass** for the Northwind Traders system.  
Connects to the Northwind Traders Backend API secured with JWT authentication.

---

## Tech Stack

| Layer         | Technology               |
| ------------- | ------------------------ |
| Framework     | Vue 3 (Composition API)  |
| Build Tool    | Vite                     |
| Language      | JavaScript               |
| State         | Pinia                    |
| Routing       | Vue Router 4             |
| HTTP Client   | Axios                    |
| Charts        | Chart.js + vue-chartjs   |
| Styling       | Sass (no component libs) |
| Notifications | Vue-Toastification       |
| Maps          | Google Maps JS API       |
| Icons         | Iconoir Vue              |

---

## Project Structure

```
src/
├── assets/
│   └── styles/
│       ├── _variables.scss   # colors, fonts, breakpoints
│       ├── _mixins.scss      # media query mixins
│       ├── _reset.scss       # CSS reset
│       └── main.scss         # imports all partials
├── axiosInstance/
│   ├── index.js              # base axios instance + interceptors
│   ├── authService.js
│   ├── orderService.js
│   ├── customerService.js
│   ├── employeeService.js
│   ├── productService.js
│   ├── supplierService.js
│   ├── categoryService.js
│   ├── shipperService.js
│   ├── dashboardService.js
│   └── shipmentStateService.js
├── stores/
│   ├── authStore.js          # auth, JWT decode, role detection
│   ├── orderStore.js
│   ├── customerStore.js
│   ├── employeeStore.js
│   ├── productStore.js
│   ├── dashboardStore.js
│   └── uiStore.js            # dark mode, sidebar state
├── router/
│   └── index.js              # routes + navigation guards
├── components/
│   ├── layout/               # AppSidebar, AppHeader, AppLayout
│   ├── common/               # AppSpinner, AppModal, ConfirmDialog
│   ├── charts/               # DonutChart, BarChart
│   ├── orders/               # OrderTable, OrderDetailModal, OrderFormModal
│   ├── customers/            # CustomerMap
│   ├── employees/            # EmployeeEditModal
│   └── products/             # ProductCard
├── views/
│   ├── LoginView.vue
│   ├── DashboardView.vue
│   ├── OrdersView.vue         # tabs: Orders table + Reports
│   ├── NewOrderView.vue       # order form with embedded map
│   ├── CustomersView.vue
│   ├── CustomerDetailView.vue
│   ├── EmployeesView.vue      # managers only
│   ├── ProductsView.vue
│   ├── SuppliersView.vue
│   ├── CategoriesView.vue
│   └── ShippersView.vue
├── App.vue
└── main.js
```

---

## Getting Started

### Prerequisites

- [Node.js 18+](https://nodejs.org/)
- Northwind Traders Backend API running locally

### 1 — Install dependencies

```bash
cd Northwind_Traders_FronEnd
npm install
```

### 2 — Configure environment

Create a `.env.local` file in the `Northwind_Traders_FronEnd` folder:

```env
VITE_API_BASE_URL=http://localhost:5272/api/v1
VITE_GOOGLE_MAPS_KEY=your-google-maps-api-key
```

> ⚠️ Never commit `.env.local` — it is in `.gitignore`.

### 3 — Run the dev server

```bash
npm run dev
```

App will be available at `http://localhost:5173`

### 4 — Build for production

```bash
npm run build
```

---

## Authentication & Roles

- Login with any seeded employee account (see Backend README for credentials)
- JWT token is stored in `localStorage` under key `nt_token`
- Token is automatically attached to every API request via Axios interceptor
- If the token expires or returns 401, the user is redirected to `/login`

### Role-based access

| Title                 | Employees Page |
| --------------------- | -------------- |
| Vice President, Sales | ✅ Accessible  |
| Sales Manager         | ✅ Accessible  |
| Any other title       | ❌ Redirected  |

---

## Pages & Features

| Route            | Page            | Description                                  |
| ---------------- | --------------- | -------------------------------------------- |
| `/login`         | Login           | JWT login with validation                    |
| `/dashboard`     | Dashboard       | Stats cards + 3 charts + date range filter   |
| `/orders`        | Orders          | Table with filters, modals, PDF/Excel export |
| `/new-order`     | New Order       | Full order form with embedded Google Map     |
| `/customers`     | Customers       | Searchable customer list                     |
| `/customers/:id` | Customer Detail | Info card + map pins + orders table          |
| `/employees`     | Employees       | Manager-only employee grid with edit modal   |
| `/products`      | Products        | Card grid filtered by category               |
| `/suppliers`     | Suppliers       | Read-only table                              |
| `/categories`    | Categories      | Read-only cards                              |
| `/shippers`      | Shippers        | Read-only table                              |

---

## Design

- **Color palette:** Purple (`#7c3aed`), Gold (`#f59e0b`), Green (`#10b981`)
- **Glassmorphism** effect on cards, modals, and sidebar
- **Dark mode** toggle — persisted in `localStorage`
- **Fully responsive** — CSS Grid + Flexbox + Sass media query mixins
- **No component libraries** — all UI is custom Sass

---

## Environment Variables

| Variable               | Description                  |
| ---------------------- | ---------------------------- |
| `VITE_API_BASE_URL`    | Base URL for the backend API |
| `VITE_GOOGLE_MAPS_KEY` | Google Maps JS API key       |
