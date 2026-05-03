import axios from 'axios'
import { useAuthStore } from '../stores/authStore.js'
import router from '../router/index.js'

// Single axios instance — all services import this
const api = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL,
  headers: { 'Content-Type': 'application/json' },
})

// ── Request interceptor: attach JWT token ──────────────────────
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('nt_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// ── Response interceptor: handle 401 globally ─────────────────
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Clear auth state and redirect to login on unauthorized
      try {
        const authStore = useAuthStore()
        authStore.logout()
      } catch {
        localStorage.removeItem('nt_token')
      }
      router.push('/login')
    }
    return Promise.reject(error)
  }
)

export default api
