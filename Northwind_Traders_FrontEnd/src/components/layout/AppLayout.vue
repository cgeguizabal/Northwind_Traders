<script setup>
import AppSidebar from './AppSidebar.vue'
import AppHeader  from './AppHeader.vue'
import { useUiStore } from '../../stores/uiStore.js'

const ui = useUiStore()
</script>

<template>
  <div class="app-layout" :class="{ 'sidebar-open': ui.sidebarOpen }">
    <!-- Mobile overlay -->
    <div
      v-if="ui.sidebarOpen"
      class="app-layout__overlay"
      @click="ui.toggleSidebar()"
    />

    <AppSidebar :class="{ open: ui.sidebarOpen }" />

    <div class="app-layout__main" :class="{ expanded: !ui.sidebarOpen }">
      <AppHeader />
      <main class="app-layout__content">
        <slot />
      </main>
    </div>
  </div>
</template>

<style lang="scss" src="../../assets/styles/Components/AppLayout.scss" scoped></style>
