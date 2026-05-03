import { defineStore } from "pinia";
import { ref, computed } from "vue";
import { jwtDecode } from "jwt-decode";
import { login as loginApi } from "../axiosInstance/authService.js";
import { getEmployeeById } from "../axiosInstance/employeeService.js";

// .NET JWT serialises ClaimTypes.Name → "unique_name"
// Fall back to other possible key variants for safety
function getClaimName(decoded) {
  return (
    decoded?.unique_name ||
    decoded?.name ||
    decoded?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ||
    ""
  );
}

function getClaimId(decoded) {
  return (
    decoded?.nameid ||
    decoded?.sub ||
    decoded?.[
      "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
    ] ||
    null
  );
}

export const useAuthStore = defineStore("auth", () => {
  const token = ref(localStorage.getItem("nt_token") || null);
  const user = ref(null);
  const employeeTitle = ref(localStorage.getItem("nt_emp_title") || "");

  // Decode stored token on store init if it exists
  if (token.value) {
    try {
      user.value = jwtDecode(token.value);
    } catch {
      token.value = null;
      localStorage.removeItem("nt_token");
    }
  }

  const isAuthenticated = computed(() => !!token.value);

  // Display name – uses correct JWT claim key
  const displayName = computed(
    () => getClaimName(user.value) || "Employee",
  );

  // isManager: only Vice President, Sales or Sales Manager can access /employees
  const isManager = computed(() => {
    const title = employeeTitle.value || "";
    return title === "Vice President, Sales" || title === "Sales Manager";
  });

  async function loginUser(credentials) {
    const response = await loginApi(credentials);
    const { token: jwt } = response.data;
    token.value = jwt;
    localStorage.setItem("nt_token", jwt);
    const decoded = jwtDecode(jwt);
    user.value = decoded;

    // Fetch employee to get title (needed for role-based access control)
    try {
      const empId = getClaimId(decoded);
      if (empId) {
        const { data: emp } = await getEmployeeById(Number(empId));
        employeeTitle.value = emp.title || "";
        localStorage.setItem("nt_emp_title", emp.title || "");
      }
    } catch {
      // Non-critical – isManager defaults to false if fetch fails
    }
  }

  function logout() {
    token.value = null;
    user.value = null;
    employeeTitle.value = "";
    localStorage.removeItem("nt_token");
    localStorage.removeItem("nt_emp_title");
  }

  return {
    token,
    user,
    displayName,
    isAuthenticated,
    isManager,
    loginUser,
    logout,
  };
});
