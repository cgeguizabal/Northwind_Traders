<script setup>
import { ref, reactive, onMounted, computed, nextTick } from "vue";
import AppModal from "../common/AppModal.vue";
import AppSpinner from "../common/AppSpinner.vue";
import { useToast } from "vue-toastification";
import { getAllShipmentStates } from "../../axiosInstance/shipmentStateService.js";
import { getAllProducts } from "../../axiosInstance/productService.js";
import { Xmark, MapPin, CheckCircle, WarningCircle } from "iconoir-vue/regular";
import { useOrderStore } from "../../stores/orderStore.js";
import { validateAddress } from "../../axiosInstance/geocodingService.js";

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

// ── Map ────────────────────────────────────────────────────────
const mapEl = ref(null);
let mapObj = null;
let pinMarker = null;

// ── Geocode state ──────────────────────────────────────────────
const shipGeocode = reactive({ loading: false, result: null, error: null });
const billGeocode = reactive({ loading: false, result: null, error: null });

function buildAddressString(addr, city, region, postalCode, country) {
  return [addr, city, region, postalCode, country].filter(Boolean).join(", ");
}

function loadGoogleMaps() {
  return new Promise((resolve) => {
    if (window.google?.maps) {
      resolve();
      return;
    }
    const key = import.meta.env.VITE_GOOGLE_MAPS_KEY;
    const script = document.createElement("script");
    script.src = `https://maps.googleapis.com/maps/api/js?key=${key}`;
    script.async = true;
    script.onload = resolve;
    document.head.appendChild(script);
  });
}

async function initMap() {
  await loadGoogleMaps();
  const center = { lat: 48.8566, lng: 2.3522 };
  mapObj = new window.google.maps.Map(mapEl.value, { center, zoom: 4 });

  // If the order already has geocoded coordinates, pan to them
  if (props.order?.shipLatitude && props.order?.shipLongitude) {
    const pos = {
      lat: Number(props.order.shipLatitude),
      lng: Number(props.order.shipLongitude),
    };
    pinMarker = new window.google.maps.Marker({ position: pos, map: mapObj });
    mapObj.panTo(pos);
    mapObj.setZoom(13);
  }

  // Map click → reverse-geocode into ship address fields
  mapObj.addListener("click", async (e) => {
    const lat = e.latLng.lat();
    const lng = e.latLng.lng();
    if (pinMarker) pinMarker.setMap(null);
    pinMarker = new window.google.maps.Marker({
      position: { lat, lng },
      map: mapObj,
    });
    try {
      const geocoder = new window.google.maps.Geocoder();
      geocoder.geocode({ location: { lat, lng } }, (results, status) => {
        if (status === "OK" && results[0]) {
          const comps = results[0].address_components;
          const get = (type) =>
            comps.find((c) => c.types.includes(type))?.long_name || "";
          form.shipAddress = get("route")
            ? `${get("street_number")} ${get("route")}`.trim()
            : results[0].formatted_address;
          form.shipCity = get("locality") || get("administrative_area_level_2");
          form.shipRegion = get("administrative_area_level_1");
          form.shipPostalCode = get("postal_code");
          form.shipCountry = get("country");
        }
      });
    } catch {
      /* non-critical */
    }
  });
}

async function geocodeShipAddress() {
  const address = buildAddressString(
    form.shipAddress,
    form.shipCity,
    form.shipRegion,
    form.shipPostalCode,
    form.shipCountry,
  );
  if (!address.trim()) {
    toast.warning("Enter at least one ship address field first.");
    return;
  }
  shipGeocode.loading = true;
  shipGeocode.result = null;
  shipGeocode.error = null;
  try {
    const { data } = await validateAddress(address);
    shipGeocode.result = data;
    if (mapObj && data.lat && data.lng) {
      const pos = { lat: Number(data.lat), lng: Number(data.lng) };
      if (pinMarker) pinMarker.setMap(null);
      pinMarker = new window.google.maps.Marker({ position: pos, map: mapObj });
      mapObj.panTo(pos);
      mapObj.setZoom(13);
    }
  } catch {
    shipGeocode.error = "Could not validate this address.";
  } finally {
    shipGeocode.loading = false;
  }
}

