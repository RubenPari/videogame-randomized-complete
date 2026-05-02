<script setup lang="ts">
import { ref, defineAsyncComponent } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/useAuthStore'
import { useToastStore } from '@/stores/useToastStore'
import AppHeader from '@/components/AppHeader.vue'
import ChangePasswordModal from '@/components/ChangePasswordModal.vue'

const SaveGamesModal = defineAsyncComponent(() =>
  import('@/components/SaveGamesModal.vue')
)

const DiscoveredTitlesModal = defineAsyncComponent(() =>
  import('@/components/DiscoveredTitlesModal.vue')
)

const { t } = useI18n()
const router = useRouter()
const authStore = useAuthStore()
const toastStore = useToastStore()

const showSavedGamesModal = ref<boolean>(false)
const showDiscoveredTitlesModal = ref<boolean>(false)
const showChangePassword = ref<boolean>(false)

const openVault = (): void => {
  showSavedGamesModal.value = true
}
const closeVault = (): void => {
  showSavedGamesModal.value = false
}
const openDiscoveredTitles = (): void => {
  showDiscoveredTitlesModal.value = true
}
const closeDiscoveredTitles = (): void => {
  showDiscoveredTitlesModal.value = false
}
const openChangePassword = (): void => {
  showChangePassword.value = true
}

const handleLogout = (): void => {
  authStore.logout()
  toastStore.showToast(t('home.logged_out'), 'success')
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen bg-zinc-950 text-zinc-300 font-sans selection:bg-cyan-500/30">
    <AppHeader
      @open-vault="openVault"
      @open-discovered="openDiscoveredTitles"
      @open-change-password="openChangePassword"
      @logout="handleLogout"
    />

    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12">
      <router-view />
    </main>

    <SaveGamesModal
      v-if="showSavedGamesModal"
      :show="showSavedGamesModal"
      @close="closeVault"
    />

    <DiscoveredTitlesModal
      v-if="showDiscoveredTitlesModal"
      :show="showDiscoveredTitlesModal"
      @close="closeDiscoveredTitles"
    />

    <ChangePasswordModal
      :show="showChangePassword"
      @close="showChangePassword = false"
    />
  </div>
</template>
