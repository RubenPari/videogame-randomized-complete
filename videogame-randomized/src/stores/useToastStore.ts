import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useToastStore = defineStore('toast', () => {
  const showNotification = ref<boolean>(false)
  const notificationType = ref<string>('info')
  const notificationMessage = ref<string>('')

  let timeoutId: ReturnType<typeof setTimeout> | null = null

  function showToast(message: string, type: string = 'info'): void {
    if (timeoutId) {
      clearTimeout(timeoutId)
    }

    notificationMessage.value = message
    notificationType.value = type
    showNotification.value = true

    timeoutId = setTimeout(() => {
      hideToast()
    }, 3000)
  }

  function hideToast(): void {
    showNotification.value = false
    if (timeoutId) {
      clearTimeout(timeoutId)
      timeoutId = null
    }
  }

  return { showNotification, notificationType, notificationMessage, showToast, hideToast }
})
