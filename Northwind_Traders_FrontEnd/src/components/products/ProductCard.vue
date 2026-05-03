<script setup>
defineProps({
  product: { type: Object, required: true },
});

// Assign a stable color per category ID
const CATEGORY_COLORS = [
  "#7c3aed",
  "#f59e0b",
  "#10b981",
  "#ef4444",
  "#3b82f6",
  "#ec4899",
  "#14b8a6",
  "#f97316",
];

function categoryColor(id) {
  return CATEGORY_COLORS[(id - 1) % CATEGORY_COLORS.length];
}

function formatCurrency(n) {
  return n != null
    ? new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
      }).format(n)
    : "—";
}
</script>

<template>
  <div class="product-card glass">
    <!-- Category badge -->
    <span
      class="product-card__category"
      :style="{
        background: categoryColor(product.categoryId) + '22',
        color: categoryColor(product.categoryId),
      }"
    >
      {{ product.categoryName || `Cat. ${product.categoryId}` }}
    </span>

    <!-- Name -->
    <h3 class="product-card__name">{{ product.productName }}</h3>

    <!-- Price -->
    <div class="product-card__price">
      {{ formatCurrency(product.unitPrice) }}
    </div>

    <!-- Stock info -->
    <div
      class="product-card__stock"
      :class="{ 'low-stock': product.unitsInStock < 10 }"
    >
      <span class="stock-dot" />
      {{ product.unitsInStock }} in stock
      <span
        v-if="product.unitsInStock < 10"
        class="badge badge-danger low-badge"
        >Low Stock</span
      >
    </div>

    <!-- Discontinued -->
    <span
      v-if="product.discontinued"
      class="badge badge-default discontinued-badge"
    >
      Discontinued
    </span>
  </div>
</template>

<style
  lang="scss"
  src="../../assets/styles/Components/ProductCard.scss"
  scoped
></style>
