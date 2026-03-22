<script setup>
import { ref } from 'vue'
import { useI18n } from 'vue-i18n'
import { useToastStore } from '@/stores/useToastStore'
import authService from '@/services/auth'

defineProps({
  show: {
    type: Boolean,
    required: true,
  },
})

const emit = defineEmits(['close'])

const { t } = useI18n()
const toastStore = useToastStore()

const currentPassword = ref('')
const newPassword = ref('')
const loading = ref(false)
const error = ref('')

const resetForm = () => {
  currentPassword.value = ''
  newPassword.value = ''
  error.value = ''
}

const handleSubmit = async () => {
  error.value = ''
  loading.value = true
  try {
    await authService.changePassword(currentPassword.value, newPassword.value)
    toastStore.showToast(t('home.password_changed'), 'success')
    emit('close')
  } catch (err) {
    error.value = err.response?.data?.error || t('home.password_change_error')
  } finally {
    loading.value = false
  }
}

const handleClose = () => {
  resetForm()
  emit('close')
}
</script>

<template>
  <Teleport to="body">
    <div v-if="show" class="fixed inset-0 z-[100] flex items-center justify-center">
      <div class="absolute inset-0 bg-black/60 backdrop-blur-sm" @click="handleClose"></div>
      <div class="relative bg-zinc-900 border border-zinc-800 rounded-2xl p-6 w-full max-w-sm mx-4 space-y-5">
        <h3 class="text-lg font-bold text-white">{{ $t('home.change_password') }}</h3>

        <div
v-if="error"
          class="bg-red-500/10 border border-red-500/30 text-red-400 px-3 py-2 rounded-lg text-sm">
          {{ error }}
        </div>

        <div class="space-y-2">
          <label class="block text-xs font-semibold text-zinc-400 uppercase tracking-wider">{{ $t('auth.current_password') }}</label>
          <input
v-model="currentPassword" type="password" autocomplete="current-password"
            class="w-full px-3 py-2.5 bg-zinc-800 border border-zinc-700 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-cyan-500/50 text-sm" />
        </div>

        <div class="space-y-2">
          <label class="block text-xs font-semibold text-zinc-400 uppercase tracking-wider">{{ $t('auth.new_password') }}</label>
          <input
v-model="newPassword" type="password" autocomplete="new-password" :placeholder="$t('auth.min_characters')"
            class="w-full px-3 py-2.5 bg-zinc-800 border border-zinc-700 rounded-lg text-white focus:outline-none focus:ring-2 focus:ring-cyan-500/50 text-sm" />
        </div>

        <div class="flex gap-3">
          <button
class="flex-1 py-2.5 bg-zinc-800 hover:bg-zinc-700 text-white font-semibold rounded-lg text-sm transition-all"
            @click="handleClose">
            {{ $t('auth.back_to_login').replace('Back to ', '') }}
          </button>
          <button
:disabled="loading" class="flex-1 py-2.5 bg-cyan-500 hover:bg-cyan-400 text-zinc-950 font-bold rounded-lg text-sm transition-all disabled:opacity-50"
            @click="handleSubmit">
            <span v-if="!loading">{{ $t('home.change_password') }}</span>
            <span v-else>{{ $t('history.saving') }}</span>
          </button>
        </div>
      </div>
    </div>
  </Teleport>
</template>
