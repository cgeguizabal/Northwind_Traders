<script setup>
import { onMounted, onUnmounted } from 'vue'
import { Xmark } from 'iconoir-vue/regular'

const props = defineProps({
  title:     { type: String, default: '' },
  width:     { type: String, default: '600px' },
  noPadding: { type: Boolean, default: false },
})

const emit = defineEmits(['close'])

// Close on Escape key
function handleKeydown(e) {
  if (e.key === 'Escape') emit('close')
}

onMounted(() => {
  document.addEventListener('keydown', handleKeydown)
  document.body.style.overflow = 'hidden'
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleKeydown)
  document.body.style.overflow = ''
})
</script>

<template>
  <Teleport to="body">
    <div class="modal-backdrop" @click.self="$emit('close')">
      <div class="modal-box glass" :style="{ maxWidth: width }">
        <!-- Header -->
        <div v-if="title" class="modal-box__header">
          <h2 class="modal-box__title">{{ title }}</h2>
          <button class="modal-box__close" @click="$emit('close')" aria-label="Close"><Xmark /></button>
        </div>
        <!-- Body -->
        <div class="modal-box__body" :class="{ 'no-padding': noPadding }">
          <slot />
        </div>
        <!-- Footer slot (optional) -->
        <div v-if="$slots.footer" class="modal-box__footer">
          <slot name="footer" />
        </div>
      </div>
    </div>
  </Teleport>
</template>

<style lang="scss" src="../../assets/styles/Components/AppModal.scss" scoped></style>
