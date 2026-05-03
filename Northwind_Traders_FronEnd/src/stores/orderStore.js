import { defineStore } from 'pinia'
import { ref } from 'vue'
import {
  getAllOrders,
  getOrderById,
  createOrder,
  updateOrder,
  updateOrderStatus,
  exportOrdersExcel,
  geocodeOrder,
} from '../axiosInstance/orderService.js'

export const useOrderStore = defineStore('orders', () => {
  const orders  = ref([])
  const current = ref(null)
  const loading = ref(false)
  const error   = ref(null)

  async function fetchOrders() {
    loading.value = true
    error.value   = null
    try {
      const { data } = await getAllOrders()
      orders.value = data
    } catch (e) {
      error.value = e
      throw e
    } finally {
      loading.value = false
    }
  }

  async function fetchOrder(id) {
    loading.value = true
    error.value   = null
    try {
      const { data } = await getOrderById(id)
      current.value  = data
      return data
    } catch (e) {
      error.value = e
      throw e
    } finally {
      loading.value = false
    }
  }

  async function submitCreateOrder(payload) {
    const { data } = await createOrder(payload)
    // Silently geocode after creation — errors are swallowed per spec
    try { await geocodeOrder(data.orderId ?? data.id) } catch {}
    return data
  }

  async function submitUpdateOrder(id, payload) {
    const { data } = await updateOrder(id, payload)
    return data
  }

  async function submitUpdateStatus(id, statusId) {
    await updateOrderStatus(id, statusId)
  }

  async function downloadExcel() {
    const { data } = await exportOrdersExcel()
    const url  = URL.createObjectURL(new Blob([data]))
    const link = document.createElement('a')
    link.href       = url
    link.download   = 'orders.xlsx'
    link.click()
    URL.revokeObjectURL(url)
  }

  return {
    orders, current, loading, error,
    fetchOrders, fetchOrder, submitCreateOrder, submitUpdateOrder,
    submitUpdateStatus, downloadExcel,
  }
})
