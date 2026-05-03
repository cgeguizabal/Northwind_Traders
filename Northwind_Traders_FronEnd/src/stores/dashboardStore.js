import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getDashboardStats } from '../axiosInstance/dashboardService.js'

export const useDashboardStore = defineStore('dashboard', () => {
  const stats   = ref(null)
  const loading = ref(false)
  const error   = ref(null)

  async function fetchStats() {
    loading.value = true
    error.value   = null
    try {
      const { data } = await getDashboardStats()
      stats.value = data
    } catch (e) {
      error.value = e
      throw e
    } finally {
      loading.value = false
    }
  }

  return { stats, loading, error, fetchStats }
})
