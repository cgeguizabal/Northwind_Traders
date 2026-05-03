import { defineStore } from 'pinia'
import { ref } from 'vue'
import { getAllCustomers, getCustomerById, getCustomerMapPins } from '../axiosInstance/customerService.js'

export const useCustomerStore = defineStore('customers', () => {
  const customers = ref([])
  const current   = ref(null)
  const mapPins   = ref([])
  const loading   = ref(false)
  const error     = ref(null)

  async function fetchCustomers() {
    loading.value = true
    error.value   = null
    try {
      const { data } = await getAllCustomers()
      customers.value = data
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

  return { customers, current, mapPins, loading, error, fetchCustomers, fetchCustomer, fetchMapPins }
})
