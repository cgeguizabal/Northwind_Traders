<script setup>
import { ref, reactive, onMounted, computed } from "vue";
import AppModal from "../common/AppModal.vue";
import AppSpinner from "../common/AppSpinner.vue";
import { useToast } from "vue-toastification";
import { getAllShipmentStates } from "../../axiosInstance/shipmentStateService.js";
import { getActiveProducts } from "../../axiosInstance/productService.js";
import { Xmark } from "iconoir-vue/regular";
import { useOrderStore } from "../../stores/orderStore.js";

const props = defineProps({
  // Pass existing order for edit mode; null for create mode
  order: { type: Object, default: null },
});

const emit = defineEmits(["close", "saved"]);
const toast = useToast();
const store = useOrderStore();

// ── Dropdown data ──────────────────────────────────────────────
const statuses = ref([]);
const products = ref([]);
const loading = ref(false);
const saving = ref(false);

// ── Form state ─────────────────────────────────────────────────
const form = reactive({
  customerId: "",
  employeeId: "",
  shipVia: "",
  shipmentStateId: "",
  orderDate: "",
  requiredDate: "",
  shippedDate: "",
  freight: 0,
  notes: "",
  shipName: "",
  shipAddress: "",
  shipCity: "",
  shipRegion: "",
  shipPostalCode: "",
  shipCountry: "",
  billAddress: "",
  billCity: "",
  billRegion: "",
  billPostalCode: "",
  billCountry: "",
  lines: [],
});

const errors = reactive({});

// ── Line item state ────────────────────────────────────────────
const newLine = reactive({
  productId: "",
  quantity: 1,
  unitPrice: 0,
  discount: 0,
});

// Product search filter for the line-item dropdown
const productSearch = ref("");
const filteredProducts = computed(() =>
  products.value.filter((p) =>
    p.productName.toLowerCase().includes(productSearch.value.toLowerCase()),
  ),
);

function onProductSelect(e) {
  const id = Number(e.target.value);
  const product = products.value.find((p) => p.productId === id);
  if (product) {
    newLine.productId = id;
    newLine.unitPrice = product.unitPrice || 0;
  }
}

function addLine() {
  if (!newLine.productId || newLine.quantity <= 0) {
    toast.warning("Select a product and enter a valid quantity.");
    return;
  }
  const product = products.value.find((p) => p.productId === newLine.productId);
  form.lines.push({
    productId: newLine.productId,
    productName: product?.productName || "",
    quantity: newLine.quantity,
    unitPrice: newLine.unitPrice,
    discount: newLine.discount,
  });
  // Reset line form
  newLine.productId = "";
  newLine.quantity = 1;
  newLine.unitPrice = 0;
  newLine.discount = 0;
  productSearch.value = "";
}

function removeLine(idx) {
  form.lines.splice(idx, 1);
}

const runningTotal = computed(() =>
  form.lines.reduce(
    (s, l) => s + l.unitPrice * l.quantity * (1 - (l.discount || 0)),
    0,
  ),
);

// ── Validation ─────────────────────────────────────────────────
function validate() {
  Object.keys(errors).forEach((k) => delete errors[k]);
  if (!form.shipmentStateId) errors.shipmentStateId = "Status is required.";
  if (!form.lines.length) errors.lines = "Add at least one product line.";
  return Object.keys(errors).length === 0;
}

// ── Save ───────────────────────────────────────────────────────
async function save() {
  if (!validate()) {
    toast.error("Please fix the form errors.");
    return;
  }

  saving.value = true;
  try {
    const payload = {
      shipmentStateId: Number(form.shipmentStateId),
      shippedDate: form.shippedDate || null,
      freight: Number(form.freight),
      lines: form.lines.map((l) => ({
        productId: l.productId,
        unitPrice: l.unitPrice,
        quantity: l.quantity,
        discount: l.discount,
      })),
    };

    if (props.order) {
      await store.submitUpdateOrder(props.order.orderId, payload);
      toast.success("Order updated successfully.");
    } else {
      await store.submitCreateOrder(payload);
      toast.success("Order created successfully.");
    }
    emit("saved");
  } catch (e) {
    toast.error(e?.response?.data?.message || "Failed to save order.");
  } finally {
    saving.value = false;
  }
}

// ── On mount: load dropdown data; populate form for edit ───────
onMounted(async () => {
  loading.value = true;
  try {
    const [st, p] = await Promise.all([
      getAllShipmentStates(),
      getActiveProducts(),
    ]);
    statuses.value = st.data;
    products.value = p.data;

    // Populate form in edit mode
    if (props.order) {
      // Resolve shipmentStateId: prefer the numeric id, fall back to matching by name
      const resolvedStateId =
        props.order.shipmentStateId ||
        statuses.value.find((s) => s.name === props.order.shipmentStatus)
          ?.shipmentStateId ||
        "";

      Object.assign(form, {
        customerId: props.order.customerId || "",
        employeeId: props.order.employeeId || "",
        shipVia: props.order.shipVia || "",
        shipmentStateId: resolvedStateId,
        orderDate: props.order.orderDate?.split("T")[0] || "",
        requiredDate: props.order.requiredDate?.split("T")[0] || "",
        shippedDate: props.order.shippedDate?.split("T")[0] || "",
        freight: props.order.freight || 0,
        notes: props.order.notes || "",
        shipName: props.order.shipName || "",
        shipAddress: props.order.shipAddress || "",
        shipCity: props.order.shipCity || "",
        shipRegion: props.order.shipRegion || "",
        shipPostalCode: props.order.shipPostalCode || "",
        shipCountry: props.order.shipCountry || "",
        billAddress: props.order.billAddress || "",
        billCity: props.order.billCity || "",
        billRegion: props.order.billRegion || "",
        billPostalCode: props.order.billPostalCode || "",
        billCountry: props.order.billCountry || "",
        lines: (props.order.lines || []).map((l) => ({
          productId: l.productId || 0,
          productName: l.productName || "",
          quantity: l.quantity,
          unitPrice: l.unitPrice,
          discount: l.discount,
        })),
      });
    }
  } catch (_e) {
    toast.error("Failed to load form data.");
  } finally {
    loading.value = false;
  }
});

