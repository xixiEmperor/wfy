<template>
  <div class="space-y-6">
    <PageHeader title="员工管理">
      <template #actions>
        <el-button
          v-if="userStore.hasAnyRole(['Admin', 'HR'])"
          @click="showImportDialog = true"
        >
          批量导入
        </el-button>
        <el-button
          v-if="userStore.hasAnyRole(['Admin', 'HR'])"
          type="primary"
          @click="handleAdd"
        >
          新增员工
        </el-button>
      </template>
    </PageHeader>

    <FilterBar :filters="filters" @search="loadData" @reset="handleReset">
      <el-form-item label="关键字">
        <el-input
          v-model="filters.keyword"
          placeholder="工号、姓名"
          clearable
        />
      </el-form-item>
      <el-form-item label="部门">
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
      <el-form-item label="车间">
        <el-select
          v-model="filters.workshopId"
          placeholder="请选择车间"
          clearable
        >
          <el-option
            v-for="workshop in workshops"
            :key="workshop.workshopId"
            :label="workshop.name"
            :value="workshop.workshopId"
          />
        </el-select>
      </el-form-item>
    </FilterBar>

    <DataTable
      :data="tableData"
      :loading="loading"
      :total="total"
      :current-page="pagination.page"
      :page-size="pagination.pageSize"
      @refresh="loadData"
      @sort-change="handleSortChange"
      @size-change="handleSizeChange"
      @current-change="handleCurrentChange"
    >
      <el-table-column prop="employeeNo" label="工号" width="120" />
      <el-table-column prop="fullName" label="姓名" width="100" />
      <el-table-column prop="gender" label="性别" width="80" />
      <el-table-column prop="department.name" label="部门" />
      <el-table-column prop="workshop.name" label="车间" />
      <el-table-column prop="hireDate" label="入职日期" width="120">
        <template #default="{ row }">
          {{ formatDate(row.hireDate) }}
        </template>
      </el-table-column>
      <el-table-column prop="baseSalary" label="基本工资" width="120">
        <template #default="{ row }">
          ¥{{ formatMoney(row.baseSalary) }}
        </template>
      </el-table-column>
      <el-table-column prop="isActive" label="状态" width="80">
        <template #default="{ row }">
          <el-tag :type="row.isActive ? 'success' : 'danger'">
            {{ row.isActive ? '在职' : '离职' }}
          </el-tag>
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
      :title="isEdit ? '编辑员工' : '新增员工'"
      width="600px"
    >
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="100px"
      >
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="工号" prop="employeeNo">
              <el-input v-model="form.employeeNo" placeholder="请输入工号" />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="姓名" prop="fullName">
              <el-input v-model="form.fullName" placeholder="请输入姓名" />
            </el-form-item>
          </el-col>
        </el-row>
        
        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="性别" prop="gender">
              <el-select v-model="form.gender" placeholder="请选择性别" class="w-full">
                <el-option
                  v-for="option in genderOptions"
                  :key="option.value"
                  :label="option.label"
                  :value="option.value"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="入职日期" prop="hireDate">
              <el-date-picker
                v-model="form.hireDate"
                type="date"
                placeholder="请选择入职日期"
                class="w-full"
                value-format="YYYY-MM-DD"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="部门" prop="departmentId">
              <el-select
                v-model="form.departmentId"
                placeholder="请选择部门"
                class="w-full"
                @change="handleDepartmentChange"
              >
                <el-option
                  v-for="dept in departments"
                  :key="dept.departmentId"
                  :label="dept.name"
                  :value="dept.departmentId"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="车间" prop="workshopId">
              <el-select
                v-model="form.workshopId"
                placeholder="请选择车间"
                class="w-full"
              >
                <el-option
                  v-for="workshop in filteredWorkshops"
                  :key="workshop.workshopId"
                  :label="workshop.name"
                  :value="workshop.workshopId"
                />
              </el-select>
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="基本工资" prop="baseSalary">
              <el-input-number
                v-model="form.baseSalary"
                :min="0"
                :precision="2"
                class="w-full"
                placeholder="请输入基本工资"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="状态" prop="isActive">
              <el-switch
                v-model="form.isActive"
                active-text="在职"
                inactive-text="离职"
              />
            </el-form-item>
          </el-col>
        </el-row>
      </el-form>

      <template #footer>
        <el-button @click="dialogVisible = false">取消</el-button>
        <el-button type="primary" :loading="submitting" @click="handleSubmit">
          确定
        </el-button>
      </template>
    </el-dialog>

    <!-- 批量导入对话框 -->
    <el-dialog
      v-model="showImportDialog"
      title="批量导入员工"
      width="800px"
    >
      <FileUpload
        :columns="importColumns"
        @upload="handleImport"
        @download-template="handleDownloadTemplate"
      />
    </el-dialog>

    <ConfirmDialog
      ref="confirmDialogRef"
      title="删除员工"
      message="确定要删除这个员工吗？删除后不可恢复。"
      @confirm="confirmDelete"
    />
  </div>
</template>

