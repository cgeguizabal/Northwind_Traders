<script setup>
import { ref, onMounted } from "vue";
import AppLayout from "../components/layout/AppLayout.vue";
import AppSpinner from "../components/common/AppSpinner.vue";
import { getAllCategories } from "../axiosInstance/categoryService.js";
import { useToast } from "vue-toastification";

const categories = ref([]);
const loading = ref(false);
const toast = useToast();

// Assign stable colors to category cards
const COLORS = [
  "#7c3aed",
  "#f59e0b",
  "#10b981",
  "#ef4444",
  "#3b82f6",
  "#ec4899",
  "#14b8a6",
  "#f97316",
];
// Assign stable colors to category cards by cycling through the palette with modulo
function catColor(id) {
  return COLORS[(id - 1) % COLORS.length];
}

onMounted(async () => {
  loading.value = true;
  try {
    const { data } = await getAllCategories();
    categories.value = data;
  } catch {
    toast.error("Failed to load categories.");
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <AppLayout>
    <div class="page-container">
      <div class="page-header"><h1>Categories</h1></div>

      <div v-if="loading" class="spinner-center"><AppSpinner size="lg" /></div>

      <div v-else class="cat-grid">
        <div
          v-for="c in categories"
          :key="c.categoryId"
          class="cat-card glass"
          :style="{ borderTop: `3px solid ${catColor(c.categoryId)}` }"
        >
          <h3 class="cat-card__name">{{ c.categoryName }}</h3>
          <p class="cat-card__desc">{{ c.description }}</p>
          <span class="cat-card__count">
            {{ c.totalProducts ?? 0 }} products
          </span>
        </div>
      </div>
    </div>
  </AppLayout>
</template>

<style
  lang="scss"
  src="../assets/styles/Components/CategoriesView.scss"
  scoped
></style>