function formatCurrency(n) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(n);
}
</script>

<template>
  <AppModal
    :title="order ? `Edit Order #${order.orderId}` : 'New Order'"
    width="900px"
    @close="$emit('close')"
  >
    <div v-if="loading" class="form-loading">
      <AppSpinner size="lg" />
    </div>

    <form v-else class="order-form" @submit.prevent="save">
      <!-- ── Status / Shipped Date / Freight ───────── -->
      <div class="form-row form-row--3">
        <div class="form-group">
          <label class="form-label">Status *</label>
          <select v-model="form.shipmentStateId" class="form-control">
            <option value="">Select status...</option>
            <option
              v-for="s in statuses"
              :key="s.shipmentStateId"
              :value="s.shipmentStateId"
            >
              {{ s.name }}
            </option>
          </select>
          <span v-if="errors.shipmentStateId" class="form-error">{{
            errors.shipmentStateId
          }}</span>
        </div>
        <div class="form-group">
          <label class="form-label">Shipped Date</label>
          <input type="date" v-model="form.shippedDate" class="form-control" />
        </div>
        <div class="form-group">
          <label class="form-label">Freight ($)</label>
          <input
            type="number"
            v-model.number="form.freight"
            min="0"
            step="0.01"
            class="form-control"
          />
        </div>
      </div>

      <!-- ── Product Lines ──────────────────────────── -->
      <div class="section-title">Product Lines</div>
      <span v-if="errors.lines" class="form-error">{{ errors.lines }}</span>

      <!-- Add line row -->
      <div class="form-row line-add-row">
        <div class="form-group" style="flex: 2">
          <label class="form-label">Product</label>
          <input
            v-model="productSearch"
            list="product-list"
            class="form-control"
            placeholder="Search product..."
            @change="
              (e) => {
                const match = products.find(
                  (p) => p.productName === e.target.value,
                );
                if (match) {
                  newLine.productId = match.productId;
                  newLine.unitPrice = match.unitPrice || 0;
                }
              }
            "
          />
          <datalist id="product-list">
            <option
              v-for="p in filteredProducts"
              :key="p.productId"
              :value="p.productName"
            />
          </datalist>
        </div>
        <div class="form-group">
          <label class="form-label">Unit Price</label>
          <input
            type="number"
            v-model.number="newLine.unitPrice"
            min="0"
            step="0.01"
            class="form-control"
          />
        </div>
        <div class="form-group">
          <label class="form-label">Qty</label>
          <input
            type="number"
            v-model.number="newLine.quantity"
            min="1"
            class="form-control"
          />
        </div>
        <div class="form-group">
          <label class="form-label">Discount %</label>
          <input
            type="number"
            v-model.number="newLine.discount"
            min="0"
            max="1"
            step="0.01"
            class="form-control"
          />
        </div>
        <div class="form-group form-group--btn">
          <label class="form-label">&nbsp;</label>
          <button type="button" class="btn btn-primary" @click="addLine">
            + Add
          </button>
        </div>
      </div>

      <!-- Lines table -->
      <div class="table-scroll" v-if="form.lines.length">
        <table class="data-table">
          <thead>
            <tr>
              <th>Product</th>
              <th>Price</th>
              <th>Qty</th>
              <th>Disc.</th>
              <th>Subtotal</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="(line, idx) in form.lines" :key="idx">
              <td>{{ line.productName }}</td>
              <td>${{ line.unitPrice.toFixed(2) }}</td>
              <td>{{ line.quantity }}</td>
              <td>{{ ((line.discount || 0) * 100).toFixed(0) }}%</td>
              <td>
                {{
                  formatCurrency(
                    line.unitPrice * line.quantity * (1 - (line.discount || 0)),
                  )
                }}
              </td>
              <td>
                <button
                  type="button"
                  class="btn btn-ghost btn-sm"
                  @click="removeLine(idx)"
                >
                  <Xmark />
                </button>
              </td>
            </tr>
          </tbody>
          <tfoot>
            <tr>
              <td colspan="4" class="total-label">Total</td>
              <td class="total-value">{{ formatCurrency(runningTotal) }}</td>
              <td></td>
            </tr>
          </tfoot>
        </table>
      </div>
    </form>

    <template #footer>
      <button class="btn btn-secondary" @click="$emit('close')">Cancel</button>
      <button class="btn btn-primary" :disabled="saving" @click="save">
        <AppSpinner v-if="saving" size="sm" />
        <span v-else>{{ order ? "Save Changes" : "Create Order" }}</span>
      </button>
    </template>
  </AppModal>
</template>

<style
  lang="scss"
  src="../../assets/styles/Components/OrderFormModal.scss"
  scoped
></style>
