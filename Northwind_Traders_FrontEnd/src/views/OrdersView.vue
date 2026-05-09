<script setup>
import { ref, computed, onMounted } from "vue";
import AppLayout from "../components/layout/AppLayout.vue";
import AppSpinner from "../components/common/AppSpinner.vue";
import AppModal from "../components/common/AppModal.vue";
import OrderTable from "../components/orders/OrderTable.vue";
import OrderDetailModal from "../components/orders/OrderDetailModal.vue";
import OrderFormModal from "../components/orders/OrderFormModal.vue";
import BarChart from "../components/charts/BarChart.vue";
import DonutChart from "../components/charts/DonutChart.vue";
import { useOrderStore } from "../stores/orderStore.js";
import { useToast } from "vue-toastification";
import { useRouter } from "vue-router";
import { StatsReport, Plus, Printer, Page } from "iconoir-vue/regular";

const store = useOrderStore();
const toast = useToast();
const router = useRouter();

// ── Tabs ───────────────────────────────────────────────────────
const activeTab = ref("orders"); // 'orders' | 'reports'

// ── Filters ────────────────────────────────────────────────────
const filterYear = ref("");
const filterMonth = ref("");
const filterWeek = ref("");
const filterRegion = ref("");

const YEARS = computed(() => {
  const years = new Set(
    store.orders
      .map((o) => o.orderDate && new Date(o.orderDate).getFullYear())
      .filter(Boolean),
  );
  return [...years].sort((a, b) => b - a);
});
const MONTHS = [
  { v: 1, l: "January" },
  { v: 2, l: "February" },
  { v: 3, l: "March" },
  { v: 4, l: "April" },
  { v: 5, l: "May" },
  { v: 6, l: "June" },
  { v: 7, l: "July" },
  { v: 8, l: "August" },
  { v: 9, l: "September" },
  { v: 10, l: "October" },
  { v: 11, l: "November" },
  { v: 12, l: "December" },
];

// ── Filtered orders ────────────────────────────────────────────
const filteredOrders = computed(() => {
  let list = store.orders;
  if (filterYear.value)
    list = list.filter(
      (o) => new Date(o.orderDate).getFullYear() === Number(filterYear.value),
    );
  if (filterMonth.value)
    list = list.filter(
      (o) => new Date(o.orderDate).getMonth() + 1 === Number(filterMonth.value),
    );
  if (filterRegion.value)
    list = list.filter(
      (o) =>
        (o.shipRegion || "").toLowerCase() === filterRegion.value.toLowerCase(),
    );
  return list;
});

const regions = computed(() => [
  ...new Set(store.orders.map((o) => o.shipRegion).filter(Boolean)),
]);

// ── Chart data from filtered orders ───────────────────────────
function groupBy(arr, key) {
  return arr.reduce((acc, item) => {
    const k = item[key] || "Unknown";
    acc[k] = (acc[k] || 0) + 1;
    return acc;
  }, {});
}

function sumBy(arr, keyGroup, keyVal) {
  return arr.reduce((acc, item) => {
    const k = item[keyGroup] || "Unknown";
    acc[k] = (acc[k] || 0) + (item[keyVal] || 0);
    return acc;
  }, {});
}

const countryOrderMap = computed(() => groupBy(store.orders, "shipCountry"));
const countryLabels = computed(() => Object.keys(countryOrderMap.value));
const countryData = computed(() => Object.values(countryOrderMap.value));

const revenueMap = computed(() =>
  sumBy(store.orders, "shipCountry", "freight"),
);
const revenueLabels = computed(() => Object.keys(revenueMap.value));
const revenueData = computed(() => Object.values(revenueMap.value));

const regionMap = computed(() => groupBy(store.orders, "shipRegion"));
const regionLabels = computed(() => Object.keys(regionMap.value));
const regionData = computed(() => Object.values(regionMap.value));

// ── Order detail modal ─────────────────────────────────────────
const showDetail = ref(false);

async function openDetail(order) {
  try {
    await store.fetchOrder(order.orderId);
    showDetail.value = true;
  } catch {
    toast.error("Failed to load order detail.");
  }
}

function closeDetail() {
  showDetail.value = false;
}

// ── Edit modal ─────────────────────────────────────────────────
const editOrder = ref(null);
const showEdit = ref(false);

function openEdit(order) {
  editOrder.value = order;
  showEdit.value = true;
  showDetail.value = false;
}

function closeEdit() {
  showEdit.value = false;
  editOrder.value = null;
}

async function onSaved() {
  closeEdit();
  await store.fetchOrders();
}

// ── Excel export ───────────────────────────────────────────────
async function exportExcel() {
  try {
    await store.downloadExcel();
  } catch {
    toast.error("Failed to export Excel.");
  }
}

