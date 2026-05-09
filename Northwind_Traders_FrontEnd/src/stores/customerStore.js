import { defineStore } from "pinia";
import { ref } from "vue";
import {
  getAllCustomers,
  getCustomerById,
  getCustomerMapPins,
} from "../axiosInstance/customerService.js";

export const useCustomerStore = defineStore("customers", () => {
  const customers = ref([]);   // current page of customers
  const current = ref(null);   // single customer loaded for the detail view
  const mapPins = ref([]);     // geocoded order locations for the map tab
  const loading = ref(false);
  const error = ref(null);
  const page = ref(1);
  const totalPages = ref(1);
  const totalCount = ref(0);

  // Fetches one page of customers; search is optional free-text filter
  async function fetchCustomers(pageNum = 1, search = "") {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await getAllCustomers(pageNum, 10, search);
      customers.value = data.items;
      page.value = data.page;
      totalPages.value = data.totalPages;
      totalCount.value = data.totalCount;
    } catch (e) {
      error.value = e;
      throw e;
    } finally {
      loading.value = false;
    }
  }

  // Fetches a single customer (with their orders) for the detail page
  async function fetchCustomer(id) {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await getCustomerById(id);
      current.value = data;
      return data;
    } catch (e) {
      error.value = e;
      throw e;
    } finally {
      loading.value = false;
    }
  }

  // Fetches geocoded order locations used to drop pins on the map
  async function fetchMapPins(id) {
    const { data } = await getCustomerMapPins(id);
    mapPins.value = data;
    return data;
  }

  return {
    customers,
    current,
    mapPins,
    loading,
    error,
    page,
    totalPages,
    totalCount,
    fetchCustomers,
    fetchCustomer,
    fetchMapPins,
  };
});
