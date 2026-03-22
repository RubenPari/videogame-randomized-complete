<script setup>
import { ref } from 'vue'

defineProps({
  screenshots: { type: Array, default: () => [] },
  videos: { type: Array, default: () => [] },
  youtubeVideoId: { type: String, default: null },
  isLoadingMedia: { type: Boolean, default: false }
})

const activeScreenshotIndex = ref(0)
</script>

<template>
  <div v-if="isLoadingMedia" class="flex-1 flex items-center justify-center p-8">
    <div class="w-8 h-8 border-2 border-fuchsia-500 border-t-transparent rounded-full animate-spin"></div>
  </div>

  <div v-else class="flex-1 flex flex-col gap-8">
    <!-- RAWG Trailers -->
    <div v-if="videos.length > 0" class="flex flex-col gap-3">
      <h3 class="text-[10px] font-bold text-zinc-500 uppercase tracking-widest flex items-center justify-between">
        <span>{{ $t('gallery.video_trailer') }}</span>
        <span class="text-zinc-600">{{ videos.length > 1 ? videos.length + ' ' + $t('gallery.videos') : '' }}</span>
      </h3>
      <div class="bg-zinc-950 border border-zinc-800 rounded-xl overflow-hidden aspect-video relative flex-shrink-0">
        <video
v-if="videos[0]?.data?.max || videos[0]?.data?.['480']"
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

    <!-- YouTube Trailer Fallback -->
    <div v-else-if="youtubeVideoId" class="flex flex-col gap-3">
      <h3 class="text-[10px] font-bold text-zinc-500 uppercase tracking-widest flex items-center gap-2">
        <svg class="w-4 h-4 text-red-500" viewBox="0 0 24 24" fill="currentColor">
          <path d="M23.498 6.186a3.016 3.016 0 0 0-2.122-2.136C19.505 3.545 12 3.545 12 3.545s-7.505 0-9.377.505A3.017 3.017 0 0 0 .502 6.186C0 8.07 0 12 0 12s0 3.93.502 5.814a3.016 3.016 0 0 0 2.122 2.136c1.871.505 9.376.505 9.376.505s7.505 0 9.377-.505a3.015 3.015 0 0 0 2.122-2.136C24 15.93 24 12 24 12s0-3.93-.502-5.814z"/>
          <path d="M9.545 15.568V8.432L15.818 12l-6.273 3.568z" fill="white"/>
        </svg>
        <span>{{ $t('gallery.youtube_trailer') }}</span>
      </h3>
      <div class="bg-zinc-950 border border-zinc-800 rounded-xl overflow-hidden aspect-video relative flex-shrink-0">
        <iframe
          :src="`https://www.youtube.com/embed/${youtubeVideoId}?rel=0`"
          class="absolute inset-0 w-full h-full"
          frameborder="0"
          allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture"
          allowfullscreen
        ></iframe>
      </div>
    </div>

    <!-- Screenshots -->
    <div v-if="screenshots.length > 0" class="flex flex-col gap-3">
      <h3 class="text-[10px] font-bold text-zinc-500 uppercase tracking-widest">{{ $t('gallery.image_gallery') }}</h3>

      <div class="flex-1 bg-zinc-950 border border-zinc-800 rounded-xl overflow-hidden min-h-64 relative group">
         <img :src="screenshots[activeScreenshotIndex]?.image" class="w-full h-full object-contain absolute inset-0" />
         <!-- Controls -->
         <div class="absolute inset-x-0 bottom-0 p-3 bg-gradient-to-t from-zinc-950/90 to-transparent flex justify-between items-center opacity-0 group-hover:opacity-100 transition-opacity">
           <button
class="p-1.5 bg-zinc-900 border border-zinc-700 text-white rounded hover:bg-zinc-800 disabled:opacity-50"
             @click="activeScreenshotIndex = Math.max(0, activeScreenshotIndex - 1)">
             <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15 19l-7-7 7-7"></path></svg>
           </button>
           <div class="text-[10px] font-mono font-bold text-zinc-400">
             {{ activeScreenshotIndex + 1 }} / {{ screenshots.length }}
           </div>
           <button
class="p-1.5 bg-zinc-900 border border-zinc-700 text-white rounded hover:bg-zinc-800 disabled:opacity-50"
             @click="activeScreenshotIndex = Math.min(screenshots.length - 1, activeScreenshotIndex + 1)">
             <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 5l7 7-7 7"></path></svg>
           </button>
         </div>
      </div>
      <div class="flex gap-2 overflow-x-auto custom-scrollbar pb-2">
        <button
v-for="(shot, index) in screenshots" :key="shot.id"
          class="w-16 h-12 flex-shrink-0 rounded-lg overflow-hidden border-2 transition-colors duration-200"
          :class="activeScreenshotIndex === index ? 'border-fuchsia-500' : 'border-zinc-800 hover:border-zinc-600'"
          @click="activeScreenshotIndex = index">
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
