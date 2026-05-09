import { defineStore } from "pinia";
import { ref } from "vue";
import {
  getAllOrders,
  getOrderById,
  createOrder,
  updateOrder,
  updateOrderStatus,
  exportOrdersExcel,
  exportOrdersPdf,
  geocodeOrder,
  deactivateOrder,
} from "../axiosInstance/orderService.js";

export const useOrderStore = defineStore("orders", () => {
  const orders = ref([]);     // flat list used by the orders table
  const current = ref(null);  // single order loaded for the detail modal
  const loading = ref(false);
  const error = ref(null);

  async function fetchOrders() {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await getAllOrders();
      orders.value = data;
    } catch (e) {
      error.value = e;
      throw e;
    } finally {
      loading.value = false;
    }
  }

  async function fetchOrder(id) {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await getOrderById(id);
      current.value = data;
      return data;
    } catch (e) {
      error.value = e;
      throw e;
    } finally {
      loading.value = false;
    }
  }

  // Creates the order, then silently geocodes its addresses in the background
  async function submitCreateOrder(payload) {
    const { data } = await createOrder(payload);

    try {
      await geocodeOrder(data.orderId ?? data.id);
    } catch (e) {
      console.warn("[OrderStore] Silent geocode failed:", e.message);
    }
    return data;
  }

  async function submitUpdateOrder(id, payload) {
    const { data } = await updateOrder(id, payload);
    return data;
  }

  async function submitUpdateStatus(id, statusId) {
    await updateOrderStatus(id, statusId);
  }

  // Soft-deletes by deactivating on the server, then removes from local list
  async function softDeleteOrder(id) {
    await deactivateOrder(id);
    orders.value = orders.value.filter((o) => o.orderId !== id);
  }

  // Returns the raw Blob — caller is responsible for creating and revoking the URL
  async function fetchExcelBlob() {
    const { data } = await exportOrdersExcel();
    return data;
  }

  // Returns the raw Blob — caller is responsible for creating and revoking the URL
  async function fetchPdfBlob() {
    const { data } = await exportOrdersPdf();
    return data;
  }

  return {
    orders,
    current,
    loading,
    error,
    fetchOrders,
    fetchOrder,
    submitCreateOrder,
    submitUpdateOrder,
    submitUpdateStatus,
    softDeleteOrder,
    fetchExcelBlob,
    fetchPdfBlob,
  };
});
