<script setup>
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/useAuthStore'
import { useToastStore } from '@/stores/useToastStore'
import { getApiErrorMessage } from '@/utils/apiError'

const { t } = useI18n()

const authStore = useAuthStore()
const toastStore = useToastStore()

const email = ref('')
const password = ref('')
const confirmPassword = ref('')
const isLoading = ref(false)
const error = ref('')
const success = ref(false)

const handleRegister = async () => {
  error.value = ''
  isLoading.value = true
  try {
    await authStore.register(email.value, password.value, confirmPassword.value)
    success.value = true
    toastStore.showToast('Registration successful!', 'success')
  } catch (err) {
    error.value = getApiErrorMessage(err, t('error.generic'))
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
        <p class="text-zinc-500 text-sm">{{ $t('auth.register_title') }}</p>
      </div>

      <!-- Success State -->
      <div
        v-if="success"
        class="bg-zinc-900/50 border border-zinc-800 rounded-2xl p-8 text-center space-y-4 backdrop-blur-sm"
      >
        <div class="w-16 h-16 bg-cyan-500/10 rounded-full flex items-center justify-center mx-auto">
          <svg class="w-8 h-8 text-cyan-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
          </svg>
        </div>
        <h2 class="text-xl font-bold text-white">{{ $t('auth.check_email') }}</h2>
        <p class="text-zinc-400 text-sm">
          {{ $t('auth.confirmation_sent', { email }) }}
        </p>
        <router-link
          to="/login"
          class="inline-block mt-4 px-6 py-2.5 bg-zinc-800 hover:bg-zinc-700 border border-zinc-700 text-white font-semibold rounded-xl transition-all text-sm"
        >
          {{ $t('auth.back_to_login') }}
        </router-link>
      </div>

      <!-- Register Form -->
      <form
        v-else
        class="bg-zinc-900/50 border border-zinc-800 rounded-2xl p-8 space-y-6 backdrop-blur-sm"
        @submit.prevent="handleRegister"
      >
        <!-- Error Message -->
        <div
          v-if="error"
          class="bg-red-500/10 border border-red-500/30 text-red-400 px-4 py-3 rounded-xl text-sm"
        >
          {{ error }}
        </div>

        <!-- Email -->
        <div class="space-y-2">
          <label for="register-email" class="block text-sm font-semibold text-zinc-400 uppercase tracking-wider">
            Email
          </label>
          <input
            id="register-email"
            v-model="email"
            type="email"
            required
            autocomplete="email"
            placeholder="your@email.com"
            class="w-full px-4 py-3 bg-zinc-800 border border-zinc-700 rounded-xl text-white placeholder-zinc-500 focus:outline-none focus:ring-2 focus:ring-fuchsia-500/50 focus:border-fuchsia-500 transition-all"
          />
        </div>

        <!-- Password -->
        <div class="space-y-2">
          <label for="register-password" class="block text-sm font-semibold text-zinc-400 uppercase tracking-wider">
            Password
          </label>
          <input
            id="register-password"
            v-model="password"
            type="password"
            required
            autocomplete="new-password"
            placeholder="Min. 6 characters"
            class="w-full px-4 py-3 bg-zinc-800 border border-zinc-700 rounded-xl text-white placeholder-zinc-500 focus:outline-none focus:ring-2 focus:ring-fuchsia-500/50 focus:border-fuchsia-500 transition-all"
          />
        </div>

        <!-- Confirm Password -->
        <div class="space-y-2">
          <label for="register-confirm" class="block text-sm font-semibold text-zinc-400 uppercase tracking-wider">
            Confirm Password
          </label>
          <input
            id="register-confirm"
            v-model="confirmPassword"
            type="password"
            required
            autocomplete="new-password"
            placeholder="Repeat password"
            class="w-full px-4 py-3 bg-zinc-800 border border-zinc-700 rounded-xl text-white placeholder-zinc-500 focus:outline-none focus:ring-2 focus:ring-fuchsia-500/50 focus:border-fuchsia-500 transition-all"
          />
        </div>

        <!-- Submit -->
        <button
          type="submit"
          :disabled="isLoading"
          class="w-full py-3.5 bg-gradient-to-r from-fuchsia-500 to-fuchsia-600 hover:from-fuchsia-400 hover:to-fuchsia-500 text-zinc-950 font-bold rounded-xl uppercase tracking-wider text-sm transition-all transform hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
        >
          <span v-if="!isLoading">{{ $t('auth.create_account') }}</span>
          <span v-else class="flex items-center justify-center gap-2">
            <svg class="animate-spin h-5 w-5" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            {{ $t('auth.creating_account') }}
          </span>
        </button>
      </form>

      <!-- Login Link -->
      <p class="text-center mt-6 text-zinc-500 text-sm">
        {{ $t('auth.already_have_account') }}
        <router-link to="/login" class="text-cyan-400 hover:text-cyan-300 font-semibold transition-colors">
          {{ $t('auth.sign_in_link') }}
        </router-link>
      </p>
    </div>
  </div>
</template>
