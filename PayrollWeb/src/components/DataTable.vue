<template>
  <div class="page-card">
    <div class="toolbar" v-if="showToolbar">
      <div class="flex items-center gap-3">
        <slot name="toolbar-left" />
      </div>
      <div class="flex items-center gap-3">
        <el-button
          :icon="Refresh"
          @click="$emit('refresh')"
        >
          刷新
        </el-button>
        <slot name="toolbar-right" />
      </div>
    </div>

    <el-table
      :data="data"
      :loading="loading"
      stripe
      @sort-change="handleSortChange"
      @selection-change="$emit('selection-change', $event)"
    >
      <el-table-column
        v-if="showSelection"
        type="selection"
        width="55"
      />
      <slot />
    </el-table>

    <div class="flex justify-between items-center mt-4" v-if="showPagination">
      <div class="text-sm text-gray-500">
        共 {{ total }} 条记录
      </div>
      <el-pagination
        v-model:current-page="currentPage"
        v-model:page-size="pageSize"
        :total="total"
        :page-sizes="[10, 20, 50, 100]"
        layout="sizes, prev, pager, next, jumper"
        @size-change="handleSizeChange"
        @current-change="handleCurrentChange"
      />
    </div>
  </div>
</template>

<script setup>
import { computed } from 'vue'
import { Refresh } from '@element-plus/icons-vue'

const props = defineProps({
  data: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  total: {
    type: Number,
    default: 0
  },
  currentPage: {
    type: Number,
    default: 1
  },
  pageSize: {
    type: Number,
    default: 20
  },
  showToolbar: {
    type: Boolean,
    default: true
  },
  showPagination: {
    type: Boolean,
    default: true
  },
  showSelection: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['refresh', 'sort-change', 'size-change', 'current-change', 'selection-change'])

const handleSortChange = ({ prop, order }) => {
  emit('sort-change', { prop, order })
}

const handleSizeChange = (size) => {
  emit('size-change', size)
}

const handleCurrentChange = (page) => {
  emit('current-change', page)
}
</script>
