<script setup>
import { ref, computed, onMounted, watch } from "vue";
import AppLayout from "../components/layout/AppLayout.vue";
import AppSpinner from "../components/common/AppSpinner.vue";
import ProductCard from "../components/products/ProductCard.vue";
import { useProductStore } from "../stores/productStore.js";
import { getAllCategories } from "../axiosInstance/categoryService.js";
import { useToast } from "vue-toastification";

const store = useProductStore();
const toast = useToast();
const categories = ref([]);
const selectedCat = ref("");  // empty = 'All Categories'

onMounted(async () => {
  try {
    // Fetch categories and products in parallel to reduce wait time
    const [cats] = await Promise.all([
      getAllCategories(),
      store.fetchProducts(),
    ]);
    categories.value = cats.data;
  } catch {
    toast.error("Failed to load products.");
  }
});

// Re-fetch when category filter changes
watch(selectedCat, async (val) => {
  try {
    if (val) {
      await store.fetchByCategory(val);
    } else {
      await store.fetchProducts();
    }
  } catch {
    toast.error("Failed to filter products.");
  }
});
</script>

<template>
  <AppLayout>
    <div class="page-container">
      <div class="page-header">
        <h1>Products</h1>
        <select
          v-model="selectedCat"
          class="form-control"
          style="max-width: 220px"
        >
          <option value="">All Categories</option>
          <option
            v-for="c in categories"
            :key="c.categoryId"
            :value="c.categoryId"
          >
            {{ c.categoryName }}
          </option>
        </select>
      </div>

      <div v-if="store.loading" class="spinner-center">
        <AppSpinner size="lg" />
      </div>

      <div v-else class="products-grid">
        <ProductCard
          v-for="p in store.products"
          :key="p.productId"
          :product="p"
        />
        <p v-if="!store.products.length" class="empty">No products found.</p>
      </div>
    </div>
  </AppLayout>
</template>

<style
  lang="scss"
  src="../assets/styles/Components/ProductsView.scss"
  scoped
></style>
