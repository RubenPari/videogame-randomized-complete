import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useToastStore = defineStore('toast', () => {
  const showNotification = ref(false)
  const notificationType = ref('success')
  const notificationMessage = ref('')
  let timeoutId = null

  const showToast = (message, type = 'success') => {
    // Clear any existing timeout so messages don't disappear too quickly if triggered repeatedly
    if (timeoutId) clearTimeout(timeoutId)

    notificationMessage.value = message
    notificationType.value = type
    showNotification.value = true

    timeoutId = setTimeout(() => {
      showNotification.value = false
    }, 3000)
  }

  const hideToast = () => {
    showNotification.value = false
    if (timeoutId) clearTimeout(timeoutId)
  }

  return {
    showNotification,
    notificationType,
    notificationMessage,
    showToast,
    hideToast
  }
})
