<script setup>
import { onMounted, computed, ref, watch } from "vue";
import { useRouter } from "vue-router";
import AppLayout from "../components/layout/AppLayout.vue";
import AppSpinner from "../components/common/AppSpinner.vue";
import DonutChart from "../components/charts/DonutChart.vue";
import BarChart from "../components/charts/BarChart.vue";
import { useDashboardStore } from "../stores/dashboardStore.js";
import { useToast } from "vue-toastification";
import { Package, CreditCard, Clock, Group, User } from "iconoir-vue/regular";

const store = useDashboardStore();
const router = useRouter();
const toast = useToast();

// Date range filter state
const dateFrom = ref("");
const dateTo = ref("");

function buildParams() {
  const p = {};
  if (dateFrom.value) p.dateFrom = dateFrom.value;
  if (dateTo.value) p.dateTo = dateTo.value;
  return p;
}

onMounted(async () => {
  try {
    await store.fetchStats();
  } catch {
    toast.error("Failed to load dashboard data.");
  }
});

watch([dateFrom, dateTo], async () => {
  try {
    await store.fetchStats(buildParams());
  } catch {
    toast.error("Failed to load dashboard data.");
  }
});

// ── Stat cards ─────────────────────────────────────────────────
const statCards = computed(() => {
  const s = store.stats;
  if (!s) return [];

  // Derive Pending Shipments from ordersByStatus list
  const pendingCount =
    s.ordersByStatus?.find((x) => x.status?.toLowerCase().includes("pend"))
      ?.count ?? 0;

  return [
    {
      label: "Total Orders",
      icon: Package,
      value: s.totalOrders ?? 0,
      route: "/orders",
    },
    {
      label: "Total Revenue",
      icon: CreditCard,
      value: formatCurrency(s.totalRevenue ?? 0),
      route: "/orders",
    },
    {
      label: "Pending Shipments",
      icon: Clock,
      value: pendingCount,
      route: "/orders",
    },
    {
      label: "Total Customers",
      icon: Group,
      value: s.totalCustomers ?? 0,
      route: "/customers",
    },
    {
      label: "Total Employees",
      icon: User,
      value: s.totalEmployees ?? 0,
      route: "/employees",
    },
  ];
});

// ── Chart data derived from dashboard stats ────────────────────
// Donut: orders by shipment status
const regionLabels = computed(() =>
  (store.stats?.ordersByStatus || []).map((x) => x.status),
);
const regionData = computed(() =>
  (store.stats?.ordersByStatus || []).map((x) => x.count),
);

// Bar: top 5 customers
const countryLabels = computed(() =>
  (store.stats?.topCustomers || []).map((x) => x.companyName),
);
const countryData = computed(() =>
  (store.stats?.topCustomers || []).map((x) => x.orderCount),
);

// Bar: top 5 employees
const revenueLabels = computed(() =>
  (store.stats?.topEmployees || []).map((x) => x.fullName),
);
const revenueData = computed(() =>
  (store.stats?.topEmployees || []).map((x) => x.orderCount),
);

function formatCurrency(n) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    maximumFractionDigits: 0,
  }).format(n);
}
</script>

<template>
  <AppLayout>
    <div class="page-container dashboard">
      <!-- Loading -->
      <div v-if="store.loading" class="dashboard__loading">
        <AppSpinner size="lg" />
      </div>

      <template v-else-if="store.stats">
        <!-- Date range filter -->
        <div class="dashboard__filter">
          <div class="form-group">
            <label class="form-label">From</label>
            <input type="date" v-model="dateFrom" class="form-control" />
          </div>
          <div class="form-group">
            <label class="form-label">To</label>
            <input type="date" v-model="dateTo" class="form-control" />
          </div>
        </div>

        <!-- Stat cards -->
        <div class="dashboard__stats">
          <div
            v-for="card in statCards"
            :key="card.label"
            class="stat-card"
            @click="router.push(card.route)"
          >
            <component :is="card.icon" class="stat-icon" />
            <div class="stat-label">{{ card.label }}</div>
            <div class="stat-value">{{ card.value }}</div>
          </div>
        </div>

        <!-- Charts -->
        <div class="dashboard__charts">
          <DonutChart
            v-if="regionLabels.length"
            :labels="regionLabels"
            :data="regionData"
            title="Orders by Status"
          />
          <BarChart
            v-if="countryLabels.length"
            :labels="countryLabels"
            :data="countryData"
            label="Orders"
            title="Top Customers"
          />
          <BarChart
            v-if="revenueLabels.length"
            :labels="revenueLabels"
            :data="revenueData"
            label="Orders handled"
            title="Top Employees"
            color="rgba(245,158,11,0.75)"
          />
        </div>
      </template>

      <p v-else-if="store.error" class="dashboard__error">
        Failed to load dashboard.
      </p>
    </div>
  </AppLayout>
</template>

<style
  lang="scss"
  src="../assets/styles/Components/DashboardView.scss"
  scoped
></style>
