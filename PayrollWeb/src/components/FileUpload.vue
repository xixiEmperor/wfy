<template>
  <div>
    <el-upload
      ref="uploadRef"
      :auto-upload="false"
      :accept="accept"
      :on-change="handleFileChange"
      :before-remove="handleBeforeRemove"
      :file-list="fileList"
      :limit="1"
      drag
    >
      <el-icon class="text-6xl text-gray-400">
        <UploadFilled />
      </el-icon>
      <div class="text-lg text-gray-600">
        将文件拖到此处，或<em>点击上传</em>
      </div>
      <div class="text-sm text-gray-500 mt-2">
        支持 {{ acceptText }} 格式文件
      </div>
    </el-upload>

    <div class="mt-4 flex gap-3">
      <el-button
        type="primary"
        :loading="uploading"
        :disabled="!fileList.length"
        @click="handleUpload"
      >
        开始导入
      </el-button>
      <el-button @click="handleClear">
        清空
      </el-button>
      <el-button
        v-if="showTemplate"
        link
        type="primary"
        @click="$emit('download-template')"
      >
        下载模板
      </el-button>
    </div>

    <!-- 解析结果预览 -->
    <div v-if="parsedData.length" class="mt-6">
      <h4 class="text-base font-medium mb-3">解析结果预览（前10条）</h4>
      <el-table
        :data="parsedData.slice(0, 10)"
        max-height="300"
        border
        size="small"
      >
        <el-table-column
          v-for="(column, index) in previewColumns"
          :key="index"
          :prop="column.prop"
          :label="column.label"
          min-width="120"
        />
      </el-table>
      <div class="text-sm text-gray-500 mt-2">
        共解析 {{ parsedData.length }} 条数据
      </div>
    </div>

    <!-- 导入结果 -->
    <div v-if="importResult" class="mt-6">
      <el-alert
        :type="importResult.success ? 'success' : 'error'"
        :title="importResult.message"
        show-icon
        :closable="false"
      />
      
      <div v-if="importResult.errors && importResult.errors.length" class="mt-4">
        <h4 class="text-base font-medium mb-3">错误详情</h4>
        <el-table
          :data="importResult.errors"
          max-height="200"
          border
          size="small"
        >
          <el-table-column prop="row" label="行号" width="80" />
          <el-table-column prop="error" label="错误信息" />
        </el-table>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { UploadFilled } from '@element-plus/icons-vue'
import { parseUploadFile } from '@/utils'

const props = defineProps({
  accept: {
    type: String,
    default: '.xlsx,.xls,.csv'
  },
  showTemplate: {
    type: Boolean,
    default: true
  },
  columns: {
    type: Array,
    default: () => []
  }
})

const emit = defineEmits(['upload', 'download-template'])

const uploadRef = ref()
const fileList = ref([])
const parsedData = ref([])
const uploading = ref(false)
const importResult = ref(null)

const acceptText = computed(() => {
  return props.accept.replace(/\./g, '').toUpperCase()
})

const previewColumns = computed(() => {
  if (props.columns.length) {
    return props.columns
  }
  
  if (parsedData.value.length) {
    return Object.keys(parsedData.value[0]).map(key => ({
      prop: key,
      label: key
    }))
  }
  
  return []
})

const handleFileChange = async (file) => {
  if (!file.raw) return
  
  try {
    const data = await parseUploadFile(file.raw)
    parsedData.value = data
    importResult.value = null
    ElMessage.success(`成功解析 ${data.length} 条数据`)
  } catch (error) {
    ElMessage.error(error.message)
    handleClear()
  }
}

const handleBeforeRemove = () => {
  parsedData.value = []
  importResult.value = null
  return true
}

const handleClear = () => {
  uploadRef.value.clearFiles()
  fileList.value = []
  parsedData.value = []
  importResult.value = null
}

const handleUpload = async () => {
  if (!parsedData.value.length) {
    ElMessage.error('请先选择文件')
    return
  }
  
  uploading.value = true
  try {
    const result = await emit('upload', parsedData.value)
    importResult.value = result
    
    if (result.success) {
      ElMessage.success('导入成功')
      handleClear()
    } else {
      ElMessage.error('导入失败')
    }
  } catch (error) {
    ElMessage.error('导入失败')
    importResult.value = {
      success: false,
      message: '导入失败: ' + error.message
    }
  } finally {
    uploading.value = false
  }
}
</script>
