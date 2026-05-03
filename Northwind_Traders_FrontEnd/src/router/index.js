import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/authStore.js'

// Lazy-loaded views for better performance
const LoginView         = () => import('../views/LoginView.vue')
const DashboardView     = () => import('../views/DashboardView.vue')
const OrdersView        = () => import('../views/OrdersView.vue')
const NewOrderView      = () => import('../views/NewOrderView.vue')
const CustomersView     = () => import('../views/CustomersView.vue')
const CustomerDetailView = () => import('../views/CustomerDetailView.vue')
const EmployeesView     = () => import('../views/EmployeesView.vue')
const ProductsView      = () => import('../views/ProductsView.vue')
const SuppliersView     = () => import('../views/SuppliersView.vue')
const CategoriesView    = () => import('../views/CategoriesView.vue')
const ShippersView      = () => import('../views/ShippersView.vue')

const routes = [
  { path: '/',          redirect: '/dashboard' },
  { path: '/login',     component: LoginView,          meta: { public: true } },
  { path: '/dashboard', component: DashboardView },
  { path: '/orders',    component: OrdersView },
  { path: '/new-order', component: NewOrderView },
  { path: '/customers', component: CustomersView },
  { path: '/customers/:id', component: CustomerDetailView },
  { path: '/employees', component: EmployeesView,       meta: { managerOnly: true } },
  { path: '/products',  component: ProductsView },
  { path: '/suppliers', component: SuppliersView },
  { path: '/categories', component: CategoriesView },
  { path: '/shippers',  component: ShippersView },
  // Catch-all — redirect unauthenticated users to login, others to dashboard
  { path: '/:pathMatch(.*)*', redirect: '/dashboard' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
})

// ── Navigation guards ──────────────────────────────────────────
router.beforeEach((to) => {
  const auth = useAuthStore()

  // Allow public routes without authentication
  if (to.meta.public) return true

  // Unauthenticated → login
  if (!auth.isAuthenticated) return '/login'

  // Manager-only route — non-managers redirected to dashboard
  if (to.meta.managerOnly && !auth.isManager) return '/dashboard'

  return true
})

export default router