// ── PDF export (all orders) ────────────────────────────────────
async function exportPdf() {
  try {
    await store.downloadPdf();
  } catch {
    toast.error("Failed to export PDF.");
  }
}

// ── Print/PDF export (Reports tab) ────────────────────────────
function printReport() {
  window.print();
}

// ── Soft delete (deactivate) ───────────────────────────────────
const showDeleteConfirm = ref(false);
const orderToDelete = ref(null);

function promptDelete(order) {
  orderToDelete.value = order;
  showDeleteConfirm.value = true;
}

async function confirmDelete() {
  try {
    await store.softDeleteOrder(orderToDelete.value.orderId);
    toast.success(`Order #${orderToDelete.value.orderId} deleted.`);
  } catch {
    toast.error("Failed to delete order.");
  } finally {
    showDeleteConfirm.value = false;
    orderToDelete.value = null;
  }
}

function cancelDelete() {
  showDeleteConfirm.value = false;
  orderToDelete.value = null;
}

onMounted(async () => {
  try {
    await store.fetchOrders();
  } catch {
    toast.error("Failed to load orders.");
  }
});
</script>

<template>
  <AppLayout>
    <div class="page-container orders-view">
      <div class="page-header">
        <h1>Orders</h1>
        <div class="header-actions">
          <button class="btn btn-secondary btn-sm" @click="exportExcel">
            <StatsReport /> Export Excel
          </button>
          <button class="btn btn-secondary btn-sm" @click="exportPdf">
            <Page /> Export PDF
          </button>
          <button
            class="btn btn-primary btn-sm"
            @click="$router.push('/new-order')"
          >
            <Plus /> New Order
          </button>
        </div>
      </div>

      <!-- Tabs -->
      <div class="tabs">
        <button
          class="tab"
          :class="{ active: activeTab === 'orders' }"
          @click="activeTab = 'orders'"
        >
          Orders
        </button>
        <button
          class="tab"
          :class="{ active: activeTab === 'reports' }"
          @click="activeTab = 'reports'"
        >
          Reports
        </button>
      </div>

      <!-- Shared toolbar -->
      <div v-if="activeTab === 'orders'" class="toolbar">
        <input
          v-model="filterYear"
          list="year-list"
          class="form-control toolbar__select"
          placeholder="All Years"
        />
        <datalist id="year-list">
          <option v-for="y in YEARS" :key="y" :value="y" />
        </datalist>
        <select v-model="filterMonth" class="form-control toolbar__select">
          <option value="">All Months</option>
          <option v-for="m in MONTHS" :key="m.v" :value="m.v">{{ m.l }}</option>
        </select>
        <select
          v-if="activeTab === 'orders'"
          v-model="filterRegion"
          class="form-control toolbar__select"
        >
          <option value="">All Regions</option>
          <option v-for="r in regions" :key="r" :value="r">{{ r }}</option>
        </select>
      </div>

      <!-- Loading -->
      <div v-if="store.loading" class="orders-view__loading">
        <AppSpinner size="lg" />
      </div>

      <!-- Tab: Orders Table -->
      <template v-else-if="activeTab === 'orders'">
        <OrderTable
          :orders="filteredOrders"
          @row-click="openDetail"
          @deactivate="promptDelete"
        />
      </template>

      <!-- Tab: Reports -->
      <template v-else>
        <div class="reports-charts">
          <BarChart
            :labels="countryLabels"
            :data="countryData"
            label="Orders"
            title="Orders per Country"
          />
          <BarChart
            :labels="revenueLabels"
            :data="revenueData"
            label="Freight ($)"
            title="Revenue by Country"
            color="rgba(245,158,11,0.75)"
          />
          <DonutChart
            :labels="regionLabels"
            :data="regionData"
            title="Shipments by Region"
          />
        </div>
      </template>

      <!-- Order Detail Modal -->
      <OrderDetailModal
        v-if="showDetail && store.current"
        :order="store.current"
        @close="closeDetail"
        @edit="openEdit"
      />

      <!-- Edit Order Modal -->
      <OrderFormModal
        v-if="showEdit && editOrder"
        :order="editOrder"
        @close="closeEdit"
        @saved="onSaved"
      />

      <!-- Delete Confirmation Modal -->
      <AppModal
        v-if="showDeleteConfirm"
        title="Delete Order"
        width="420px"
        @close="cancelDelete"
      >
        <p>Are you sure you want to delete this item?</p>
        <template #footer>
          <button class="btn btn-secondary" @click="cancelDelete">
            Cancel
          </button>
          <button class="btn btn-danger" @click="confirmDelete">
            Yes, delete
          </button>
        </template>
      </AppModal>
    </div>
  </AppLayout>
</template>

<style
  lang="scss"
  src="../assets/styles/Components/OrdersView.scss"
  scoped
></style>
