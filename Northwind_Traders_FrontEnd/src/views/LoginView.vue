<script setup>
import { reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "../stores/authStore.js";
import { useToast } from "vue-toastification";
import AppSpinner from "../components/common/AppSpinner.vue";
import { Building, Eye, EyeClosed } from "iconoir-vue/regular";

const auth = useAuthStore();
const router = useRouter();
const toast = useToast();

const form = reactive({ email: "", password: "" });
const errors = reactive({ email: "", password: "" });
const loading = ref(false);
const showPassword = ref(false); // toggles between input type='password' and type='text'

// Client-side validation before sending the request
function validate() {
  errors.email = "";
  errors.password = "";
  let valid = true;

  if (!form.email) {
    errors.email = "Email is required.";
    valid = false;
  } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email)) {
    errors.email = "Enter a valid email address.";
    valid = false;
  }

  if (!form.password) {
    errors.password = "Password is required.";
    valid = false;
  }

  return valid;
}

async function submit() {
  if (!validate()) return;
  loading.value = true;
  try {
    await auth.loginUser({ email: form.email, password: form.password });
    // Redirect to change-password if the employee must set a new one first
    if (auth.mustChangePassword) {
      router.push("/change-password");
    } else {
      router.push("/dashboard");
    }
  } catch {
    toast.error("Invalid email or password.");
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="login-page">
    <div class="login-card glass">
      <div class="login-brand">
        <Building class="login-brand__icon" />
        <h1 class="login-brand__name">Northwind Traders</h1>
        <p class="login-brand__sub">Internal Management System</p>
      </div>

      <form class="login-form" @submit.prevent="submit" novalidate>
        <div class="form-group">
          <label class="form-label" for="email">Email</label>
          <input
            id="email"
            v-model="form.email"
            type="email"
            class="form-control"
            placeholder="you@northwind.com"
            autocomplete="email"
          />
          <span v-if="errors.email" class="form-error">{{ errors.email }}</span>
        </div>

        <div class="form-group">
          <label class="form-label" for="password">Password</label>
          <div class="password-wrapper">
            <input
              id="password"
              v-model="form.password"
              :type="showPassword ? 'text' : 'password'"
              class="form-control"
              placeholder="••••••••"
              autocomplete="current-password"
            />
            <button
              type="button"
              class="password-toggle"
              @click="showPassword = !showPassword"
              :aria-label="showPassword ? 'Hide password' : 'Show password'"
            >
              <component :is="showPassword ? EyeClosed : Eye" />
            </button>
          </div>
          <span v-if="errors.password" class="form-error">{{
            errors.password
          }}</span>
        </div>

        <button
          type="submit"
          class="btn btn-primary login-submit"
          :disabled="loading"
        >
          <AppSpinner v-if="loading" size="sm" />
          <span v-else>Sign In</span>
        </button>
      </form>
    </div>
  </div>
</template>

<style
  lang="scss"
  src="../assets/styles/Components/LoginView.scss"
  scoped
></style>
