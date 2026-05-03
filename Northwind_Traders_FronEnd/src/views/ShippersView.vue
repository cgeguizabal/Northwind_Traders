<script setup>
import { ref, onMounted } from "vue";
import AppLayout from "../components/layout/AppLayout.vue";
import AppSpinner from "../components/common/AppSpinner.vue";
import { getAllShippers } from "../axiosInstance/shipperService.js";
import { useToast } from "vue-toastification";

const shippers = ref([]);
const loading = ref(false);
const toast = useToast();

onMounted(async () => {
  loading.value = true;
  try {
    const { data } = await getAllShippers();
    shippers.value = data;
  } catch {
    toast.error("Failed to load shippers.");
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <AppLayout>
    <div class="page-container">
      <div class="page-header"><h1>Shippers</h1></div>

      <div v-if="loading" class="spinner-center"><AppSpinner size="lg" /></div>

      <div v-else class="table-scroll glass" style="padding: 0">
        <table class="data-table">
          <thead>
            <tr>
              <th>Company</th>
              <th>Phone</th>
              <th>Total Orders</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="s in shippers" :key="s.shipperId">
              <td>{{ s.companyName }}</td>
              <td>{{ s.phone }}</td>
              <td>{{ s.orderCount ?? s.orders?.length ?? "—" }}</td>
            </tr>
            <tr v-if="!shippers.length">
              <td
                colspan="3"
                style="
                  text-align: center;
                  padding: 32px;
                  color: var(--text-muted);
                "
              >
                No shippers.
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </AppLayout>
</template>

<style
  lang="scss"
  src="../assets/styles/Components/ShippersView.scss"
  scoped
></style>
