<script setup>
import { ref, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import authService from '@/services/auth'

const route = useRoute()

const isLoading = ref(true)
const success = ref(false)
const error = ref('')

onMounted(async () => {
  const userId = route.query.userId
  const token = route.query.token

  if (!userId || !token) {
    error.value = 'Invalid confirmation link.'
    isLoading.value = false
    return
  }

  try {
    await authService.confirmEmail(userId, token)
    success.value = true
  } catch (err) {
    error.value = err.response?.data?.error || 'Email confirmation failed. The link may have expired.'
  } finally {
    isLoading.value = false
  }
})
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
      </div>

      <!-- Loading -->
      <div
        v-if="isLoading"
        class="bg-zinc-900/50 border border-zinc-800 rounded-2xl p-8 text-center space-y-4 backdrop-blur-sm"
      >
        <svg class="animate-spin h-10 w-10 text-cyan-400 mx-auto" viewBox="0 0 24 24">
          <circle class="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" stroke-width="4" fill="none" />
          <path class="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
        </svg>
        <p class="text-zinc-400">Confirming your email...</p>
      </div>

      <!-- Success -->
      <div
        v-else-if="success"
        class="bg-zinc-900/50 border border-zinc-800 rounded-2xl p-8 text-center space-y-4 backdrop-blur-sm"
      >
        <div class="w-16 h-16 bg-green-500/10 rounded-full flex items-center justify-center mx-auto">
          <svg class="w-8 h-8 text-green-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <h2 class="text-xl font-bold text-white">Email Confirmed!</h2>
        <p class="text-zinc-400 text-sm">Your email has been verified. You can now sign in.</p>
        <router-link
          to="/login"
          class="inline-block mt-4 px-6 py-2.5 bg-gradient-to-r from-cyan-500 to-cyan-600 text-zinc-950 font-bold rounded-xl transition-all text-sm hover:from-cyan-400 hover:to-cyan-500"
        >
          Sign In
        </router-link>
      </div>

      <!-- Error -->
      <div
        v-else
        class="bg-zinc-900/50 border border-zinc-800 rounded-2xl p-8 text-center space-y-4 backdrop-blur-sm"
      >
        <div class="w-16 h-16 bg-red-500/10 rounded-full flex items-center justify-center mx-auto">
          <svg class="w-8 h-8 text-red-400" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M6 18L18 6M6 6l12 12" />
          </svg>
        </div>
        <h2 class="text-xl font-bold text-white">Confirmation Failed</h2>
        <p class="text-red-400 text-sm">{{ error }}</p>
        <router-link
          to="/login"
          class="inline-block mt-4 px-6 py-2.5 bg-zinc-800 hover:bg-zinc-700 border border-zinc-700 text-white font-semibold rounded-xl transition-all text-sm"
        >
          Back to Login
        </router-link>
      </div>
    </div>
  </div>
</template>