<script setup>
import { ref, reactive, computed, onMounted } from 'vue'
import { ElMessage } from 'element-plus'
import PageHeader from '@/components/PageHeader.vue'
import DataTable from '@/components/DataTable.vue'
import FilterBar from '@/components/FilterBar.vue'
import ConfirmDialog from '@/components/ConfirmDialog.vue'
import FileUpload from '@/components/FileUpload.vue'
import { useUserStore } from '@/stores/user'
import api from '@/api'
import { formatDate, formatMoney, genderOptions, downloadTemplate } from '@/utils'

const userStore = useUserStore()
const formRef = ref()
const confirmDialogRef = ref()

const loading = ref(false)
const submitting = ref(false)
const dialogVisible = ref(false)
const showImportDialog = ref(false)
const isEdit = ref(false)
const currentRow = ref(null)

const tableData = ref([])
const total = ref(0)
const departments = ref([])
const workshops = ref([])

const pagination = reactive({
  page: 1,
  pageSize: 20,
  sortBy: '',
  sortDir: 'desc'
})

const filters = reactive({
  keyword: '',
  departmentId: null,
  workshopId: null
})

const form = reactive({
  employeeNo: '',
  fullName: '',
  gender: '',
  departmentId: null,
  workshopId: null,
  hireDate: '',
  baseSalary: 0,
  isActive: true
})

const importColumns = [
  { prop: 'employeeNo', label: '工号', example: 'E001' },
  { prop: 'fullName', label: '姓名', example: '张三' },
  { prop: 'gender', label: '性别', example: '男' },
  { prop: 'departmentName', label: '部门名称', example: '生产部' },
  { prop: 'workshopName', label: '车间名称', example: '一车间' },
  { prop: 'hireDate', label: '入职日期', example: '2024-01-01' },
  { prop: 'baseSalary', label: '基本工资', example: '5000' }
]

const rules = {
  employeeNo: [
    { required: true, message: '请输入工号', trigger: 'blur' }
  ],
  fullName: [
    { required: true, message: '请输入姓名', trigger: 'blur' }
  ],
  gender: [
    { required: true, message: '请选择性别', trigger: 'change' }
  ],
  departmentId: [
    { required: true, message: '请选择部门', trigger: 'change' }
  ],
  hireDate: [
    { required: true, message: '请选择入职日期', trigger: 'change' }
  ],
  baseSalary: [
    { required: true, message: '请输入基本工资', trigger: 'blur' }
  ]
}

const filteredWorkshops = computed(() => {
  if (!form.departmentId) return []
  return workshops.value.filter(w => w.departmentId === form.departmentId)
})

const loadData = async () => {
  loading.value = true
  try {
    const params = {
      ...pagination,
      ...filters
    }
    const { data } = await api.employees.list(params)
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

const loadWorkshops = async () => {
  try {
    const { data } = await api.workshops.list({ page: 1, pageSize: 1000 })
    if (data.success) {
      workshops.value = data.data.items
    }
  } catch (error) {
    console.error('加载车间数据失败:', error)
  }
}

const handleReset = () => {
  filters.keyword = ''
  filters.departmentId = null
  filters.workshopId = null
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
    employeeNo: '',
    fullName: '',
    gender: '',
    departmentId: null,
    workshopId: null,
    hireDate: '',
    baseSalary: 0,
    isActive: true
  })
  dialogVisible.value = true
}

const handleEdit = (row) => {
  isEdit.value = true
  currentRow.value = row
  Object.assign(form, {
    employeeNo: row.employeeNo,
    fullName: row.fullName,
    gender: row.gender,
    departmentId: row.departmentId,
    workshopId: row.workshopId,
    hireDate: row.hireDate,
    baseSalary: row.baseSalary,
    isActive: row.isActive
  })
  dialogVisible.value = true
}

const handleDepartmentChange = () => {
  form.workshopId = null
}

const handleSubmit = async () => {
  if (!formRef.value) return
  
  const valid = await formRef.value.validate().catch(() => false)
  if (!valid) return
  
  submitting.value = true
  try {
    if (isEdit.value) {
      await api.employees.update(currentRow.value.employeeId, form)
      ElMessage.success('员工更新成功')
    } else {
      await api.employees.create(form)
      ElMessage.success('员工创建成功')
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
    await api.employees.delete(currentRow.value.employeeId)
    ElMessage.success('员工删除成功')
    confirmDialogRef.value.hide()
    loadData()
  } catch (error) {
    console.error('删除失败:', error)
  }
}

const handleDownloadTemplate = () => {
  downloadTemplate(importColumns, '员工导入模板')
}

const handleImport = async (data) => {
  try {
    // 这里应该调用后端批量导入接口
    // 暂时模拟成功
    ElMessage.success(`成功导入 ${data.length} 条员工数据`)
    showImportDialog.value = false
    loadData()
    return { success: true, message: '导入成功' }
  } catch (error) {
    return { success: false, message: '导入失败: ' + error.message }
  }
}

onMounted(() => {
  loadDepartments()
  loadWorkshops()
  loadData()
})
</script>
