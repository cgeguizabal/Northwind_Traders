<script setup>
import { reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { useAuthStore } from "../stores/authStore.js";
import { useToast } from "vue-toastification";
import AppSpinner from "../components/common/AppSpinner.vue";
import { Building, Eye, EyeClosed, Lock } from "iconoir-vue/regular";

const auth = useAuthStore();
const router = useRouter();
const toast = useToast();

const form = reactive({ newPassword: "", confirmPassword: "" });
const errors = reactive({ newPassword: "", confirmPassword: "" });
const loading = ref(false);
const showNew = ref(false);      // toggle new-password visibility
const showConfirm = ref(false);  // toggle confirm-password visibility

// Client-side validation before sending the request
function validate() {
  errors.newPassword = "";
  errors.confirmPassword = "";
  let valid = true;

  if (!form.newPassword) {
    errors.newPassword = "New password is required.";
    valid = false;
  } else if (form.newPassword.length < 8) {
    errors.newPassword = "Password must be at least 8 characters.";
    valid = false;
  }

  if (!form.confirmPassword) {
    errors.confirmPassword = "Please confirm your password.";
    valid = false;
  } else if (form.newPassword !== form.confirmPassword) {
    errors.confirmPassword = "Passwords do not match.";
    valid = false;
  }

  return valid;
}

async function submit() {
  if (!validate()) return;
  loading.value = true;
  try {
    await auth.changePasswordFn(form.newPassword, form.confirmPassword);
    toast.success("Password updated. Welcome!");
    router.push("/dashboard");
  } catch (_e) {
    toast.error("Failed to change password. Please try again.");
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <div class="chpwd-page">
    <div class="chpwd-card glass">
      <div class="chpwd-brand">
        <Building class="chpwd-brand__icon" />
        <h1 class="chpwd-brand__name">Northwind Traders</h1>
        <p class="chpwd-brand__sub">Internal Management System</p>
      </div>

      <div class="chpwd-notice">
        <Lock class="chpwd-notice__icon" />
        <div>
          <p class="chpwd-notice__title">Password change required</p>
          <p class="chpwd-notice__text">
            For security, please set a new password before continuing.
          </p>
        </div>
      </div>

      <form class="chpwd-form" @submit.prevent="submit" novalidate>
        <div class="form-group">
          <label class="form-label" for="newPassword">New Password</label>
          <div class="password-wrapper">
            <input
              id="newPassword"
              v-model="form.newPassword"
              :type="showNew ? 'text' : 'password'"
              class="form-control"
              placeholder="Minimum 8 characters"
              autocomplete="new-password"
            />
            <button
              type="button"
              class="password-toggle"
              @click="showNew = !showNew"
              :aria-label="showNew ? 'Hide password' : 'Show password'"
            >
              <component :is="showNew ? EyeClosed : Eye" />
            </button>
          </div>
          <span v-if="errors.newPassword" class="form-error">{{
            errors.newPassword
          }}</span>
        </div>

        <div class="form-group">
          <label class="form-label" for="confirmPassword"
            >Confirm Password</label
          >
          <div class="password-wrapper">
            <input
              id="confirmPassword"
              v-model="form.confirmPassword"
              :type="showConfirm ? 'text' : 'password'"
              class="form-control"
              placeholder="Repeat new password"
              autocomplete="new-password"
            />
            <button
              type="button"
              class="password-toggle"
              @click="showConfirm = !showConfirm"
              :aria-label="showConfirm ? 'Hide password' : 'Show password'"
            >
              <component :is="showConfirm ? EyeClosed : Eye" />
            </button>
          </div>
          <span v-if="errors.confirmPassword" class="form-error">{{
            errors.confirmPassword
          }}</span>
        </div>

        <button
          type="submit"
          class="btn btn-primary chpwd-submit"
          :disabled="loading"
        >
          <AppSpinner v-if="loading" size="sm" />
          <span v-else>Set New Password</span>
        </button>
      </form>
    </div>
  </div>
</template>

<style
  lang="scss"
  src="../assets/styles/Components/ChangePasswordView.scss"
  scoped
></style>
