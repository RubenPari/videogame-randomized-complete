<script setup>
import { ref, onMounted, defineAsyncComponent } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useVaultStore } from '@/stores/useVaultStore'
import { useAuthStore } from '@/stores/useAuthStore'
import { useToastStore } from '@/stores/useToastStore'
import { useGameDiscovery } from '@/composables/useGameDiscovery'
import apiService from '@/services/api'
import authService from '@/services/auth'

const { t } = useI18n()

// Components
import FilterSection from '@/components/FilterSection.vue'
import GameCard from '@/components/GameCard.vue'
import GameHistory from '@/components/GameHistory.vue'
import GameStatePlaceholder from '@/components/GameStatePlaceholder.vue'
import LanguageSwitcher from '@/components/LanguageSwitcher.vue'

// Async Modal (Optimization)
const SaveGamesModal = defineAsyncComponent(() =>
  import('@/components/SaveGamesModal.vue')
)

// Store & Composable
const router = useRouter()
const vault = useVaultStore()
const authStore = useAuthStore()
const toastStore = useToastStore()
const discovery = useGameDiscovery()

// Local State
const genres = ref([])
const platforms = ref([])
const showSavedGamesModal = ref(false)
const showUserMenu = ref(false)
const showChangePassword = ref(false)
const currentPassword = ref('')
const newPassword = ref('')
const changePwdLoading = ref(false)
const changePwdError = ref('')

/**
 * Initial load: populate genres, platforms and vault data
 */
onMounted(async () => {
  try {
    const [genresRes, platformsRes] = await Promise.all([
      apiService.getGenres(),
      apiService.getPlatforms()
    ])
    genres.value = genresRes.data.results
    platforms.value = platformsRes.data.results
    await Promise.all([
      vault.loadVault(),
      discovery.loadPastHistory()
    ])
  } catch (err) {
    console.error('Initialization error:', err)
  }
})

// UI Event Handlers
const openVault = () => (showSavedGamesModal.value = true)
const closeVault = () => (showSavedGamesModal.value = false)
const toggleUserMenu = () => (showUserMenu.value = !showUserMenu.value)

const handleLogout = () => {
  authStore.logout()
  toastStore.showToast(t('home.logged_out'), 'success')
  router.push('/login')
}

const openChangePassword = () => {
  showChangePassword.value = true
  showUserMenu.value = false
  currentPassword.value = ''
  newPassword.value = ''
  changePwdError.value = ''
}

const handleChangePassword = async () => {
  changePwdError.value = ''
  changePwdLoading.value = true
  try {
    await authService.changePassword(currentPassword.value, newPassword.value)
    showChangePassword.value = false
    toastStore.showToast(t('home.password_changed'), 'success')
  } catch (err) {
    changePwdError.value = err.response?.data?.error || t('home.password_change_error')
  } finally {
    changePwdLoading.value = false
  }
}

const handleClearPastHistory = async () => {
  try {
    await discovery.clearPastHistory()
    toastStore.showToast(t('home.history_cleared'), 'success')
  } catch {
    toastStore.showToast(t('home.history_clear_error'), 'error')
  }
}
</script>

