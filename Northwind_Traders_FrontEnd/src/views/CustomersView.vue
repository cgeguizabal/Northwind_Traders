<script setup>
import { ref, watch, onMounted } from "vue";
import { useRouter } from "vue-router";
import AppLayout from "../components/layout/AppLayout.vue";
import AppSpinner from "../components/common/AppSpinner.vue";
import { useCustomerStore } from "../stores/customerStore.js";
import { useToast } from "vue-toastification";

const store = useCustomerStore();
const router = useRouter();
const toast = useToast();
const search = ref("");

async function load(page = 1) {
  try {
    await store.fetchCustomers(page, search.value);
  } catch {
    toast.error("Failed to load customers.");
  }
}

onMounted(() => load(1));

// Debounce search — reset to page 1 on new search
let debounceTimer;
watch(search, () => {
  clearTimeout(debounceTimer);
  debounceTimer = setTimeout(() => load(1), 350);
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
                v-for="c in store.customers"
                :key="c.customerId"
                @click="router.push(`/customers/${c.customerId}`)"
              >
                <td>{{ c.companyName }}</td>
                <td>{{ c.contactName }}</td>
                <td>{{ c.city }}</td>
                <td>{{ c.country }}</td>
                <td>{{ c.phone }}</td>
              </tr>
              <tr v-if="!store.customers.length">
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
            v-for="c in store.customers"
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
          <p v-if="!store.customers.length" class="customers-empty">
            No customers found.
          </p>
        </div>

        <!-- Pagination -->
        <div v-if="store.totalPages > 1" class="pagination">
          <button
            class="btn btn-secondary btn-sm"
            :disabled="store.page <= 1"
            @click="load(store.page - 1)"
          >
            ‹ Prev
          </button>
          <span class="pagination__info">
            Page {{ store.page }} of {{ store.totalPages }}
            <span class="pagination__count">({{ store.totalCount }} customers)</span>
          </span>
          <button
            class="btn btn-secondary btn-sm"
            :disabled="store.page >= store.totalPages"
            @click="load(store.page + 1)"
          >
            Next ›
          </button>
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
