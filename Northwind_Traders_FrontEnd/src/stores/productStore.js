import { defineStore } from "pinia";
import { ref } from "vue";
import {
  getAllProducts,
  getProductsByCategory,
} from "../axiosInstance/productService.js";

export const useProductStore = defineStore("products", () => {
  const products = ref([]);
  const loading = ref(false);
  const error = ref(null);

  async function fetchProducts() {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await getAllProducts();
      products.value = data;
    } catch (e) {
      error.value = e;
      throw e;
    } finally {
      loading.value = false;
    }
  }

  async function fetchByCategory(id) {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await getProductsByCategory(id);
      products.value = data;
    } catch (e) {
      error.value = e;
      throw e;
    } finally {
      loading.value = false;
    }
  }

  return { products, loading, error, fetchProducts, fetchByCategory };
});
