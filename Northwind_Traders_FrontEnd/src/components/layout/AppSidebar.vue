<script setup>
import { computed } from "vue";
import { useRouter, useRoute } from "vue-router";
import { useAuthStore } from "../../stores/authStore.js";
import { useUiStore } from "../../stores/uiStore.js";
import ConfirmDialog from "../common/ConfirmDialog.vue";
import { ref } from "vue";
import {
  Home,
  Package,
  Plus,
  Group,
  ShoppingBag,
  Industry,
  Folder,
  Truck,
  User,
  Building,
  SunLight,
  HalfMoon,
  LogOut,
} from "iconoir-vue/regular";

const auth = useAuthStore();
const ui = useUiStore();
const router = useRouter();
const route = useRoute();

const showLogoutDialog = ref(false);

const navItems = computed(() => [
  { label: "Dashboard", icon: Home, path: "/dashboard" },
  { label: "Orders", icon: Package, path: "/orders" },
  { label: "New Order", icon: Plus, path: "/new-order" },
  { label: "Customers", icon: Group, path: "/customers" },
  { label: "Products", icon: ShoppingBag, path: "/products" },
  { label: "Suppliers", icon: Industry, path: "/suppliers" },
  { label: "Categories", icon: Folder, path: "/categories" },
  { label: "Shippers", icon: Truck, path: "/shippers" },
  // Only rendered for managers
  ...(auth.isManager
    ? [{ label: "Employees", icon: User, path: "/employees" }]
    : []),
]);

function isActive(path) {
  if (path === "/dashboard") return route.path === "/dashboard";
  return route.path.startsWith(path);
}

function navigate(path) {
  router.push(path);
  // On mobile, close the sidebar after navigation
  if (window.innerWidth <= 1024) ui.sidebarOpen = false;
}

function confirmLogout() {
  showLogoutDialog.value = true;
}

function handleLogout() {
  auth.logout();
  router.push("/login");
}

const employeeName = computed(() => auth.displayName);
const employeeTitle = computed(() => auth.employeeTitle || "");
</script>

<template>
  <aside class="sidebar" :class="{ collapsed: !ui.sidebarOpen }">
    <!-- Brand -->
    <div class="sidebar__brand">
      <span class="sidebar__logo"><Building /></span>
      <span class="sidebar__brand-name">Northwind</span>
    </div>

    <!-- Navigation -->
    <nav class="sidebar__nav">
      <button
        v-for="item in navItems"
        :key="item.path"
        class="sidebar__item"
        :class="{ active: isActive(item.path) }"
        @click="navigate(item.path)"
      >
        <component :is="item.icon" class="sidebar__icon" />
        <span class="sidebar__label">{{ item.label }}</span>
      </button>
    </nav>

    <!-- Footer: theme + logout + user info -->
    <div class="sidebar__footer">
      <button class="sidebar__item sidebar__theme-btn" @click="ui.toggleDark()">
        <component
          :is="ui.isDark ? SunLight : HalfMoon"
          class="sidebar__icon"
        />
        <span class="sidebar__label">{{
          ui.isDark ? "Light Mode" : "Dark Mode"
        }}</span>
      </button>

      <button class="sidebar__item sidebar__logout-btn" @click="confirmLogout">
        <LogOut class="sidebar__icon" />
        <span class="sidebar__label">Logout</span>
      </button>

      <div class="sidebar__user">
        <div class="sidebar__user-avatar">{{ employeeName.charAt(0) }}</div>
        <div class="sidebar__user-info">
          <div class="sidebar__user-name">{{ employeeName }}</div>
          <div class="sidebar__user-title">{{ employeeTitle }}</div>
        </div>
      </div>
    </div>

    <ConfirmDialog
      v-if="showLogoutDialog"
      title="Confirm Logout"
      message="Are you sure you want to log out?"
      @confirm="handleLogout"
      @cancel="showLogoutDialog = false"
    />
  </aside>
</template>

<style
  lang="scss"
  src="../../assets/styles/Components/AppSidebar.scss"
  scoped
></style>
