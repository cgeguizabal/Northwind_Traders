import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useUiStore = defineStore('ui', () => {
  // Dark mode — default dark; persisted in localStorage
  const isDark       = ref(localStorage.getItem('nt_dark') !== 'false')
  const sidebarOpen  = ref(true)   // collapsed on mobile via CSS

  function applyTheme() {
    if (isDark.value) {
      document.documentElement.classList.remove('light')
    } else {
      document.documentElement.classList.add('light')
    }
  }

  function toggleDark() {
    isDark.value = !isDark.value
    localStorage.setItem('nt_dark', String(isDark.value))
    applyTheme()
  }

  function toggleSidebar() {
    sidebarOpen.value = !sidebarOpen.value
  }

  // Apply theme on store init
  applyTheme()

  return { isDark, sidebarOpen, toggleDark, toggleSidebar }
})
