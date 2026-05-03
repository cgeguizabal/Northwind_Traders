<script setup>
import AppBadge from "../common/AppBadge.vue";

defineProps({
  orders: { type: Array, required: true },
});

const emit = defineEmits(["row-click"]);

// Map shipment status names to badge variants
function statusVariant(statusName) {
  const s = (statusName || "").toLowerCase();
  if (s.includes("pending")) return "gold";
  if (s.includes("ship")) return "green";
  if (s.includes("cancel")) return "danger";
  if (s.includes("deliver")) return "purple";
  return "default";
}

function formatDate(d) {
  if (!d) return "—";
  return new Date(d).toLocaleDateString("en-US", {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

function formatCurrency(n) {
  if (n == null) return "—";
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(n);
}
</script>

<template>
  <div class="table-scroll">
    <table class="data-table">
      <thead>
        <tr>
          <th>#</th>
          <th>Customer</th>
          <th>Employee</th>
          <th>Order Date</th>
          <th>Ship Country</th>
          <th>Region</th>
          <th>Freight</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="order in orders"
          :key="order.orderId"
          @click="$emit('row-click', order)"
        >
          <td>{{ order.orderId }}</td>
          <td>{{ order.customerName || order.customerId }}</td>
          <td>{{ order.employeeName || order.employeeId }}</td>
          <td>{{ formatDate(order.orderDate) }}</td>
          <td>{{ order.shipCountry || "—" }}</td>
          <td>{{ order.shipRegion || "—" }}</td>
          <td>{{ formatCurrency(order.freight) }}</td>
          <td>
            <AppBadge
              :label="order.shipmentStatus || 'Unknown'"
              :variant="statusVariant(order.shipmentStatus)"
            />
          </td>
        </tr>
        <tr v-if="!orders.length">
          <td colspan="8" class="table-empty">No orders found.</td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style
  lang="scss"
  src="../../assets/styles/Components/OrderTable.scss"
  scoped
></style>