async function geocodeBillAddress() {
  const address = buildAddressString(
    form.billAddress,
    form.billCity,
    form.billRegion,
    form.billPostalCode,
    form.billCountry,
  );
  if (!address.trim()) {
    toast.warning("Enter at least one bill address field first.");
    return;
  }
  billGeocode.loading = true;
  billGeocode.result = null;
  billGeocode.error = null;
  try {
    const { data } = await validateAddress(address);
    billGeocode.result = data;
  } catch {
    billGeocode.error = "Could not validate this address.";
  } finally {
    billGeocode.loading = false;
  }
}

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
      notes: form.notes || null,
      shipName: form.shipName || null,
      shipAddress: form.shipAddress || null,
      shipCity: form.shipCity || null,
      shipRegion: form.shipRegion || null,
      shipPostalCode: form.shipPostalCode || null,
      shipCountry: form.shipCountry || null,
      billAddress: form.billAddress || null,
      billCity: form.billCity || null,
      billRegion: form.billRegion || null,
      billPostalCode: form.billPostalCode || null,
      billCountry: form.billCountry || null,
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
      getAllProducts(),
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

  // Init map after form data is loaded and DOM is rendered
  await nextTick();
  try {
    await initMap();
  } catch {
    /* Maps API unavailable */
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

      <!-- ── Notes ─────────────────────────────────── -->
      <div class="form-group">
        <label class="form-label">Notes</label>
        <textarea v-model="form.notes" class="form-control" rows="2" />
      </div>

      <!-- ── Ship Address ───────────────────────────── -->
      <div class="section-title">Ship Address</div>
      <div class="form-row">
        <div class="form-group">
          <label class="form-label">Ship Name</label>
          <input v-model="form.shipName" class="form-control" />
        </div>
        <div class="form-group">
          <label class="form-label">Address</label>
          <input v-model="form.shipAddress" class="form-control" />
        </div>
      </div>
      <div class="form-row form-row--4">
        <div class="form-group">
          <label class="form-label">City</label>
          <input v-model="form.shipCity" class="form-control" />
        </div>
        <div class="form-group">
          <label class="form-label">Region</label>
          <input v-model="form.shipRegion" class="form-control" />
        </div>
        <div class="form-group">
          <label class="form-label">Postal Code</label>
          <input v-model="form.shipPostalCode" class="form-control" />
        </div>
        <div class="form-group">
          <label class="form-label">Country</label>
          <input v-model="form.shipCountry" class="form-control" />
        </div>
      </div>

      <!-- Map (click to set address) -->
      <div ref="mapEl" class="order-modal-map" />

      <!-- Geocode validate row -->
      <div class="geocode-row">
        <button
          type="button"
          class="btn btn-secondary btn-sm geocode-btn"
          :disabled="shipGeocode.loading"
          @click="geocodeShipAddress"
        >
          <AppSpinner v-if="shipGeocode.loading" size="sm" />
          <MapPin v-else />
          <span>{{
            shipGeocode.loading ? "Validating…" : "Validate & Geocode"
          }}</span>
        </button>
        <div
          v-if="shipGeocode.result"
          class="geocode-result geocode-result--success"
        >
          <CheckCircle class="geocode-result__icon" />
          <div class="geocode-result__body">
            <span class="geocode-result__address">{{
              shipGeocode.result.validatedAddress
            }}</span>
            <div class="geocode-coords">
              <span class="coord-badge"
                >{{ shipGeocode.result.lat?.toFixed(5) }}°N</span
              >
              <span class="coord-badge"
                >{{ shipGeocode.result.lng?.toFixed(5) }}°E</span
              >
            </div>
          </div>
        </div>
        <div
          v-if="shipGeocode.error"
          class="geocode-result geocode-result--error"
        >
          <WarningCircle class="geocode-result__icon" />
          <span>{{ shipGeocode.error }}</span>
        </div>
      </div>

      <!-- ── Bill Address ───────────────────────────── -->
      <div class="section-title">Bill Address</div>
      <div class="form-row">
        <div class="form-group">
          <label class="form-label">Address</label>
          <input v-model="form.billAddress" class="form-control" />
        </div>
        <div class="form-group">
          <label class="form-label">City</label>
          <input v-model="form.billCity" class="form-control" />
        </div>
      </div>
      <div class="form-row form-row--3">
        <div class="form-group">
          <label class="form-label">Region</label>
          <input v-model="form.billRegion" class="form-control" />
        </div>
        <div class="form-group">
          <label class="form-label">Postal Code</label>
          <input v-model="form.billPostalCode" class="form-control" />
        </div>
        <div class="form-group">
          <label class="form-label">Country</label>
          <input v-model="form.billCountry" class="form-control" />
        </div>
      </div>
      <div class="geocode-row">
        <button
          type="button"
          class="btn btn-secondary btn-sm geocode-btn"
          :disabled="billGeocode.loading"
          @click="geocodeBillAddress"
        >
          <AppSpinner v-if="billGeocode.loading" size="sm" />
          <MapPin v-else />
          <span>{{
            billGeocode.loading ? "Validating…" : "Validate & Geocode"
          }}</span>
        </button>
        <div
          v-if="billGeocode.result"
          class="geocode-result geocode-result--success"
        >
          <CheckCircle class="geocode-result__icon" />
          <div class="geocode-result__body">
            <span class="geocode-result__address">{{
              billGeocode.result.validatedAddress
            }}</span>
            <div class="geocode-coords">
              <span class="coord-badge"
                >{{ billGeocode.result.lat?.toFixed(5) }}°N</span
              >
              <span class="coord-badge"
                >{{ billGeocode.result.lng?.toFixed(5) }}°E</span
              >
            </div>
          </div>
        </div>
        <div
          v-if="billGeocode.error"
          class="geocode-result geocode-result--error"
        >
          <WarningCircle class="geocode-result__icon" />
          <span>{{ billGeocode.error }}</span>
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
