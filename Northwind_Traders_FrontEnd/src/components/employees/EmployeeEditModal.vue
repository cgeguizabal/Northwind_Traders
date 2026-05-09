<script setup>
import { reactive, ref, onMounted } from "vue";
import AppModal from "../common/AppModal.vue";
import AppSpinner from "../common/AppSpinner.vue";
import { useToast } from "vue-toastification";
import { useEmployeeStore } from "../../stores/employeeStore.js";

const props = defineProps({
  employee: { type: Object, required: true },
});

const emit = defineEmits(["close", "saved"]);
const toast = useToast();
const store = useEmployeeStore();

const saving = ref(false);

// Allowed job titles — only these four are valid in this system
const TITLE_OPTIONS = [
  "Sales Representative",
  "Inside Sales Coordinator",
  "Sales Manager",
  "Vice President, Sales",
];

// Pre-populate the form from the prop so edits start from current values
const form = reactive({
  title: props.employee.title || "",
  titleOfCourtesy: props.employee.titleOfCourtesy || "",
  address: props.employee.address || "",
  city: props.employee.city || "",
  region: props.employee.region || "",
  postalCode: props.employee.postalCode || "",
  country: props.employee.country || "",
  homePhone: props.employee.homePhone || "",
  extension: props.employee.extension || "",
  notes: props.employee.notes || "",
  photoPath: props.employee.photoPath || "",
});

function validate() {
  if (!form.title) {
    toast.error("Title is required.");
    return false;
  }
  return true;
}

async function save() {
  if (!validate()) return;
  saving.value = true;
  try {
    await store.submitUpdateEmployee(props.employee.employeeId, form);
    toast.success("Employee updated successfully.");
    emit("saved");
  } catch (e) {
    toast.error(e?.response?.data?.message || "Failed to update employee.");
  } finally {
    saving.value = false;
  }
}
</script>

<template>
  <AppModal
    :title="`Edit — ${employee.firstName} ${employee.lastName}`"
    width="600px"
    @close="$emit('close')"
  >
    <form class="emp-form" @submit.prevent="save">
      <div class="form-row">
        <div class="form-group">
          <label class="form-label">Title</label>
          <select v-model="form.title" class="form-control">
            <option v-for="t in TITLE_OPTIONS" :key="t" :value="t">
              {{ t }}
            </option>
          </select>
        </div>
        <div class="form-group">
          <label class="form-label">Title of Courtesy</label>
          <input
            v-model="form.titleOfCourtesy"
            class="form-control"
            placeholder="Mr., Ms., Dr..."
          />
        </div>
      </div>

      <div class="form-group">
        <label class="form-label">Address</label>
        <input v-model="form.address" class="form-control" />
      </div>

      <div class="form-row form-row--3">
        <div class="form-group">
          <label class="form-label">City</label>
          <input v-model="form.city" class="form-control" />
        </div>
        <div class="form-group">
          <label class="form-label">Region</label>
          <input v-model="form.region" class="form-control" />
        </div>
        <div class="form-group">
          <label class="form-label">Postal Code</label>
          <input v-model="form.postalCode" class="form-control" />
        </div>
      </div>

      <div class="form-row">
        <div class="form-group">
          <label class="form-label">Country</label>
          <input v-model="form.country" class="form-control" />
        </div>
        <div class="form-group">
          <label class="form-label">Home Phone</label>
          <input v-model="form.homePhone" class="form-control" />
        </div>
      </div>

      <div class="form-row">
        <div class="form-group">
          <label class="form-label">Extension</label>
          <input v-model="form.extension" class="form-control" />
        </div>
        <div class="form-group">
          <label class="form-label">Photo Path</label>
          <input v-model="form.photoPath" class="form-control" />
        </div>
      </div>

      <div class="form-group">
        <label class="form-label">Notes</label>
        <textarea v-model="form.notes" class="form-control" rows="3" />
      </div>
    </form>

    <template #footer>
      <button class="btn btn-secondary" @click="$emit('close')">Cancel</button>
      <button class="btn btn-primary" :disabled="saving" @click="save">
        <AppSpinner v-if="saving" size="sm" />
        <span v-else>Save Changes</span>
      </button>
    </template>
  </AppModal>
</template>

<style
  lang="scss"
  src="../../assets/styles/Components/EmployeeEditModal.scss"
  scoped
></style>
