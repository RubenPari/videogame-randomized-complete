<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useI18n } from 'vue-i18n'
import { useAuthStore } from '@/stores/useAuthStore'
import { useToastStore } from '@/stores/useToastStore'

const { t } = useI18n()

const router = useRouter()
const authStore = useAuthStore()
const toastStore = useToastStore()

const email = ref('')
const password = ref('')
const isLoading = ref(false)
const error = ref('')

const handleLogin = async () => {
  error.value = ''
  isLoading.value = true
  try {
    await authStore.login(email.value, password.value)
    toastStore.showToast('Welcome back!', 'success')
    router.push('/')
  } catch (err) {
    error.value = err.response?.data?.error || t('error.generic')
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
        <p class="text-zinc-500 text-sm">{{ $t('auth.login_title') }}</p>
      </div>

      <!-- Login Form -->
      <form
        @submit.prevent="handleLogin"
        class="bg-zinc-900/50 border border-zinc-800 rounded-2xl p-8 space-y-6 backdrop-blur-sm"
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
          <label for="login-email" class="block text-sm font-semibold text-zinc-400 uppercase tracking-wider">
            Email
          </label>
          <input
            id="login-email"
            v-model="email"
            type="email"
            required
            autocomplete="email"
            placeholder="your@email.com"
            class="w-full px-4 py-3 bg-zinc-800 border border-zinc-700 rounded-xl text-white placeholder-zinc-500 focus:outline-none focus:ring-2 focus:ring-cyan-500/50 focus:border-cyan-500 transition-all"
          />
        </div>

        <!-- Password -->
        <div class="space-y-2">
          <label for="login-password" class="block text-sm font-semibold text-zinc-400 uppercase tracking-wider">
            Password
          </label>
          <input
            id="login-password"
            v-model="password"
            type="password"
            required
            autocomplete="current-password"
            placeholder="••••••••"
            class="w-full px-4 py-3 bg-zinc-800 border border-zinc-700 rounded-xl text-white placeholder-zinc-500 focus:outline-none focus:ring-2 focus:ring-cyan-500/50 focus:border-cyan-500 transition-all"
          />
        </div>

        <!-- Forgot Password -->
        <div class="text-right">
          <router-link
            to="/forgot-password"
            class="text-sm text-cyan-400 hover:text-cyan-300 transition-colors"
          >
            {{ $t('auth.forgot_password') }}
          </router-link>
        </div>

        <!-- Submit -->
        <button
          type="submit"
          :disabled="isLoading"
          class="w-full py-3.5 bg-gradient-to-r from-cyan-500 to-cyan-600 hover:from-cyan-400 hover:to-cyan-500 text-zinc-950 font-bold rounded-xl uppercase tracking-wider text-sm transition-all transform hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100"
        >
          <span v-if="!isLoading">{{ $t('auth.sign_in') }}</span>
          <span v-else class="flex items-center justify-center gap-2">
            <svg class="animate-spin h-5 w-5" viewBox="0 0 24 24">
              <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none" />
              <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
            </svg>
            {{ $t('auth.signing_in') }}
          </span>
        </button>
      </form>

      <!-- Register Link -->
      <p class="text-center mt-6 text-zinc-500 text-sm">
        {{ $t('auth.dont_have_account') }}
        <router-link to="/register" class="text-fuchsia-400 hover:text-fuchsia-300 font-semibold transition-colors">
          {{ $t('auth.create_one') }}
        </router-link>
      </p>
    </div>
  </div>
</template>
