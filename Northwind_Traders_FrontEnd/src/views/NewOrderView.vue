<script setup>
import { ref, reactive, onMounted, computed, nextTick } from "vue";
import { useRouter } from "vue-router";
import AppLayout from "../components/layout/AppLayout.vue";
import AppSpinner from "../components/common/AppSpinner.vue";
import { useToast } from "vue-toastification";
import { getAllCustomers } from "../axiosInstance/customerService.js";
import { getAllShippers } from "../axiosInstance/shipperService.js";
import { getAllShipmentStates } from "../axiosInstance/shipmentStateService.js";
import { useOrderStore } from "../stores/orderStore.js";
import { useEmployeeStore } from "../stores/employeeStore.js";
import { useProductStore } from "../stores/productStore.js";
import { validateAddress } from "../axiosInstance/geocodingService.js";
import { Xmark, MapPin, CheckCircle, WarningCircle } from "iconoir-vue/regular";

const router = useRouter();
const toast = useToast();
const store = useOrderStore();
const employeeStore = useEmployeeStore();
const productStore = useProductStore();

// ── Dropdown data ─────────────────────────────────────────────
const shippers = ref([]);
const statuses = ref([]);
const loading = ref(false);
const saving = ref(false);

// ── Geocode state ──────────────────────────────────────────────
const shipGeocode = reactive({ loading: false, result: null, error: null });
const billGeocode = reactive({ loading: false, result: null, error: null });

function buildAddressString(addr, city, region, postalCode, country) {
  return [addr, city, region, postalCode, country].filter(Boolean).join(", ");
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
    // Move map pin to geocoded location
    if (mapObj && data.lat && data.lng) {
      const pos = { lat: Number(data.lat), lng: Number(data.lng) };
      if (pinMarker) pinMarker.setMap(null);
      pinMarker = new window.google.maps.Marker({ position: pos, map: mapObj });
      mapObj.panTo(pos);
      mapObj.setZoom(13);
    }
  } catch (_e) {
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
  } catch (_e) {
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

// ── Map pin click → fill ship address ─────────────────────────
const mapEl = ref(null);
let mapObj = null;
let pinMarker = null;

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
  mapObj = new window.google.maps.Map(mapEl.value, {
    center: { lat: 48.8566, lng: 2.3522 },
    zoom: 4,
    mapTypeId: "roadmap",
  });

  // On map click: drop pin + reverse geocode to fill address fields
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

// ── Product line state ─────────────────────────────────────────
const productSearch = ref("");
const newLine = reactive({
  productId: "",
  quantity: 1,
  unitPrice: 0,
  discount: 0,
});

const filteredProducts = computed(() =>
  productStore.products.filter((p) =>
    p.productName.toLowerCase().startsWith(productSearch.value.toLowerCase()),
  ),
);

const customerSearch = ref("");
const customerResults = ref([]);
const customerLoading = ref(false);
let customerDebounce;

async function searchCustomers(q) {
  clearTimeout(customerDebounce);
  if (!q) {
    customerResults.value = [];
    return;
  }
  customerDebounce = setTimeout(async () => {
    customerLoading.value = true;
    try {
      const { data } = await getAllCustomers(1, 10, q);
      customerResults.value = data.items;
    } catch {
      /* ignore */
    } finally {
      customerLoading.value = false;
    }
  }, 300);
}

function addLine() {
  if (!newLine.productId || newLine.quantity <= 0) {
    toast.warning("Select a product and enter a valid quantity.");
    return;
  }
  const product = productStore.products.find(
    (p) => p.productId === newLine.productId,
  );
  form.lines.push({
    productId: newLine.productId,
    productName: product?.productName || "",
    quantity: newLine.quantity,
    unitPrice: newLine.unitPrice,
    discount: newLine.discount,
  });
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

function validate() {
  Object.keys(errors).forEach((k) => delete errors[k]);
  if (!form.customerId) errors.customerId = "Customer is required.";
  if (!form.employeeId) errors.employeeId = "Employee is required.";
  if (!form.shipVia) errors.shipVia = "Shipper is required.";
  if (!form.shipmentStateId) errors.shipmentStateId = "Status is required.";
  if (!form.orderDate) errors.orderDate = "Order date is required.";
  if (!form.lines.length) errors.lines = "Add at least one product line.";
  return Object.keys(errors).length === 0;
}

async function submit() {
  if (!validate()) {
    toast.error("Please fix the form errors.");
    return;
  }
  saving.value = true;
  try {
    const payload = {
      ...form,
      employeeId: Number(form.employeeId),
      shipVia: Number(form.shipVia),
      shipmentStateId: Number(form.shipmentStateId),
      freight: Number(form.freight),
      orderDate: form.orderDate || null,
      requiredDate: form.requiredDate || null,
      shippedDate: form.shippedDate || null,
      shipRegion: form.shipRegion || null,
      billRegion: form.billRegion || null,
      lines: form.lines.map((l) => ({
        productId: l.productId,
        unitPrice: l.unitPrice,
        quantity: l.quantity,
        discount: l.discount,
      })),
    };
    await store.submitCreateOrder(payload);
    toast.success("Order created successfully.");
    router.push("/orders");
  } catch (e) {
    const serverMsg =
      typeof e?.response?.data === "string"
        ? e.response.data
        : e?.response?.data?.title || e?.response?.data?.message;
    toast.error(serverMsg || "Failed to create order.");
  } finally {
    saving.value = false;
  }
}

function formatCurrency(n) {
  return new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
  }).format(n);
}

