<script setup>
defineProps({
  size: { type: String, default: 'md' }, // sm | md | lg
})
</script>

<template>
  <div class="spinner" :class="`spinner--${size}`" aria-label="Loading...">
    <div class="spinner__ring" />
  </div>
</template>

<style lang="scss" src="../../assets/styles/Components/AppSpinner.scss" scoped></style>
