<script setup>
defineProps({
  variant: { type: String, default: 'default' }, 
  label:   { type: String, required: true },
})
</script>

<template>
  <span class="badge" :class="`badge-${variant}`">{{ label }}</span>
</template>
