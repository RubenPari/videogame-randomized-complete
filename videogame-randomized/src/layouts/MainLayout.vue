<script setup>
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

const { t } = useI18n()
const router = useRouter()
const authStore = useAuthStore()
const toastStore = useToastStore()

const showSavedGamesModal = ref(false)
const showChangePassword = ref(false)

const openVault = () => (showSavedGamesModal.value = true)
const closeVault = () => (showSavedGamesModal.value = false)
const openChangePassword = () => (showChangePassword.value = true)

const handleLogout = () => {
  authStore.logout()
  toastStore.showToast(t('home.logged_out'), 'success')
  router.push('/login')
}
</script>

<template>
  <div class="min-h-screen bg-zinc-950 text-zinc-300 font-sans selection:bg-cyan-500/30">
    <AppHeader
      @open-vault="openVault"
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

    <ChangePasswordModal
      :show="showChangePassword"
      @close="showChangePassword = false"
    />
  </div>
</template>
