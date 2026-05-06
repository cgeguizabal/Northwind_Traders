<script setup>
import { ref, computed, onMounted } from "vue";
import { useRouter } from "vue-router";
import AppLayout from "../components/layout/AppLayout.vue";
import AppSpinner from "../components/common/AppSpinner.vue";
import { useCustomerStore } from "../stores/customerStore.js";
import { useToast } from "vue-toastification";

const store = useCustomerStore();
const router = useRouter();
const toast = useToast();
const search = ref("");

onMounted(async () => {
  try {
    await store.fetchCustomers();
  } catch {
    toast.error("Failed to load customers.");
  }
});

const filtered = computed(() => {
  const q = search.value.toLowerCase();
  return store.customers.filter(
    (c) =>
      (c.companyName || "").toLowerCase().includes(q) ||
      (c.contactName || "").toLowerCase().includes(q) ||
      (c.city || "").toLowerCase().includes(q) ||
      (c.country || "").toLowerCase().includes(q),
  );
});
</script>

<template>
  <AppLayout>
    <div class="page-container">
      <div class="page-header">
        <h1>Customers</h1>
        <input
          v-model="search"
          class="form-control search-input"
          placeholder="Search by name, city, country..."
        />
      </div>

      <div v-if="store.loading" class="spinner-center">
        <AppSpinner size="lg" />
      </div>

      <template v-else>
        <!-- Desktop: table -->
        <div class="table-scroll glass customers-table" style="padding: 0">
          <table class="data-table">
            <thead>
              <tr>
                <th>Company Name</th>
                <th>Contact</th>
                <th>City</th>
                <th>Country</th>
                <th>Phone</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="c in filtered"
                :key="c.customerId"
                @click="router.push(`/customers/${c.customerId}`)"
              >
                <td>{{ c.companyName }}</td>
                <td>{{ c.contactName }}</td>
                <td>{{ c.city }}</td>
                <td>{{ c.country }}</td>
                <td>{{ c.phone }}</td>
              </tr>
              <tr v-if="!filtered.length">
                <td
                  colspan="5"
                  style="
                    text-align: center;
                    padding: 32px;
                    color: var(--text-muted);
                  "
                >
                  No customers found.
                </td>
              </tr>
            </tbody>
          </table>
        </div>

        <!-- Mobile: cards -->
        <div class="customers-cards">
          <div
            v-for="c in filtered"
            :key="c.customerId"
            class="customer-card glass"
            @click="router.push(`/customers/${c.customerId}`)"
          >
            <div class="customer-card__name">{{ c.companyName }}</div>
            <div class="customer-card__contact">{{ c.contactName }}</div>
            <div class="customer-card__meta">
              <span v-if="c.city || c.country">{{
                [c.city, c.country].filter(Boolean).join(", ")
              }}</span>
              <span v-if="c.phone" class="customer-card__phone">{{
                c.phone
              }}</span>
            </div>
          </div>
          <p v-if="!filtered.length" class="customers-empty">
            No customers found.
          </p>
        </div>
      </template>
    </div>
  </AppLayout>
</template>

<style
  lang="scss"
  src="../assets/styles/Components/CustomersView.scss"
  scoped
></style>
