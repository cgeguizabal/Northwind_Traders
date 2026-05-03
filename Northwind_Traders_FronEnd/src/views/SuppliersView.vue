<script setup>
import { ref, onMounted } from "vue";
import AppLayout from "../components/layout/AppLayout.vue";
import AppSpinner from "../components/common/AppSpinner.vue";
import { getAllSuppliers } from "../axiosInstance/supplierService.js";
import { useToast } from "vue-toastification";

const suppliers = ref([]);
const loading = ref(false);
const toast = useToast();

onMounted(async () => {
  loading.value = true;
  try {
    const { data } = await getAllSuppliers();
    suppliers.value = data;
  } catch {
    toast.error("Failed to load suppliers.");
  } finally {
    loading.value = false;
  }
});
</script>

<template>
  <AppLayout>
    <div class="page-container">
      <div class="page-header"><h1>Suppliers</h1></div>

      <div v-if="loading" class="spinner-center"><AppSpinner size="lg" /></div>

      <div v-else class="table-scroll glass" style="padding: 0">
        <table class="data-table">
          <thead>
            <tr>
              <th>Company</th>
              <th>Contact</th>
              <th>City</th>
              <th>Country</th>
              <th>Phone</th>
              <th>Products</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="s in suppliers" :key="s.supplierId">
              <td>{{ s.companyName }}</td>
              <td>{{ s.contactName }}</td>
              <td>{{ s.city }}</td>
              <td>{{ s.country }}</td>
              <td>{{ s.phone }}</td>
              <td>{{ s.productCount ?? s.products?.length ?? "—" }}</td>
            </tr>
            <tr v-if="!suppliers.length">
              <td
                colspan="6"
                style="
                  text-align: center;
                  padding: 32px;
                  color: var(--text-muted);
                "
              >
                No suppliers.
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
  src="../assets/styles/Components/SuppliersView.scss"
  scoped
></style>
