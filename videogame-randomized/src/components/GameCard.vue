<script setup>
import { ref, computed, watch } from 'vue'
import { useVaultStore } from '@/stores/useVaultStore'
import { useToastStore } from '@/stores/useToastStore'
import api from '@/services/api'
import GameMediaGallery from './GameMediaGallery.vue'

const props = defineProps({
  game: { type: Object, default: null },
  description: { type: String, default: '' }
})

const vault = useVaultStore()
const toastSession = useToastStore()

// Local UI State
const screenshots = ref([])
const videos = ref([])
const youtubeVideoId = ref(null)
const isLoadingMedia = ref(false)

// Computed
const isSaved = computed(() => vault.isGameSaved(props.game?.id))

// Methods
const loadMediaData = async () => {
  if (!props.game || !props.game.id) return

  isLoadingMedia.value = true
  screenshots.value = []
  videos.value = []
  youtubeVideoId.value = null

  try {
    const [screenshotsRes, moviesRes] = await Promise.allSettled([
      api.getGameScreenshots(props.game.id),
      api.getGameMovies(props.game.id)
    ])

    if (screenshotsRes.status === 'fulfilled') {
      screenshots.value = screenshotsRes.value.data?.results || []
    }

    if (moviesRes.status === 'fulfilled') {
      videos.value = moviesRes.value.data?.results || []
    }

    // Fallback: search YouTube if no RAWG videos
    if (videos.value.length === 0 && props.game.name) {
      const videoId = await api.searchYouTubeTrailer(props.game.name)
      if (videoId) {
        youtubeVideoId.value = videoId
      }
    }
  } catch (error) {
    console.error('Failed to load media for game:', error)
  } finally {
    isLoadingMedia.value = false
  }
}

watch(() => props.game?.id, (newId) => {
  if (newId) loadMediaData()
}, { immediate: true })

const toggleSaveGame = async () => {
  if (!props.game) return

  try {
    const saved = await vault.toggleGame(props.game)
    toastSession.showToast(
      saved ? 'Game saved to vault' : 'Game removed from vault',
      'success'
    )
  } catch {
    toastSession.showToast('Vault error: transmission failed', 'error')
  }
}

const formatDate = (dateString) => {
  if (!dateString) return 'UNKNOWN'
  return new Date(dateString).toLocaleDateString('it-IT', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  }).toUpperCase()
}
</script>

