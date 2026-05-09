<script setup>
// variant controls the CSS color class applied to the badge
// Supported: 'default' | 'green' | 'gold' | 'danger' | 'purple'
defineProps({
  variant: { type: String, default: "default" },
  label: { type: String, required: true },
});
</script>

<template>
  <span class="badge" :class="`badge-${variant}`">{{ label }}</span>
</template>
