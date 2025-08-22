<template>
  <div class="space-y-6">
    <PageHeader title="工资变动">
      <template #actions>
        <el-button
          v-if="userStore.hasAnyRole(['Admin', 'HR'])"
          type="primary"
          @click="handleAdd"
        >
          新增变动记录
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
      <el-form-item label="变动日期">
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
      <el-table-column prop="employee.employeeNo" label="工号" width="100" />
      <el-table-column prop="employee.fullName" label="姓名" width="100" />
      <el-table-column prop="changeDate" label="变动日期" width="120">
        <template #default="{ row }">
          {{ formatDate(row.changeDate) }}
        </template>
      </el-table-column>
      <el-table-column prop="oldBaseSalary" label="原基本工资" width="120">
        <template #default="{ row }">
          ¥{{ formatMoney(row.oldBaseSalary) }}
        </template>
      </el-table-column>
      <el-table-column prop="newBaseSalary" label="新基本工资" width="120">
        <template #default="{ row }">
          ¥{{ formatMoney(row.newBaseSalary) }}
        </template>
      </el-table-column>
      <el-table-column label="变动幅度" width="120">
        <template #default="{ row }">
          <span :class="getSalaryChangeClass(row)">
            {{ getSalaryChangeText(row) }}
          </span>
        </template>
      </el-table-column>
      <el-table-column prop="reason" label="变动原因" />
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
      :title="isEdit ? '编辑变动记录' : '新增变动记录'"
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
            <el-form-item label="员工" prop="employeeId">
              <el-select
                v-model="form.employeeId"
                placeholder="请选择员工"
                class="w-full"
                filterable
                @change="handleEmployeeChange"
              >
                <el-option
                  v-for="emp in employees"
                  :key="emp.employeeId"
                  :label="`${emp.employeeNo} - ${emp.fullName}`"
                  :value="emp.employeeId"
                />
              </el-select>
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="变动日期" prop="changeDate">
              <el-date-picker
                v-model="form.changeDate"
                type="date"
                placeholder="请选择变动日期"
                class="w-full"
                value-format="YYYY-MM-DD"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="12">
            <el-form-item label="原基本工资" prop="oldBaseSalary">
              <el-input-number
                v-model="form.oldBaseSalary"
                :min="0"
                :precision="2"
                class="w-full"
                :disabled="!isEdit"
              />
            </el-form-item>
          </el-col>
          <el-col :span="12">
            <el-form-item label="新基本工资" prop="newBaseSalary">
              <el-input-number
                v-model="form.newBaseSalary"
                :min="0"
                :precision="2"
                class="w-full"
              />
            </el-form-item>
          </el-col>
        </el-row>

        <el-form-item label="变动原因" prop="reason">
          <el-input
            v-model="form.reason"
            type="textarea"
            :rows="3"
            placeholder="请输入变动原因"
          />
        </el-form-item>

        <el-form-item label="变动预览">
          <div class="p-4 bg-gray-50 rounded">
            <div class="flex items-center justify-between">
              <span>变动金额：</span>
              <span :class="getPreviewChangeClass()" class="font-semibold">
                {{ getPreviewChangeText() }}
              </span>
            </div>
            <div class="flex items-center justify-between mt-2">
              <span>变动比例：</span>
              <span :class="getPreviewChangeClass()" class="font-semibold">
                {{ getPreviewPercentage() }}
              </span>
            </div>
          </div>
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
      title="删除变动记录"
      message="确定要删除这条变动记录吗？删除后不可恢复。"
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
import { useUserStore } from '@/stores/user'
import api from '@/api'
import { formatDate, formatMoney } from '@/utils'
import dayjs from 'dayjs'

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
  changeDate: dayjs().format('YYYY-MM-DD'),
  oldBaseSalary: 0,
  newBaseSalary: 0,
  reason: ''
})

const rules = {
  employeeId: [
    { required: true, message: '请选择员工', trigger: 'change' }
  ],
  changeDate: [
    { required: true, message: '请选择变动日期', trigger: 'change' }
  ],
  newBaseSalary: [
    { required: true, message: '请输入新基本工资', trigger: 'blur' }
  ],
  reason: [
    { required: true, message: '请输入变动原因', trigger: 'blur' }
  ]
}

const getSalaryChangeClass = (row) => {
  const diff = row.newBaseSalary - row.oldBaseSalary
  return diff > 0 ? 'text-green-600' : diff < 0 ? 'text-red-600' : 'text-gray-600'
}

const getSalaryChangeText = (row) => {
  const diff = row.newBaseSalary - row.oldBaseSalary
  const percentage = row.oldBaseSalary > 0 ? ((diff / row.oldBaseSalary) * 100).toFixed(1) : 0
  
  if (diff > 0) {
    return `+¥${formatMoney(diff)} (+${percentage}%)`
  } else if (diff < 0) {
    return `-¥${formatMoney(Math.abs(diff))} (${percentage}%)`
  }
  return '无变化'
}

const getPreviewChangeClass = () => {
  const diff = form.newBaseSalary - form.oldBaseSalary
  return diff > 0 ? 'text-green-600' : diff < 0 ? 'text-red-600' : 'text-gray-600'
}

const getPreviewChangeText = () => {
  const diff = form.newBaseSalary - form.oldBaseSalary
  if (diff > 0) {
    return `+¥${formatMoney(diff)}`
  } else if (diff < 0) {
    return `-¥${formatMoney(Math.abs(diff))}`
  }
  return '¥0.00'
}

const getPreviewPercentage = () => {
  const diff = form.newBaseSalary - form.oldBaseSalary
  const percentage = form.oldBaseSalary > 0 ? ((diff / form.oldBaseSalary) * 100).toFixed(1) : 0
  
  if (diff > 0) {
    return `+${percentage}%`
  } else if (diff < 0) {
    return `${percentage}%`
  }
  return '0%'
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

const handleEmployeeChange = () => {
  if (form.employeeId && !isEdit.value) {
    const employee = employees.value.find(emp => emp.employeeId === form.employeeId)
    if (employee) {
      form.oldBaseSalary = employee.baseSalary
    }
  }
}

const loadData = async () => {
  loading.value = true
  try {
    const params = {
      ...pagination,
      ...filters
    }
    const { data } = await api.salaryChanges.list(params)
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
    changeDate: dayjs().format('YYYY-MM-DD'),
    oldBaseSalary: 0,
    newBaseSalary: 0,
    reason: ''
  })
  dialogVisible.value = true
}

const handleEdit = (row) => {
  isEdit.value = true
  currentRow.value = row
  Object.assign(form, {
    employeeId: row.employeeId,
    changeDate: row.changeDate,
    oldBaseSalary: row.oldBaseSalary,
    newBaseSalary: row.newBaseSalary,
    reason: row.reason
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
      await api.salaryChanges.update(currentRow.value.salaryChangeId, form)
      ElMessage.success('变动记录更新成功')
    } else {
      await api.salaryChanges.create(form)
      ElMessage.success('变动记录创建成功')
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
    await api.salaryChanges.delete(currentRow.value.salaryChangeId)
    ElMessage.success('变动记录删除成功')
    confirmDialogRef.value.hide()
    loadData()
  } catch (error) {
    console.error('删除失败:', error)
  }
}

onMounted(() => {
  loadEmployees()
  loadData()
})
</script>
