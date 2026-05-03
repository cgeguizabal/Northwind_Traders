<script setup>
import { useUiStore } from "../../stores/uiStore.js";
import { useAuthStore } from "../../stores/authStore.js";
import { useRoute } from "vue-router";
import { computed } from "vue";

const ui = useUiStore();
const auth = useAuthStore();
const route = useRoute();

// Derive a human-readable page title from the route path
const pageTitle = computed(() => {
  const map = {
    "/dashboard": "Dashboard",
    "/orders": "Orders",
    "/new-order": "New Order",
    "/customers": "Customers",
    "/products": "Products",
    "/suppliers": "Suppliers",
    "/categories": "Categories",
    "/shippers": "Shippers",
    "/employees": "Employees",
  };
  const key = Object.keys(map).find((k) => route.path.startsWith(k));
  return key ? map[key] : "Northwind Traders";
});

const employeeName = computed(() => auth.displayName || "Employee");
</script>

<template>
  <header class="app-header">
    <!-- Hamburger (mobile + sidebar toggle) -->
    <button
      class="app-header__hamburger"
      @click="ui.toggleSidebar()"
      aria-label="Toggle sidebar"
    >
      <span></span><span></span><span></span>
    </button>

    <h1 class="app-header__title">{{ pageTitle }}</h1>

    <div class="app-header__right">
      <span class="app-header__greeting">Hello, {{ employeeName }}</span>
    </div>
  </header>
</template>

<style
  lang="scss"
  src="../../assets/styles/Components/AppHeader.scss"
  scoped
></style>
