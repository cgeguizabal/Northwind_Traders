<script setup>
import { ref, onMounted } from "vue";
import { useRoute, useRouter } from "vue-router";
import AppLayout from "../components/layout/AppLayout.vue";
import AppSpinner from "../components/common/AppSpinner.vue";
import OrderTable from "../components/orders/OrderTable.vue";
import OrderDetailModal from "../components/orders/OrderDetailModal.vue";
import { useCustomerStore } from "../stores/customerStore.js";
import { useToast } from "vue-toastification";
import { getOrderById } from "../axiosInstance/orderService.js";
import { Phone } from "iconoir-vue/regular";

const route = useRoute();
const router = useRouter();
const store = useCustomerStore();
const toast = useToast();

const detailOrder = ref(null);
const showDetail = ref(false);
const detailLoading = ref(false);

onMounted(async () => {
  try {
    await store.fetchCustomer(route.params.id);
  } catch {
    toast.error("Failed to load customer data.");
  }
});

async function openOrder(order) {
  detailLoading.value = true;
  try {
    const { data } = await getOrderById(order.orderId);
    detailOrder.value = data;
    showDetail.value = true;
  } catch {
    toast.error("Failed to load order.");
  } finally {
    detailLoading.value = false;
  }
}
</script>

<template>
  <AppLayout>
    <div class="page-container">
      <button
        class="btn btn-secondary btn-sm back-btn"
        @click="router.push('/customers')"
      >
        ← Customers
      </button>

      <div v-if="store.loading" class="spinner-center">
        <AppSpinner size="lg" />
      </div>

      <template v-else-if="store.current">
        <!-- Customer info card -->
        <div class="customer-header glass">
          <div class="customer-header__info">
            <h2>{{ store.current.companyName }}</h2>
            <p>
              {{ store.current.contactName }} · {{ store.current.contactTitle }}
            </p>
            <p>
              {{ store.current.address }}, {{ store.current.city }},
              {{ store.current.country }}
            </p>
            <p>
              Phone:
              {{ store.current.phone }}
            </p>
          </div>
          <div class="customer-header__id">
            <span class="badge badge-purple">{{
              store.current.customerId
            }}</span>
          </div>
        </div>

        <!-- Orders -->
        <h3 class="section-title">Orders</h3>
        <OrderTable
          :orders="store.current.orders || []"
          @row-click="openOrder"
        />
      </template>

      <!-- Order detail modal -->
      <OrderDetailModal
        v-if="showDetail && detailOrder"
        :order="detailOrder"
        @close="
          showDetail = false;
          detailOrder = null;
        "
        @edit="() => {}"
      />
    </div>
  </AppLayout>
</template>

<style
  lang="scss"
  src="../assets/styles/Components/CustomerDetailView.scss"
  scoped
></style>
