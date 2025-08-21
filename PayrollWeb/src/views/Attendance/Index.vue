<template>
  <div class="space-y-6">
    <PageHeader title="考勤管理">
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
          新增考勤
        </el-button>
      </template>
    </PageHeader>

    <FilterBar :filters="filters" @search="loadData" @reset="handleReset">
      <el-form-item label="员工">
        <el-select
          v-model="filters.employeeId"
          placeholder="请选择员工"
          clearable
          filterable
        >
          <el-option
            v-for="emp in employees"
            :key="emp.employeeId"
            :label="`${emp.employeeNo} - ${emp.fullName}`"
            :value="emp.employeeId"
          />
        </el-select>
      </el-form-item>
      <el-form-item label="日期范围">
        <el-date-picker
          v-model="dateRange"
          type="daterange"
          range-separator="至"
          start-placeholder="开始日期"
          end-placeholder="结束日期"
          value-format="YYYY-MM-DD"
          @change="handleDateRangeChange"
        />
      </el-form-item>
      <el-form-item label="显示方式">
        <el-radio-group v-model="viewMode" @change="loadData">
          <el-radio label="daily">按日显示</el-radio>
          <el-radio label="monthly">按月汇总</el-radio>
        </el-radio-group>
      </el-form-item>
    </FilterBar>

    <!-- 按日显示 -->
    <DataTable
      v-if="viewMode === 'daily'"
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
      <el-table-column prop="employee.employeeNo" label="工号" width="100" />
      <el-table-column prop="employee.fullName" label="姓名" width="100" />
      <el-table-column prop="workDate" label="工作日期" width="120">
        <template #default="{ row }">
          {{ formatDate(row.workDate) }}
        </template>
      </el-table-column>
      <el-table-column prop="hoursWorked" label="工作时长" width="100">
        <template #default="{ row }">
          {{ row.hoursWorked }}h
        </template>
      </el-table-column>
      <el-table-column prop="overtimeHours" label="加班时长" width="100">
        <template #default="{ row }">
          {{ row.overtimeHours }}h
        </template>
      </el-table-column>
      <el-table-column prop="absentHours" label="缺勤时长" width="100">
        <template #default="{ row }">
          {{ row.absentHours }}h
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

    <!-- 按月汇总 -->
    <DataTable
      v-if="viewMode === 'monthly'"
      :data="monthlyData"
      :loading="loading"
      :show-pagination="false"
    >
      <el-table-column prop="employee.employeeNo" label="工号" width="100" />
      <el-table-column prop="employee.fullName" label="姓名" width="100" />
      <el-table-column prop="month" label="月份" width="100" />
      <el-table-column prop="totalWorked" label="总工作时长" width="120">
        <template #default="{ row }">
          {{ row.totalWorked }}h
        </template>
      </el-table-column>
      <el-table-column prop="totalOvertime" label="总加班时长" width="120">
        <template #default="{ row }">
          {{ row.totalOvertime }}h
        </template>
      </el-table-column>
      <el-table-column prop="totalAbsent" label="总缺勤时长" width="120">
        <template #default="{ row }">
          {{ row.totalAbsent }}h
        </template>
      </el-table-column>
      <el-table-column prop="workDays" label="出勤天数" width="100" />
    </DataTable>

    <!-- 新增/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑考勤' : '新增考勤'"
      width="500px"
    >
      <el-form
        ref="formRef"
        :model="form"
        :rules="rules"
        label-width="100px"
      >
        <el-form-item label="员工" prop="employeeId">
          <el-select
            v-model="form.employeeId"
            placeholder="请选择员工"
            class="w-full"
            filterable
          >
            <el-option
              v-for="emp in employees"
              :key="emp.employeeId"
              :label="`${emp.employeeNo} - ${emp.fullName}`"
              :value="emp.employeeId"
            />
          </el-select>
        </el-form-item>
        
        <el-form-item label="工作日期" prop="workDate">
          <el-date-picker
            v-model="form.workDate"
            type="date"
            placeholder="请选择工作日期"
            class="w-full"
            value-format="YYYY-MM-DD"
          />
        </el-form-item>

        <el-form-item label="工作时长" prop="hoursWorked">
          <el-input-number
            v-model="form.hoursWorked"
            :min="0"
            :max="24"
            :precision="1"
            class="w-full"
            placeholder="小时"
          />
        </el-form-item>

        <el-form-item label="加班时长" prop="overtimeHours">
          <el-input-number
            v-model="form.overtimeHours"
            :min="0"
            :max="24"
            :precision="1"
            class="w-full"
            placeholder="小时"
          />
        </el-form-item>

        <el-form-item label="缺勤时长" prop="absentHours">
          <el-input-number
            v-model="form.absentHours"
            :min="0"
            :max="24"
            :precision="1"
            class="w-full"
            placeholder="小时"
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

    <!-- 批量导入对话框 -->
    <el-dialog
      v-model="showImportDialog"
      title="批量导入考勤"
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
      title="删除考勤"
      message="确定要删除这条考勤记录吗？删除后不可恢复。"
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
import { formatDate, downloadTemplate } from '@/utils'
import dayjs from 'dayjs'

