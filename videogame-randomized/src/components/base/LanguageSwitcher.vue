<template>
  <div class="relative">
    <button
      class="flex items-center gap-2 px-3 py-2 rounded-lg bg-zinc-800/50 border border-zinc-700 hover:bg-zinc-800 transition-colors text-sm font-medium"
      aria-label="Select Language"
      @click="isOpen = !isOpen"
    >
      <span class="text-xl leading-none">{{ currentLangInfo.flag }}</span>
      <span class="hidden sm:inline">{{ currentLangInfo.name }}</span>
      <svg
        class="w-4 h-4 text-zinc-400 transition-transform"
        :class="{ 'rotate-180': isOpen }"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
      </svg>
    </button>

    <!-- Dropdown -->
    <div
      v-if="isOpen"
      class="absolute right-0 mt-2 w-40 bg-zinc-800 border border-zinc-700 rounded-lg shadow-xl py-1 z-50 overflow-hidden"
    >
      <button
        v-for="lang in availableLanguages"
        :key="lang.code"
        class="w-full text-left px-4 py-2 text-sm hover:bg-zinc-700 transition-colors flex items-center justify-between"
        :class="{ 'text-cyan-400 font-medium bg-zinc-700/50': currentLocale === lang.code }"
        @click="switchLanguage(lang.code)"
      >
        <div class="flex items-center gap-3">
          <span class="text-lg">{{ lang.flag }}</span>
          <span>{{ lang.name }}</span>
        </div>
        <svg
          v-if="currentLocale === lang.code"
          class="w-4 h-4 text-cyan-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useI18n } from 'vue-i18n'

const { locale } = useI18n()
const isOpen = ref(false)

const availableLanguages = [
  { code: 'en', name: 'English', flag: '🇺🇸' },
  { code: 'it', name: 'Italiano', flag: '🇮🇹' },
  { code: 'es', name: 'Español', flag: '🇪🇸' }
]

const currentLocale = computed(() => locale.value)
const currentLangInfo = computed(() => 
  availableLanguages.find(lang => lang.code === locale.value) || availableLanguages[0]
)

const switchLanguage = (code) => {
  locale.value = code
  localStorage.setItem('user-locale', code)
  
  // Set html lang attribute for accessibility
  document.documentElement.lang = code
  
  isOpen.value = false
}

// Close dropdown when clicking outside
const closeDropdown = (e) => {
  if (isOpen.value && !e.target.closest('.relative')) {
    isOpen.value = false
  }
}

onMounted(() => {
  document.addEventListener('click', closeDropdown)
  // Ensure html lang is set on mount
  document.documentElement.lang = locale.value
})

onUnmounted(() => {
  document.removeEventListener('click', closeDropdown)
})
</script>
