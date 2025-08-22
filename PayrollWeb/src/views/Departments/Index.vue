<template>
  <div class="space-y-6">
    <PageHeader title="部门管理">
      <template #actions>
        <el-button
          v-if="userStore.hasAnyRole(['Admin', 'HR'])"
          type="primary"
          @click="handleAdd"
        >
          新增部门
        </el-button>
      </template>
    </PageHeader>

    <FilterBar :filters="filters" @search="loadData" @reset="handleReset">
      <el-form-item label="部门名称">
        <el-input
          v-model="filters.keyword"
          placeholder="请输入部门名称"
          clearable
        />
      </el-form-item>
    </FilterBar>

    <DataTable
      :data="tableData"
      :loading="loading"
      :total="total"
      v-model:current-page="pagination.page"
      v-model:page-size="pagination.pageSize"
      @refresh="loadData"
      @sort-change="handleSortChange"
      @size-change="handleSizeChange"
      @current-change="handleCurrentChange"
    >
      <el-table-column prop="departmentId" label="ID" width="80" />
      <el-table-column prop="name" label="部门名称" />
      <el-table-column prop="description" label="描述" />
      <el-table-column prop="createdAt" label="创建时间" width="180">
        <template #default="{ row }">
          {{ formatDate(row.createdAt) }}
        </template>
      </el-table-column>
      <el-table-column label="操作" width="200" fixed="right">
        <template #default="{ row }">
          <el-button
            v-if="userStore.hasAnyRole(['Admin', 'HR'])"
            type="primary"
            link
            @click="handleEdit(row)"
          >
            编辑
          </el-button>
          <el-button
            v-if="userStore.hasRole('Admin')"
            type="danger"
            link
            @click="handleDelete(row)"
          >
            删除
          </el-button>
        </template>
      </el-table-column>
    </DataTable>

    <!-- 新增/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑部门' : '新增部门'"
      width="500px"
    >
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="80px"
      >
        <el-form-item label="部门名称" prop="name">
          <el-input v-model="form.name" placeholder="请输入部门名称" />
        </el-form-item>
        <el-form-item label="描述" prop="description">
          <el-input
            v-model="form.description"
            type="textarea"
            :rows="3"
            placeholder="请输入部门描述"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">
          确定
        </el-button>
      </template>
    </el-dialog>

    <ConfirmDialog
      ref="confirmDialogRef"
      title="删除部门"
      message="确定要删除这个部门吗？删除后不可恢复。"
      @confirm="confirmDelete"
    />
  </div>
</template>

<script setup>
import { ref, reactive, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import PageHeader from '@/components/PageHeader.vue'
import DataTable from '@/components/DataTable.vue'
import FilterBar from '@/components/FilterBar.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import { useUserStore } from '@/stores/user'
import api from '@/api'
import { formatDate } from '@/utils'

const userStore = useUserStore()
const formRef = ref()
const confirmDialogRef = ref()

const loading = ref(false)
const submitting = ref(false)
const dialogVisible = ref(false)
const isEdit = ref(false)
const currentRow = ref(null)

const tableData = ref([])
const total = ref(0)

const pagination = reactive({
  page: 1,
  pageSize: 20,
  sortBy: '',
  sortDir: 'desc'
})

const filters = reactive({
  keyword: ''
})

const form = reactive({
  name: '',
  description: ''
})

const rules = {
  name: [
    { required: true, message: '请输入部门名称', trigger: 'blur' }
  ]
}

const loadData = async () => {
  loading.value = true
  try {
    const params = {
      ...pagination,
      ...filters
    }
    const { data } = await api.departments.list(params)
    if (data.success) {
      tableData.value = data.data.items
      total.value = data.data.meta.total
    }
  } catch (error) {
    console.error('加载数据失败:', error)
  } finally {
    loading.value = false
  }
}

const handleReset = () => {
  filters.keyword = ''
}

const handleSortChange = ({ prop, order }) => {
  pagination.sortBy = prop
  pagination.sortDir = order === 'ascending' ? 'asc' : 'desc'
  loadData()
}

const handleSizeChange = (size) => {
  pagination.pageSize = size
  pagination.page = 1
  loadData()
}

const handleCurrentChange = (page) => {
  pagination.page = page
  loadData()
}

const handleAdd = () => {
  isEdit.value = false
  Object.assign(form, {
    name: '',
    description: ''
  })
  dialogVisible.value = true
}

const handleEdit = (row) => {
  isEdit.value = true
  currentRow.value = row
  Object.assign(form, {
    name: row.name,
    description: row.description || ''
  })
  dialogVisible.value = true
}

const handleSubmit = async () => {
  if (!formRef.value) return
  
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  
  submitting.value = true
  try {
    if (isEdit.value) {
      await api.departments.update(currentRow.value.departmentId, form)
      ElMessage.success('部门更新成功')
    } else {
      await api.departments.create(form)
      ElMessage.success('部门创建成功')
    }
    
    dialogVisible.value = false
    loadData()
  } catch (error) {
    console.error('操作失败:', error)
  } finally {
    submitting.value = false
  }
}

const handleDelete = (row) => {
  currentRow.value = row
  confirmDialogRef.value.show()
}

const confirmDelete = async () => {
  try {
    await api.departments.delete(currentRow.value.departmentId)
    ElMessage.success('部门删除成功')
    confirmDialogRef.value.hide()
    loadData()
  } catch (error) {
    console.error('删除失败:', error)
  }
}

onMounted(() => {
  loadData()
})
</script>
