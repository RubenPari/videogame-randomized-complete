<script setup>
import { ref } from 'vue'

defineProps({
  screenshots: { type: Array, default: () => [] },
  videos: { type: Array, default: () => [] },
  isLoadingMedia: { type: Boolean, default: false }
})

const activeScreenshotIndex = ref(0)
</script>

<template>
  <div v-if="isLoadingMedia" class="flex-1 flex items-center justify-center p-8">
    <div class="w-8 h-8 border-2 border-fuchsia-500 border-t-transparent rounded-full animate-spin"></div>
  </div>

  <div v-else class="flex-1 flex flex-col gap-8">
    <!-- Trailers -->
    <div v-if="videos.length > 0" class="flex flex-col gap-3">
      <h3 class="text-[10px] font-bold text-zinc-500 uppercase tracking-widest flex items-center justify-between">
        <span>Video Trailer</span>
        <span class="text-zinc-600">{{ videos.length > 1 ? videos.length + ' Videos' : '' }}</span>
      </h3>
      <div class="bg-zinc-950 border border-zinc-800 rounded-xl overflow-hidden aspect-video relative flex-shrink-0">
        <video v-if="videos[0]?.data?.max || videos[0]?.data?.['480']"
           controls playsinline
           class="absolute inset-0 w-full h-full object-contain bg-black"
           :poster="videos[0]?.preview">
           <source :src="videos[0].data.max" type="video/mp4" />
           <source :src="videos[0].data['480']" type="video/mp4" />
           Your browser does not support the video tag.
        </video>
      </div>
      <p class="text-xs text-zinc-500 font-mono">{{ videos[0]?.name }}</p>
    </div>

    <!-- Screenshots -->
    <div v-if="screenshots.length > 0" class="flex flex-col gap-3">
      <h3 class="text-[10px] font-bold text-zinc-500 uppercase tracking-widest">Image Gallery</h3>

      <div class="flex-1 bg-zinc-950 border border-zinc-800 rounded-xl overflow-hidden min-h-64 relative group">
         <img :src="screenshots[activeScreenshotIndex]?.image" class="w-full h-full object-contain absolute inset-0" />
         <!-- Controls -->
         <div class="absolute inset-x-0 bottom-0 p-3 bg-gradient-to-t from-zinc-950/90 to-transparent flex justify-between items-center opacity-0 group-hover:opacity-100 transition-opacity">
           <button @click="activeScreenshotIndex = Math.max(0, activeScreenshotIndex - 1)"
             class="p-1.5 bg-zinc-900 border border-zinc-700 text-white rounded hover:bg-zinc-800 disabled:opacity-50">
             <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"></path></svg>
           </button>
           <div class="text-[10px] font-mono font-bold text-zinc-400">
             {{ activeScreenshotIndex + 1 }} / {{ screenshots.length }}
           </div>
           <button @click="activeScreenshotIndex = Math.min(screenshots.length - 1, activeScreenshotIndex + 1)"
             class="p-1.5 bg-zinc-900 border border-zinc-700 text-white rounded hover:bg-zinc-800 disabled:opacity-50">
             <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg>
           </button>
         </div>
      </div>
      <div class="flex gap-2 overflow-x-auto custom-scrollbar pb-2">
        <button v-for="(shot, index) in screenshots" :key="shot.id"
          @click="activeScreenshotIndex = index"
          class="w-16 h-12 flex-shrink-0 rounded-lg overflow-hidden border-2 transition-colors duration-200"
          :class="activeScreenshotIndex === index ? 'border-fuchsia-500' : 'border-zinc-800 hover:border-zinc-600'">
          <img :src="shot.image" class="w-full h-full object-cover" />
        </button>
      </div>
    </div>
  </div>
</template>

<style scoped>
.custom-scrollbar::-webkit-scrollbar { width: 4px; }
.custom-scrollbar::-webkit-scrollbar-track { background: transparent; }
.custom-scrollbar::-webkit-scrollbar-thumb { background: #27272a; border-radius: 2px; }
.custom-scrollbar::-webkit-scrollbar-thumb:hover { background: #3f3f46; }
</style>
