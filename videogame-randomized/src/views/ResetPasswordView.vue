<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { useI18n } from 'vue-i18n'
import authService from '@/services/auth'
import { getApiErrorMessage } from '@/utils/apiError'

const { t } = useI18n()

const route = useRoute()

const newPassword = ref('')
const confirmPassword = ref('')
const isLoading = ref(false)
const success = ref(false)
const error = ref('')

const userId = ref('')
const token = ref('')

onMounted(() => {
  userId.value = route.query.userId || ''
  token.value = route.query.token || ''

  if (!userId.value || !token.value) {
    error.value = t('auth.invalid_reset_link')
  }
})

const handleSubmit = async () => {
  error.value = ''

  if (newPassword.value !== confirmPassword.value) {
    error.value = t('auth.passwords_dont_match')
    return
  }

  isLoading.value = true
  try {
    await authService.resetPassword(userId.value, token.value, newPassword.value)
    success.value = true
  } catch (err) {
    error.value = getApiErrorMessage(err, t('auth.reset_failed'))
  } finally {
    isLoading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-zinc-950 flex items-center justify-center px-4">
    <div class="w-full max-w-md">
      <!-- Logo -->
      <div class="text-center mb-10">
        <div class="inline-flex items-center gap-3 mb-4">
          <div
            class="w-12 h-12 bg-cyan-500 text-zinc-950 flex items-center justify-center rounded-lg font-black text-2xl transform -skew-x-6"
          >
            VG
          </div>
          <h1 class="text-3xl font-black text-white tracking-tight uppercase italic">
            Random<span class="text-cyan-400">Generator</span>
          </h1>
        </div>
        <p class="text-zinc-500 text-sm">{{ $t('auth.reset_password_title') }}</p>
      </div>

      <!-- Success State -->
      <div
        v-if="success"
        class="bg-zinc-900/50 border border-zinc-800 rounded-2xl p-8 text-center space-y-4 backdrop-blur-sm"
      >
        <div class="w-16 h-16 bg-green-500/10 rounded-full flex items-center justify-center mx-auto">
          <svg class="w-8 h-8 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <h2 class="text-xl font-bold text-white">{{ $t('auth.password_reset_success') }}</h2>
        <p class="text-zinc-400 text-sm">{{ $t('auth.password_reset_desc') }}</p>
        <router-link
          to="/login"
          class="inline-block mt-4 px-6 py-2.5 bg-gradient-to-r from-cyan-500 to-cyan-600 text-zinc-950 font-bold rounded-xl transition-all text-sm hover:from-cyan-400 hover:to-cyan-500"
        >
          {{ $t('auth.sign_in_now') }}
        </router-link>
      </div>

      <!-- Form -->
      <form
        v-else
        class="bg-zinc-900/50 border border-zinc-800 rounded-2xl p-8 space-y-6 backdrop-blur-sm"
        @submit.prevent="handleSubmit"
      >
        <div
          v-if="error"
          class="bg-red-500/10 border border-red-500/30 text-red-400 px-4 py-3 rounded-xl text-sm"
        >
          {{ error }}
        </div>

        <!-- New Password -->
        <div class="space-y-2">
          <label for="reset-password" class="block text-sm font-semibold text-zinc-400 uppercase tracking-wider">
            {{ $t('auth.new_password') }}
          </label>
          <input
            id="reset-password"
            v-model="newPassword"
            type="password"
            required
            autocomplete="new-password"
            :placeholder="$t('auth.min_characters')"
            class="w-full px-4 py-3 bg-zinc-800 border border-zinc-700 rounded-xl text-white placeholder-zinc-500 focus:outline-none focus:ring-2 focus:ring-fuchsia-500/50 focus:border-fuchsia-500 transition-all"
          />
        </div>

        <!-- Confirm Password -->
        <div class="space-y-2">
          <label for="reset-confirm" class="block text-sm font-semibold text-zinc-400 uppercase tracking-wider">
            {{ $t('auth.confirm_password') }}
          </label>
          <input
            id="reset-confirm"
            v-model="confirmPassword"
            type="password"
            required
            autocomplete="new-password"
            :placeholder="$t('auth.repeat_password')"
            class="w-full px-4 py-3 bg-zinc-800 border border-zinc-700 rounded-xl text-white placeholder-zinc-500 focus:outline-none focus:ring-2 focus:ring-fuchsia-500/50 focus:border-fuchsia-500 transition-all"
          />
        </div>

        <!-- Submit -->
        <button
          type="submit"
          :disabled="isLoading || (!userId && !token)"
          class="w-full py-3.5 bg-gradient-to-r from-fuchsia-500 to-fuchsia-600 hover:from-fuchsia-400 hover:to-fuchsia-500 text-zinc-950 font-bold rounded-xl uppercase tracking-wider text-sm transition-all transform hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
        >
          <span v-if="!isLoading">{{ $t('auth.reset_password') }}</span>
          <span v-else class="flex items-center justify-center gap-2">
            <svg class="animate-spin h-5 w-5" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            {{ $t('auth.resetting') }}
          </span>
        </button>
      </form>
    </div>
  </div>
</template>
