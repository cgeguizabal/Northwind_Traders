<script setup>
import { ref, computed } from "vue";
import AppModal from "../common/AppModal.vue";
import AppBadge from "../common/AppBadge.vue";
import AppSpinner from "../common/AppSpinner.vue";
import OrderMap from "./OrderMap.vue";
import { getOrderPdf } from "../../axiosInstance/orderService.js";
import { useToast } from "vue-toastification";
import { Page, EditPencil } from "iconoir-vue/regular";

const props = defineProps({
  order: { type: Object, required: true },
});

const emit = defineEmits(["close", "edit"]);
const toast = useToast();
const pdfLoading = ref(false);

async function downloadPdf() {
  pdfLoading.value = true;
  try {
    const { data } = await getOrderPdf(props.order.orderId);
    const url = URL.createObjectURL(
      new Blob([data], { type: "application/pdf" }),
    );
    const link = document.createElement("a");
    link.href = url;
    link.download = `order-${props.order.orderId}.pdf`;
    link.click();
    URL.revokeObjectURL(url);
  } catch {
    toast.error("Failed to download PDF.");
  } finally {
    pdfLoading.value = false;
  }
}

function statusVariant(name) {
  const s = (name || "").toLowerCase();
  if (s.includes("pending")) return "gold";
  if (s.includes("ship")) return "green";
  if (s.includes("cancel")) return "danger";
  if (s.includes("deliver")) return "purple";
  return "default";
}

function formatDate(d) {
  return d ? new Date(d).toLocaleDateString() : "—";
}
function formatCurrency(n) {
  return n != null
    ? new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD",
      }).format(n)
    : "—";
}

const mapLat = computed(() =>
  props.order.shipLatitude ? Number(props.order.shipLatitude) : null,
);
const mapLng = computed(() =>
  props.order.shipLongitude ? Number(props.order.shipLongitude) : null,
);

const total = computed(() => {
  if (!props.order.lines?.length) return 0;
  return props.order.lines.reduce(
    (sum, l) => sum + l.unitPrice * l.quantity * (1 - (l.discount || 0)),
    0,
  );
});
</script>

<template>
  <AppModal
    :title="`Order #${order.orderId}`"
    width="800px"
    @close="$emit('close')"
  >
    <!-- Status badge -->
    <div class="detail-header">
      <AppBadge
        :label="order.shipmentStatus || 'Unknown'"
        :variant="statusVariant(order.shipmentStatus)"
      />
    </div>

    <!-- Two-column info grid -->
    <div class="detail-grid">
      <div class="detail-section">
        <h3 class="detail-section-title">Order Info</h3>
        <dl class="detail-dl">
          <div>
            <dt>Customer</dt>
            <dd>{{ order.customerName || order.customerId }}</dd>
          </div>
          <div>
            <dt>Employee</dt>
            <dd>{{ order.employeeName || order.employeeId }}</dd>
          </div>
          <div>
            <dt>Shipper</dt>
            <dd>{{ order.shipperName || order.shipVia }}</dd>
          </div>
          <div>
            <dt>Order Date</dt>
            <dd>{{ formatDate(order.orderDate) }}</dd>
          </div>
          <div>
            <dt>Required Date</dt>
            <dd>{{ formatDate(order.requiredDate) }}</dd>
          </div>
          <div>
            <dt>Shipped Date</dt>
            <dd>{{ formatDate(order.shippedDate) }}</dd>
          </div>
          <div>
            <dt>Freight</dt>
            <dd>{{ formatCurrency(order.freight) }}</dd>
          </div>
          <div v-if="order.notes">
            <dt>Notes</dt>
            <dd>{{ order.notes }}</dd>
          </div>
        </dl>
      </div>

      <div class="detail-section">
        <h3 class="detail-section-title">Ship Address</h3>
        <dl class="detail-dl">
          <div>
            <dt>Name</dt>
            <dd>{{ order.shipName }}</dd>
          </div>
          <div>
            <dt>Address</dt>
            <dd>{{ order.shipAddress }}</dd>
          </div>
          <div>
            <dt>City</dt>
            <dd>{{ order.shipCity }}</dd>
          </div>
          <div>
            <dt>Region</dt>
            <dd>{{ order.shipRegion || "—" }}</dd>
          </div>
          <div>
            <dt>Postal</dt>
            <dd>{{ order.shipPostalCode }}</dd>
          </div>
          <div>
            <dt>Country</dt>
            <dd>{{ order.shipCountry }}</dd>
          </div>
        </dl>
      </div>
    </div>

    <!-- Map -->
    <OrderMap :lat="mapLat" :lng="mapLng" style="margin-top: 16px" />

    <!-- Line items -->
    <h3 class="detail-section-title" style="margin-top: 20px">Line Items</h3>
    <div class="table-scroll">
      <table class="data-table">
        <thead>
          <tr>
            <th>Product</th>
            <th>Unit Price</th>
            <th>Qty</th>
            <th>Discount</th>
            <th>Subtotal</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(line, i) in order.lines" :key="i">
            <td>{{ line.productName }}</td>
            <td>{{ formatCurrency(line.unitPrice) }}</td>
            <td>{{ line.quantity }}</td>
            <td>{{ ((line.discount || 0) * 100).toFixed(0) }}%</td>
            <td>
              {{
                formatCurrency(
                  line.lineTotal ??
                    line.unitPrice * line.quantity * (1 - (line.discount || 0)),
                )
              }}
            </td>
          </tr>
          <tr v-if="!order.lines?.length">
            <td
              colspan="5"
              style="
                text-align: center;
                color: var(--text-muted);
                padding: 16px;
              "
            >
              No line items.
            </td>
          </tr>
        </tbody>
        <tfoot>
          <tr>
            <td colspan="4" class="total-label">Order Total</td>
            <td class="total-value">{{ formatCurrency(total) }}</td>
          </tr>
        </tfoot>
      </table>
    </div>

    <template #footer>
      <button
        class="btn btn-secondary"
        :disabled="pdfLoading"
        @click="downloadPdf"
      >
        <AppSpinner v-if="pdfLoading" size="sm" />
        <span v-else><Page /> Download PDF</span>
      </button>
      <button class="btn btn-primary" @click="$emit('edit', order)">
        <EditPencil /> Edit Order
      </button>
    </template>
  </AppModal>
</template>

<style
  lang="scss"
  src="../../assets/styles/Components/OrderDetailModal.scss"
  scoped
></style>