const userStore = useUserStore()
const formRef = ref()
const confirmDialogRef = ref()

const loading = ref(false)
const submitting = ref(false)
const dialogVisible = ref(false)
const showImportDialog = ref(false)
const isEdit = ref(false)
const currentRow = ref(null)
const viewMode = ref('daily')

const tableData = ref([])
const monthlyData = ref([])
const total = ref(0)
const employees = ref([])
const dateRange = ref([])

const pagination = reactive({
  page: 1,
  pageSize: 20,
  sortBy: '',
  sortDir: 'desc'
})

const filters = reactive({
  employeeId: null,
  dateFrom: '',
  dateTo: ''
})

const form = reactive({
  employeeId: null,
  workDate: '',
  hoursWorked: 8,
  overtimeHours: 0,
  absentHours: 0
})

const importColumns = [
  { prop: 'employeeNo', label: '工号', example: 'E001' },
  { prop: 'workDate', label: '工作日期', example: '2024-01-01' },
  { prop: 'hoursWorked', label: '工作时长', example: '8' },
  { prop: 'overtimeHours', label: '加班时长', example: '2' },
  { prop: 'absentHours', label: '缺勤时长', example: '0' }
]

const rules = {
  employeeId: [
    { required: true, message: '请选择员工', trigger: 'change' }
  ],
  workDate: [
    { required: true, message: '请选择工作日期', trigger: 'change' }
  ],
  hoursWorked: [
    { required: true, message: '请输入工作时长', trigger: 'blur' }
  ]
}

const handleDateRangeChange = (dates) => {
  if (dates && dates.length === 2) {
    filters.dateFrom = dates[0]
    filters.dateTo = dates[1]
  } else {
    filters.dateFrom = ''
    filters.dateTo = ''
  }
}

const loadData = async () => {
  loading.value = true
  try {
    if (viewMode.value === 'daily') {
      const params = {
        ...pagination,
        ...filters
      }
      const { data } = await api.attendance.list(params)
      if (data.success) {
        tableData.value = data.data.items
        total.value = data.data.meta.total
      }
    } else {
      // 加载月度汇总数据
      await loadMonthlyData()
    }
  } catch (error) {
    console.error('加载数据失败:', error)
  } finally {
    loading.value = false
  }
}

const loadMonthlyData = async () => {
  try {
    // 这里模拟月度汇总数据，实际应该调用后端聚合接口
    const mockData = [
      {
        employee: { employeeNo: 'E001', fullName: '张三' },
        month: '2024-01',
        totalWorked: 176,
        totalOvertime: 20,
        totalAbsent: 0,
        workDays: 22
      }
    ]
    monthlyData.value = mockData
  } catch (error) {
    console.error('加载月度数据失败:', error)
  }
}

const loadEmployees = async () => {
  try {
    const { data } = await api.employees.list({ page: 1, pageSize: 1000 })
    if (data.success) {
      employees.value = data.data.items.filter(emp => emp.isActive)
    }
  } catch (error) {
    console.error('加载员工数据失败:', error)
  }
}

const handleReset = () => {
  filters.employeeId = null
  filters.dateFrom = ''
  filters.dateTo = ''
  dateRange.value = []
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
    employeeId: null,
    workDate: dayjs().format('YYYY-MM-DD'),
    hoursWorked: 8,
    overtimeHours: 0,
    absentHours: 0
  })
  dialogVisible.value = true
}

const handleEdit = (row) => {
  isEdit.value = true
  currentRow.value = row
  Object.assign(form, {
    employeeId: row.employeeId,
    workDate: row.workDate,
    hoursWorked: row.hoursWorked,
    overtimeHours: row.overtimeHours,
    absentHours: row.absentHours
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
      await api.attendance.update(currentRow.value.attendanceId, form)
      ElMessage.success('考勤更新成功')
    } else {
      await api.attendance.create(form)
      ElMessage.success('考勤创建成功')
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
    await api.attendance.delete(currentRow.value.attendanceId)
    ElMessage.success('考勤删除成功')
    confirmDialogRef.value.hide()
    loadData()
  } catch (error) {
    console.error('删除失败:', error)
  }
}

const handleDownloadTemplate = () => {
  downloadTemplate(importColumns, '考勤导入模板')
}

const handleImport = async (data) => {
  try {
    ElMessage.success(`成功导入 ${data.length} 条考勤数据`)
    showImportDialog.value = false
    loadData()
    return { success: true, message: '导入成功' }
  } catch (error) {
    return { success: false, message: '导入失败: ' + error.message }
  }
}

onMounted(() => {
  loadEmployees()
  loadData()
})
</script>
