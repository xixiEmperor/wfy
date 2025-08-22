<template>
  <div class="space-y-6">
    <PageHeader title="车间管理">
      <template #actions>
        <el-button
          v-if="userStore.hasAnyRole(['Admin', 'HR'])"
          type="primary"
          @click="handleAdd"
        >
          新增车间
        </el-button>
      </template>
    </PageHeader>

    <FilterBar :filters="filters" @search="loadData" @reset="handleReset">
      <el-form-item label="车间名称">
        <el-input
          v-model="filters.keyword"
          placeholder="请输入车间名称"
          clearable
        />
      </el-form-item>
      <el-form-item label="所属部门">
        <el-select
          v-model="filters.departmentId"
          placeholder="请选择部门"
          clearable
        >
          <el-option
            v-for="dept in departments"
            :key="dept.departmentId"
            :label="dept.name"
            :value="dept.departmentId"
          />
        </el-select>
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
      <el-table-column prop="workshopId" label="ID" width="80" />
      <el-table-column prop="name" label="车间名称" />
      <el-table-column prop="department.name" label="所属部门" />
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
      :title="isEdit ? '编辑车间' : '新增车间'"
      width="500px"
    >
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="80px"
      >
        <el-form-item label="车间名称" prop="name">
          <el-input v-model="form.name" placeholder="请输入车间名称" />
        </el-form-item>
        <el-form-item label="所属部门" prop="departmentId">
          <el-select
            v-model="form.departmentId"
            placeholder="请选择部门"
            class="w-full"
          >
            <el-option
              v-for="dept in departments"
              :key="dept.departmentId"
              :label="dept.name"
              :value="dept.departmentId"
            />
          </el-select>
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
      title="删除车间"
      message="确定要删除这个车间吗？删除后不可恢复。"
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
const departments = ref([])

const pagination = reactive({
  page: 1,
  pageSize: 20,
  sortBy: '',
  sortDir: 'desc'
})

const filters = reactive({
  keyword: '',
  departmentId: null
})

const form = reactive({
  name: '',
  departmentId: null
})

const rules = {
  name: [
    { required: true, message: '请输入车间名称', trigger: 'blur' }
  ],
  departmentId: [
    { required: true, message: '请选择部门', trigger: 'change' }
  ]
}

const loadData = async () => {
  loading.value = true
  try {
    const params = {
      ...pagination,
      ...filters
    }
    const { data } = await api.workshops.list(params)
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

const loadDepartments = async () => {
  try {
    const { data } = await api.departments.list({ page: 1, pageSize: 1000 })
    if (data.success) {
      departments.value = data.data.items
    }
  } catch (error) {
    console.error('加载部门数据失败:', error)
  }
}

const handleReset = () => {
  filters.keyword = ''
  filters.departmentId = null
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
    departmentId: null
  })
  dialogVisible.value = true
}

const handleEdit = (row) => {
  isEdit.value = true
  currentRow.value = row
  Object.assign(form, {
    name: row.name,
    departmentId: row.departmentId
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
      await api.workshops.update(currentRow.value.workshopId, form)
      ElMessage.success('车间更新成功')
    } else {
      await api.workshops.create(form)
      ElMessage.success('车间创建成功')
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
    await api.workshops.delete(currentRow.value.workshopId)
    ElMessage.success('车间删除成功')
    confirmDialogRef.value.hide()
    loadData()
  } catch (error) {
    console.error('删除失败:', error)
  }
}

onMounted(() => {
  loadDepartments()
  loadData()
})
</script>