<template>
  <div class="min-h-screen bg-zinc-950 text-zinc-300 font-sans selection:bg-cyan-500/30">
    <header class="sticky top-0 z-50 bg-zinc-950/90 backdrop-blur-md border-b border-zinc-800">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 h-20 flex items-center justify-between">
        <div class="flex items-center gap-3">
          <div class="w-10 h-10 bg-cyan-500 text-zinc-950 flex items-center justify-center rounded-lg font-black text-xl transform -skew-x-6">
            VG
          </div>
          <h1 class="text-2xl font-black text-white tracking-tight uppercase italic">
            Random<span class="text-cyan-400">Generator</span>
          </h1>
        </div>

        <div class="flex items-center gap-3">
            <!-- Language Switcher -->
            <LanguageSwitcher />

            <!-- Vault Button -->
            <button @click="openVault"
              class="group flex items-center gap-2 px-4 py-2.5 bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 hover:border-fuchsia-500/50 rounded-xl transition-all">
              <svg class="w-5 h-5 text-fuchsia-500 group-hover:scale-110 transition-transform" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="square" stroke-linejoin="miter" stroke-width="2" d="M5 5a2 2 0 012-2h10a2 2 0 012 2v16l-7-3.5L5 21V5z"></path>
              </svg>
              <span class="font-bold text-white uppercase text-sm tracking-wider hidden sm:block">{{ $t('nav.vault') }}</span>
            <span v-if="vault.count > 0" class="flex h-5 w-5 items-center justify-center rounded-md bg-fuchsia-500 text-[10px] font-black text-zinc-950 ml-1">
              {{ vault.count }}
            </span>
          </button>

          <!-- User Menu -->
          <div class="relative">
            <button @click="toggleUserMenu"
              class="flex items-center gap-2 px-3 py-2.5 bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 hover:border-cyan-500/50 rounded-xl transition-all">
              <svg class="w-5 h-5 text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
              </svg>
              <span class="text-sm text-zinc-400 hidden md:block max-w-[120px] truncate">{{ authStore.email }}</span>
              <svg class="w-4 h-4 text-zinc-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
              </svg>
            </button>

            <!-- Dropdown -->
            <div v-if="showUserMenu"
              class="absolute right-0 mt-2 w-56 bg-zinc-900 border border-zinc-800 rounded-xl shadow-xl overflow-hidden z-50">
              <div class="px-4 py-3 border-b border-zinc-800">
                <p class="text-xs text-zinc-500 uppercase tracking-wider">{{ $t('home.signed_in_as') }}</p>
                <p class="text-sm text-white font-semibold truncate">{{ authStore.email }}</p>
              </div>
              <button @click="openChangePassword"
                class="w-full text-left px-4 py-3 text-sm text-zinc-300 hover:bg-zinc-800 hover:text-white transition-colors flex items-center gap-2">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
                </svg>
                {{ $t('home.change_password') }}
              </button>
              <button @click="handleLogout"
                class="w-full text-left px-4 py-3 text-sm text-red-400 hover:bg-zinc-800 hover:text-red-300 transition-colors flex items-center gap-2 border-t border-zinc-800">
                <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
                {{ $t('home.sign_out') }}
              </button>
            </div>
          </div>
        </div>
      </div>
    </header>

    <!-- Click outside to close user menu -->
    <div v-if="showUserMenu" @click="showUserMenu = false" class="fixed inset-0 z-40"></div>

    <main class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12 flex flex-col lg:flex-row gap-8">
      <!-- Sidebar with Filters -->
      <div class="w-full lg:w-1/3 lg:sticky lg:top-28 h-fit">
        <FilterSection
          v-model="discovery.filters"
          :genres="genres"
          :platforms="platforms"
          :isLoading="discovery.isLoading.value"
          @generate="discovery.generateGame"
        />
      </div>

      <!-- Main Results Column -->
      <div class="w-full lg:w-2/3 space-y-8 flex flex-col">
        <div class="min-h-[400px] flex flex-col relative">
          <GameCard
            v-if="discovery.currentGame.value"
            :game="discovery.currentGame.value"
            :description="discovery.gameDescription.value"
            class="animate-fade-in"
          />

          <!-- Loading State -->
          <GameStatePlaceholder
            v-else-if="discovery.isLoading.value"
            :isLoading="true"
          />

          <!-- Empty State -->
          <GameStatePlaceholder
            v-else
            :isLoading="false"
          />
        </div>

        <!-- History -->
        <GameHistory
          :gameHistory="discovery.gameHistory.value"
          :pastHistory="discovery.pastHistory.value"
          @clear-history="discovery.clearHistory"
          @clear-past-history="handleClearPastHistory"
        />

        <!-- Error Reporting -->
        <div v-if="discovery.error.value" class="bg-red-500/10 border-l-2 border-red-500 text-red-400 p-4 rounded-r-xl text-sm font-mono animate-fade-in">
          <p class="font-bold uppercase tracking-wider mb-1">{{ $t('error.system_error') }}</p>
          <p>{{ discovery.error.value }}</p>
        </div>
      </div>
    </main>

    <SaveGamesModal
      v-if="showSavedGamesModal"
      :show="showSavedGamesModal"
      @close="closeVault"
    />

    <!-- Change Password Modal -->
    <Teleport to="body">
      <div v-if="showChangePassword" class="fixed inset-0 z-[100] flex items-center justify-center">
        <div @click="showChangePassword = false" class="absolute inset-0 bg-black/60 backdrop-blur-sm"></div>
        <div class="relative bg-zinc-900 border border-zinc-800 rounded-2xl p-6 w-full max-w-sm mx-4 space-y-5">
          <h3 class="text-lg font-bold text-white">{{ $t('home.change_password') }}</h3>

          <div v-if="changePwdError"
            class="bg-red-500/10 border border-red-500/30 text-red-400 px-3 py-2 rounded-lg text-sm">
            {{ changePwdError }}
          </div>

          <div class="space-y-2">
            <label class="block text-xs font-semibold text-zinc-400 uppercase tracking-wider">{{ $t('auth.current_password') }}</label>
            <input v-model="currentPassword" type="password" autocomplete="current-password"
              class="w-full px-3 py-2.5 bg-zinc-800 border border-zinc-700 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-cyan-500/50 text-sm" />
          </div>

          <div class="space-y-2">
            <label class="block text-xs font-semibold text-zinc-400 uppercase tracking-wider">{{ $t('auth.new_password') }}</label>
            <input v-model="newPassword" type="password" autocomplete="new-password" :placeholder="$t('auth.min_characters')"
              class="w-full px-3 py-2.5 bg-zinc-800 border border-zinc-700 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-cyan-500/50 text-sm" />
          </div>

          <div class="flex gap-3">
            <button @click="showChangePassword = false"
              class="flex-1 py-2.5 bg-zinc-800 hover:bg-zinc-700 text-white font-semibold rounded-lg text-sm transition-all">
              {{ $t('auth.back_to_login').replace('Back to ', '') }}
            </button>
            <button @click="handleChangePassword" :disabled="changePwdLoading"
              class="flex-1 py-2.5 bg-cyan-500 hover:bg-cyan-400 text-zinc-950 font-bold rounded-lg text-sm transition-all disabled:opacity-50">
              <span v-if="!changePwdLoading">{{ $t('home.change_password') }}</span>
              <span v-else>{{ $t('history.saving') }}</span>
            </button>
          </div>
        </div>
      </div>
    </Teleport>
  </div>
</template>
