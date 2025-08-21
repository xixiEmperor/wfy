<template>
  <el-dialog
    v-model="visible"
    :title="title"
    width="400px"
    :before-close="handleClose"
  >
    <div class="flex items-center gap-3">
      <el-icon class="text-orange-500 text-xl">
        <WarningFilled />
      </el-icon>
      <span>{{ message }}</span>
    </div>
    
    <template #footer>
      <el-button @click="handleClose">
        取消
      </el-button>
      <el-button
        type="primary"
        :loading="loading"
        @click="handleConfirm"
      >
        确定
      </el-button>
    </template>
  </el-dialog>
</template>

<script setup>
import { ref } from 'vue'
import { WarningFilled } from '@element-plus/icons-vue'

const props = defineProps({
  title: {
    type: String,
    default: '确认操作'
  },
  message: {
    type: String,
    required: true
  },
  loading: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['confirm', 'cancel'])

const visible = ref(false)

const show = () => {
  visible.value = true
}

const hide = () => {
  visible.value = false
}

const handleClose = () => {
  hide()
  emit('cancel')
}

const handleConfirm = () => {
  emit('confirm')
}

defineExpose({
  show,
  hide
})
</script>
