import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useAppStore = defineStore('app', () => {
  const sidebarCollapsed = ref(false)
  const loading = ref(false)

  const toggleSidebar = () => {
    sidebarCollapsed.value = !sidebarCollapsed.value
  }

  const setLoading = (status) => {
    loading.value = status
  }

  return {
    sidebarCollapsed,
    loading,
    toggleSidebar,
    setLoading
  }
})
