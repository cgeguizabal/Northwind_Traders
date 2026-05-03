<script setup>
import { ref, onMounted } from "vue";
import AppLayout from "../components/layout/AppLayout.vue";
import AppSpinner from "../components/common/AppSpinner.vue";
import EmployeeEditModal from "../components/employees/EmployeeEditModal.vue";
import { useEmployeeStore } from "../stores/employeeStore.js";
import { useToast } from "vue-toastification";
import { getEmployeePhoto } from "../axiosInstance/employeeService.js";
import { EditPencil } from "iconoir-vue/regular";

const store = useEmployeeStore();
const toast = useToast();

const editTarget = ref(null);
const showEdit = ref(false);

// Cache for blob photo URLs to avoid repeated fetches
const photoUrls = ref({});

onMounted(async () => {
  try {
    await store.fetchEmployees();
  } catch {
    toast.error("Failed to load employees.");
  }
});

async function loadPhoto(id) {
  if (photoUrls.value[id]) return;
  try {
    const { data } = await getEmployeePhoto(id);
    photoUrls.value[id] = URL.createObjectURL(new Blob([data]));
  } catch {
    photoUrls.value[id] = null;
  }
}

function openEdit(emp) {
  editTarget.value = emp;
  showEdit.value = true;
}
function closeEdit() {
  showEdit.value = false;
  editTarget.value = null;
}

async function onSaved() {
  closeEdit();
  await store.fetchEmployees();
}
</script>

<template>
  <AppLayout>
    <div class="page-container">
      <div class="page-header"><h1>Employees</h1></div>

      <div v-if="store.loading" class="spinner-center">
        <AppSpinner size="lg" />
      </div>

      <div v-else class="emp-grid">
        <div
          v-for="emp in store.employees"
          :key="emp.employeeId"
          class="emp-card glass"
          @vue:mounted="loadPhoto(emp.employeeId)"
        >
          <!-- Photo -->
          <div class="emp-card__photo">
            <img
              v-if="photoUrls[emp.employeeId]"
              :src="photoUrls[emp.employeeId]"
              :alt="`${emp.firstName} ${emp.lastName}`"
            />
            <span v-else class="emp-card__initials">
              {{ (emp.firstName || "").charAt(0)
              }}{{ (emp.lastName || "").charAt(0) }}
            </span>
          </div>

          <!-- Info -->
          <div class="emp-card__info">
            <h3 class="emp-card__name">
              {{ emp.firstName }} {{ emp.lastName }}
            </h3>
            <p class="emp-card__title">{{ emp.title }}</p>
            <p class="emp-card__location">{{ emp.city }}, {{ emp.country }}</p>
          </div>

          <button
            class="btn btn-secondary btn-sm emp-card__edit"
            @click="openEdit(emp)"
          >
            <EditPencil /> Edit
          </button>
        </div>
      </div>

      <EmployeeEditModal
        v-if="showEdit && editTarget"
        :employee="editTarget"
        @close="closeEdit"
        @saved="onSaved"
      />
    </div>
  </AppLayout>
</template>

<style
  lang="scss"
  src="../assets/styles/Components/EmployeesView.scss"
  scoped
></style>