<template>
  <div v-if="game" class="bg-zinc-900 border border-zinc-800 rounded-2xl overflow-hidden shadow-2xl relative group flex flex-col">
    <!-- Image Section -->
    <div class="relative h-72 md:h-96 w-full flex-shrink-0">
      <img :src="game.backgroundImage || game.background_image || '/placeholder-game.jpg'" :alt="game.name"
        class="w-full h-full object-cover transition-transform duration-1000 group-hover:scale-105" />
      <div class="absolute inset-0 bg-gradient-to-t from-zinc-900 via-zinc-900/40 to-transparent"></div>

      <!-- Rating Badge -->
      <div class="absolute top-4 right-4 flex gap-2">
        <div class="bg-zinc-950/90 backdrop-blur border border-zinc-800 px-3 py-1.5 rounded-lg flex items-center gap-1.5">
          <svg class="w-4 h-4 text-cyan-400" fill="currentColor" viewBox="0 0 20 20">
            <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z"></path>
          </svg>
          <span class="text-white font-mono font-bold">{{ Number(game.rating).toFixed(1) }}</span>
        </div>
      </div>

      <!-- Save Button -->
      <div class="absolute top-4 left-4">
        <button @click="toggleSaveGame"
          class="p-2.5 rounded-lg backdrop-blur border transition-all"
          :class="isSaved ? 'bg-fuchsia-500/20 border-fuchsia-500 text-fuchsia-400 hover:bg-fuchsia-500/30' : 'bg-zinc-950/90 border-zinc-800 text-zinc-400 hover:text-white hover:border-zinc-600'">
          <svg v-if="isSaved" class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
             <path fill-rule="evenodd" d="M3 5a2 2 0 012-2h10a2 2 0 012 2v10l-7-3.5L3 15V5z" clip-rule="evenodd"></path>
          </svg>
          <svg v-else class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
             <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 5a2 2 0 012-2h10a2 2 0 012 2v16l-7-3.5L5 21V5z"></path>
          </svg>
        </button>
      </div>

      <!-- Title & Release -->
      <div class="absolute bottom-0 left-0 w-full p-6 md:p-8">
        <div class="flex flex-wrap gap-2 mb-3">
          <span v-for="genre in game.genres?.slice(0,3)" :key="genre.id"
            class="text-[10px] font-bold uppercase tracking-widest px-2 py-1 bg-zinc-800/80 backdrop-blur border border-zinc-700 text-zinc-300 rounded">
            {{ genre.name }}
          </span>
        </div>
        <h2 class="text-3xl md:text-5xl font-black text-white leading-none mb-3 uppercase tracking-tight">{{ game.name }}</h2>
        <div class="flex items-center gap-2 text-cyan-400 text-xs font-mono font-bold tracking-wider">
          <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"></path>
          </svg>
          {{ formatDate(game.released) }}
        </div>
      </div>
    </div>

    <!-- Content Section -->
    <div class="p-6 md:p-8 space-y-8 flex-1 flex flex-col">
      <!-- Hardware -->
      <div>
        <h3 class="text-[10px] font-bold text-zinc-500 uppercase tracking-widest mb-3">{{ $t('game.platforms') }}</h3>
        <div class="flex flex-wrap gap-2">
          <span v-for="platform in game.platforms" :key="platform.platform.id"
            class="text-xs font-mono px-2.5 py-1 bg-zinc-950 border border-zinc-800 text-zinc-400 rounded-md flex items-center gap-2">
            <span class="w-1.5 h-1.5 rounded-full bg-cyan-500/50"></span>
            {{ platform.platform.name }}
          </span>
        </div>
      </div>

      <!-- Game Description -->
      <div v-if="description" class="description-card">
        <div class="flex items-center gap-2.5 mb-4">
          <div class="w-1 h-5 bg-gradient-to-b from-cyan-400 to-fuchsia-500 rounded-full"></div>
          <h3 class="text-xs font-bold text-zinc-400 uppercase tracking-[0.2em]">{{ $t('game.description') }}</h3>
        </div>
        <div class="description-content" v-html="description"></div>
      </div>

      <GameMediaGallery
        :isLoadingMedia="isLoadingMedia"
        :screenshots="screenshots"
        :videos="videos"
        :youtubeVideoId="youtubeVideoId"
      />
    </div>
  </div>
</template>

<style scoped>
.description-card {
  background: linear-gradient(135deg, rgba(24, 24, 27, 0.8), rgba(9, 9, 11, 0.6));
  border: 1px solid #27272a;
  border-radius: 1rem;
  padding: 1.5rem 1.75rem;
  position: relative;
  overflow: hidden;
}
.description-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  width: 3px;
  height: 100%;
  background: linear-gradient(to bottom, #22d3ee, #d946ef);
  border-radius: 3px;
}
.description-content {
  font-size: 0.9375rem;
  line-height: 1.8;
  color: #a1a1aa;
  font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif;
  letter-spacing: 0.01em;
}
.description-content :deep(p) {
  margin-bottom: 1rem;
}
.description-content :deep(p:last-child) {
  margin-bottom: 0;
}
.description-content :deep(strong),
.description-content :deep(b) {
  color: #f4f4f5;
  font-weight: 600;
}
.description-content :deep(a) {
  color: #22d3ee;
  text-decoration: none;
  border-bottom: 1px solid rgba(34, 211, 238, 0.3);
  transition: all 0.2s;
}
.description-content :deep(a:hover) {
  color: #67e8f9;
  border-bottom-color: #67e8f9;
}
.description-content :deep(ul),
.description-content :deep(ol) {
  padding-left: 1.5rem;
  margin-bottom: 1rem;
}
.description-content :deep(li) {
  margin-bottom: 0.4rem;
}
.description-content :deep(br) {
  content: '';
  display: block;
  margin-bottom: 0.5rem;
}
.description-content :deep(h3),
.description-content :deep(h4) {
  color: #ffffff;
  font-weight: 700;
  margin-top: 1.25rem;
  margin-bottom: 0.625rem;
  font-size: 0.9375rem;
  text-transform: uppercase;
  letter-spacing: 0.06em;
}
</style>
