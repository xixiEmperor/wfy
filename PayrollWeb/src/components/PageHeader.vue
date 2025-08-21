<template>
  <div class="toolbar">
    <div class="flex items-center gap-4">
      <h2 class="text-xl font-semibold text-gray-800">{{ title }}</h2>
      <slot name="title-extra" />
    </div>
    <div class="flex items-center gap-3">
      <slot name="actions" />
    </div>
  </div>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    required: true
  }
})
</script>