onMounted(async () => {
  loading.value = true;
  try {
    const [sh, st] = await Promise.all([
      getAllShippers(),
      getAllShipmentStates(),
      employeeStore.fetchEmployees(),
      productStore.fetchProducts(),
    ]);
    shippers.value = sh.data;
    statuses.value = st.data;
    // Pre-fill today's date
    form.orderDate = new Date().toISOString().split("T")[0];
  } catch {
    toast.error("Failed to load form data.");
  } finally {
    loading.value = false;
  }

  // Map runs after loading=false so the mapEl div is rendered in the DOM
  await nextTick();
  try {
    await initMap();
  } catch {
    /* map unavailable – no API key or Maps JS API not enabled */
  }
});
</script>

<template>
  <AppLayout>
    <div class="page-container new-order-view">
      <div class="page-header">
        <h1>New Order</h1>
        <button
          class="btn btn-secondary btn-sm"
          @click="$router.push('/orders')"
        >
          ← Back
        </button>
      </div>

      <div v-if="loading" class="loading-center"><AppSpinner size="lg" /></div>

      <form v-else class="order-form glass" @submit.prevent="submit">
        <!-- Customer / Employee -->
        <div class="section-title">Order Details</div>
        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Customer *</label>
            <input
              v-model="customerSearch"
              list="customer-list"
              class="form-control"
              placeholder="Type to search customer..."
              @input="searchCustomers(customerSearch)"
              @change="
                (e) => {
                  const m = customerResults.find(
                    (c) => c.companyName === e.target.value,
                  );
                  if (m) {
                    form.customerId = m.customerId;
                    form.shipName = m.companyName || '';
                    form.shipCity = m.city || '';
                    form.shipCountry = m.country || '';
                  }
                }
              "
            />
            <datalist id="customer-list">
              <option
                v-for="c in customerResults"
                :key="c.customerId"
                :value="c.companyName"
              />
            </datalist>
            <span v-if="customerLoading" class="form-hint">Searching...</span>
            <span v-if="errors.customerId" class="form-error">{{
              errors.customerId
            }}</span>
          </div>
          <div class="form-group">
            <label class="form-label">Employee *</label>
            <select v-model="form.employeeId" class="form-control">
              <option value="">Select employee...</option>
              <option
                v-for="e in employeeStore.employees"
                :key="e.employeeId"
                :value="e.employeeId"
              >
                {{ e.firstName }} {{ e.lastName }}
              </option>
            </select>
            <span v-if="errors.employeeId" class="form-error">{{
              errors.employeeId
            }}</span>
          </div>
        </div>

        <div class="form-row">
          <div class="form-group">
            <label class="form-label">Shipper *</label>
            <select v-model="form.shipVia" class="form-control">
              <option value="">Select shipper...</option>
              <option
                v-for="s in shippers"
                :key="s.shipperId"
                :value="s.shipperId"
              >
                {{ s.companyName }}
              </option>
            </select>
            <span v-if="errors.shipVia" class="form-error">{{
              errors.shipVia
            }}</span>
          </div>
          <div class="form-group">
            <label class="form-label">Shipment Status *</label>
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
        </div>

        <!-- Dates -->
        <div class="form-row form-row--3">
          <div class="form-group">
            <label class="form-label">Order Date *</label>
            <input type="date" v-model="form.orderDate" class="form-control" />
            <span v-if="errors.orderDate" class="form-error">{{
              errors.orderDate
            }}</span>
          </div>
          <div class="form-group">
            <label class="form-label">Required Date</label>
            <input
              type="date"
              v-model="form.requiredDate"
              class="form-control"
            />
          </div>
          <div class="form-group">
            <label class="form-label">Shipped Date</label>
            <input
              type="date"
              v-model="form.shippedDate"
              class="form-control"
            />
          </div>
        </div>

        <div class="form-row">
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
          <div class="form-group">
            <label class="form-label">Notes</label>
            <textarea v-model="form.notes" class="form-control" rows="2" />
          </div>
        </div>

        <!-- Map -->
        <div class="section-title">
          Ship Location (click map to set address)
        </div>
        <div ref="mapEl" class="new-order-map" />

        <!-- Ship Address -->
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
                  >Lat {{ Number(shipGeocode.result.lat).toFixed(6) }}</span
                >
                <span class="coord-badge"
                  >Lng {{ Number(shipGeocode.result.lng).toFixed(6) }}</span
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

        <!-- Bill Address -->
        <div class="section-title">Bill Address</div>
        <div class="form-row form-row--4">
          <div class="form-group">
            <label class="form-label">Address</label>
            <input v-model="form.billAddress" class="form-control" />
          </div>
          <div class="form-group">
            <label class="form-label">City</label>
            <input v-model="form.billCity" class="form-control" />
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
                  >Lat {{ Number(billGeocode.result.lat).toFixed(6) }}</span
                >
                <span class="coord-badge"
                  >Lng {{ Number(billGeocode.result.lng).toFixed(6) }}</span
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

        <!-- Product Lines -->
        <div class="section-title">Product Lines</div>
        <span v-if="errors.lines" class="form-error">{{ errors.lines }}</span>

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
                  const m = productStore.products.find(
                    (p) => p.productName === e.target.value,
                  );
                  if (m) {
                    newLine.productId = m.productId;
                    newLine.unitPrice = m.unitPrice || 0;
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
            <label class="form-label">Discount (0–1)</label>
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

        <div v-if="form.lines.length" class="table-scroll">
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
                      line.unitPrice *
                        line.quantity *
                        (1 - (line.discount || 0)),
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
                <td
                  colspan="4"
                  style="
                    text-align: right;
                    font-weight: 600;
                    color: var(--text-muted);
                  "
                >
                  Total
                </td>
                <td style="font-weight: 700; color: #10b981">
                  {{ formatCurrency(runningTotal) }}
                </td>
                <td></td>
              </tr>
            </tfoot>
          </table>
        </div>

        <!-- Submit -->
        <div class="form-actions">
          <button
            type="button"
            class="btn btn-secondary"
            @click="$router.push('/orders')"
          >
            Cancel
          </button>
          <button type="submit" class="btn btn-primary" :disabled="saving">
            <AppSpinner v-if="saving" size="sm" />
            <span v-else>Create Order</span>
          </button>
        </div>
      </form>
    </div>
  </AppLayout>
</template>

<style
  lang="scss"
  src="../assets/styles/Components/NewOrderView.scss"
  scoped
></style>
