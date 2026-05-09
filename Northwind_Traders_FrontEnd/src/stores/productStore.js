import { defineStore } from "pinia";
import { ref } from "vue";
import {
  getAllProducts,
  getProductsByCategory,
} from "../axiosInstance/productService.js";

export const useProductStore = defineStore("products", () => {
  const products = ref([]);   // active product list (or filtered by category)
  const loading = ref(false);
  const error = ref(null);

  // Fetches all products regardless of category
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

  // Fetches products filtered to a single category
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
