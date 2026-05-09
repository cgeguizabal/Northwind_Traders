import { defineStore } from "pinia";
import { ref } from "vue";
import {
  getAllEmployees,
  getEmployeeById,
  updateEmployee,
} from "../axiosInstance/employeeService.js";

export const useEmployeeStore = defineStore("employees", () => {
  const employees = ref([]); // full employee list
  const current = ref(null); // single employee for detail/edit
  const loading = ref(false);
  const error = ref(null);

  async function fetchEmployees() {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await getAllEmployees();
      employees.value = data;
    } catch (e) {
      error.value = e;
      throw e;
    } finally {
      loading.value = false;
    }
  }

  async function fetchEmployee(id) {
    loading.value = true;
    error.value = null;
    try {
      const { data } = await getEmployeeById(id);
      current.value = data;
      return data;
    } catch (e) {
      error.value = e;
      throw e;
    } finally {
      loading.value = false;
    }
  }

  async function submitUpdateEmployee(id, payload) {
    const { data } = await updateEmployee(id, payload);
    // Refresh the local list entry
    const idx = employees.value.findIndex((e) => e.employeeId === id);
    if (idx !== -1) employees.value[idx] = { ...employees.value[idx], ...data };
    return data;
  }

  return {
    employees,
    current,
    loading,
    error,
    fetchEmployees,
    fetchEmployee,
    submitUpdateEmployee,
  };
});
