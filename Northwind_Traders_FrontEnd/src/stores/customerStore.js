import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getAllCustomers, getCustomerById, getCustomerMapPins } from '../axiosInstance/customerService.js'

export const useCustomerStore = defineStore('customers', () => {
  const customers  = ref([])
  const current    = ref(null)
  const mapPins    = ref([])
  const loading    = ref(false)
  const error      = ref(null)
  const page       = ref(1)
  const totalPages = ref(1)
  const totalCount = ref(0)

  async function fetchCustomers(pageNum = 1, search = '') {
    loading.value = true
    error.value   = null
    try {
      const { data } = await getAllCustomers(pageNum, 10, search)
      customers.value = data.items
      page.value       = data.page
      totalPages.value = data.totalPages
      totalCount.value = data.totalCount
    } catch (e) {
      error.value = e
      throw e
    } finally {
      loading.value = false
    }
  }

  async function fetchCustomer(id) {
    loading.value = true
    error.value   = null
    try {
      const { data } = await getCustomerById(id)
      current.value  = data
      return data
    } catch (e) {
      error.value = e
      throw e
    } finally {
      loading.value = false
    }
  }

  async function fetchMapPins(id) {
    const { data } = await getCustomerMapPins(id)
    mapPins.value  = data
    return data
  }

  return { customers, current, mapPins, loading, error, page, totalPages, totalCount, fetchCustomers, fetchCustomer, fetchMapPins }
})
