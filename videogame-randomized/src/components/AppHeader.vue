<script setup>
import { ref } from 'vue'
import { useVaultStore } from '@/stores/useVaultStore'
import { useAuthStore } from '@/stores/useAuthStore'
import LanguageSwitcher from '@/components/base/LanguageSwitcher.vue'

const emit = defineEmits(['open-vault', 'open-change-password', 'logout'])

const vault = useVaultStore()
const authStore = useAuthStore()
const showUserMenu = ref(false)

const toggleUserMenu = () => (showUserMenu.value = !showUserMenu.value)

const handleOpenChangePassword = () => {
  showUserMenu.value = false
  emit('open-change-password')
}
</script>

<template>
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
          <button
class="group flex items-center gap-2 px-4 py-2.5 bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 hover:border-fuchsia-500/50 rounded-xl transition-all"
            @click="emit('open-vault')">
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
          <button
class="flex items-center gap-2 px-3 py-2.5 bg-zinc-900 hover:bg-zinc-800 border border-zinc-800 hover:border-cyan-500/50 rounded-xl transition-all"
            @click="toggleUserMenu">
            <svg class="w-5 h-5 text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            </svg>
            <span class="text-sm text-zinc-400 hidden md:block max-w-[120px] truncate">{{ authStore.email }}</span>
            <svg class="w-4 h-4 text-zinc-500" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
            </svg>
          </button>

          <!-- Dropdown -->
          <div
v-if="showUserMenu"
            class="absolute right-0 mt-2 w-56 bg-zinc-900 border border-zinc-800 rounded-xl shadow-xl overflow-hidden z-50">
            <div class="px-4 py-3 border-b border-zinc-800">
              <p class="text-xs text-zinc-500 uppercase tracking-wider">{{ $t('home.signed_in_as') }}</p>
              <p class="text-sm text-white font-semibold truncate">{{ authStore.email }}</p>
            </div>
            <button
class="w-full text-left px-4 py-3 text-sm text-zinc-300 hover:bg-zinc-800 hover:text-white transition-colors flex items-center gap-2"
              @click="handleOpenChangePassword">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 7a2 2 0 012 2m4 0a6 6 0 01-7.743 5.743L11 17H9v2H7v2H4a1 1 0 01-1-1v-2.586a1 1 0 01.293-.707l5.964-5.964A6 6 0 1121 9z" />
              </svg>
              {{ $t('home.change_password') }}
            </button>
            <button
class="w-full text-left px-4 py-3 text-sm text-red-400 hover:bg-zinc-800 hover:text-red-300 transition-colors flex items-center gap-2 border-t border-zinc-800"
              @click="emit('logout')">
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
  <div v-if="showUserMenu" class="fixed inset-0 z-40" @click="showUserMenu = false"></div>
</template>
