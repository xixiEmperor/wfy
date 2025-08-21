<template>
  <div class="page-card mb-4">
    <el-form :model="filters" class="form-inline">
      <slot />
      <el-form-item>
        <el-button
          type="primary"
          @click="$emit('search', filters)"
        >
          查询
        </el-button>
        <el-button @click="handleReset">
          重置
        </el-button>
      </el-form-item>
    </el-form>
  </div>
</template>

<script setup>
const props = defineProps({
  filters: {
    type: Object,
    required: true
  },
  defaultFilters: {
    type: Object,
    default: () => ({})
  }
})

const emit = defineEmits(['search', 'reset'])

const handleReset = () => {
  Object.assign(props.filters, props.defaultFilters)
  emit('reset', props.filters)
  emit('search', props.filters)
}
</script>
