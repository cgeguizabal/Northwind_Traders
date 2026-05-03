<script setup>
import AppModal from './AppModal.vue'

defineProps({
  title:   { type: String, default: 'Confirm' },
  message: { type: String, default: 'Are you sure?' },
})

const emit = defineEmits(['confirm', 'cancel'])
</script>

<template>
  <AppModal :title="title" width="400px" @close="$emit('cancel')">
    <p class="confirm-dialog__msg">{{ message }}</p>

    <template #footer>
      <button class="btn btn-secondary" @click="$emit('cancel')">Cancel</button>
      <button class="btn btn-danger"    @click="$emit('confirm')">Confirm</button>
    </template>
  </AppModal>
</template>

<style lang="scss" src="../../assets/styles/Components/ConfirmDialog.scss" scoped></style>
