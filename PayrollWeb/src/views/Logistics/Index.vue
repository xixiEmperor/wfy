<template>
  <div class="space-y-6">
    <PageHeader title="后勤管理">
      <template #actions>
        <el-button
          v-if="userStore.hasAnyRole(['Admin', 'HR'])"
          type="primary"
          @click="handleAdd"
        >
          新增后勤数据
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
      <el-form-item label="月份">
        <el-select
          v-model="filters.month"
          placeholder="请选择月份"
          clearable
        >
          <el-option
            v-for="month in recentMonths"
            :key="month.value"
            :label="month.label"
            :value="month.value"
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
      <el-table-column prop="employee.employeeNo" label="工号" width="100" />
      <el-table-column prop="employee.fullName" label="姓名" width="100" />
      <el-table-column prop="month" label="月份" width="100" />
      <el-table-column prop="housingDeduction" label="住宿扣款" width="120">
        <template #default="{ row }">
          ¥{{ formatMoney(row.housingDeduction) }}
        </template>
      </el-table-column>
      <el-table-column prop="mealAllowance" label="餐费补贴" width="120">
        <template #default="{ row }">
          ¥{{ formatMoney(row.mealAllowance) }}
        </template>
      </el-table-column>
      <el-table-column prop="utilitiesDeduction" label="水电扣费" width="120">
        <template #default="{ row }">
          ¥{{ formatMoney(row.utilitiesDeduction) }}
        </template>
      </el-table-column>
      <el-table-column label="净额" width="120">
        <template #default="{ row }">
          <span :class="getNetAmountClass(row)">
            ¥{{ formatMoney(row.mealAllowance - row.housingDeduction - row.utilitiesDeduction) }}
          </span>
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
      :title="isEdit ? '编辑后勤数据' : '新增后勤数据'"
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
        
        <el-form-item label="月份" prop="month">
          <el-select
            v-model="form.month"
            placeholder="请选择月份"
            class="w-full"
          >
            <el-option
              v-for="month in recentMonths"
              :key="month.value"
              :label="month.label"
              :value="month.value"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="住宿扣款" prop="housingDeduction">
          <el-input-number
            v-model="form.housingDeduction"
            :min="0"
            :precision="2"
            class="w-full"
            placeholder="元"
          />
        </el-form-item>

        <el-form-item label="餐费补贴" prop="mealAllowance">
          <el-input-number
            v-model="form.mealAllowance"
            :min="0"
            :precision="2"
            class="w-full"
            placeholder="元"
          />
        </el-form-item>

        <el-form-item label="水电扣费" prop="utilitiesDeduction">
          <el-input-number
            v-model="form.utilitiesDeduction"
            :min="0"
            :precision="2"
            class="w-full"
            placeholder="元"
          />
        </el-form-item>

        <el-form-item label="净额">
          <div class="text-lg font-semibold" :class="getFormNetAmountClass()">
            ¥{{ formatMoney(form.mealAllowance - form.housingDeduction - form.utilitiesDeduction) }}
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
      title="删除后勤数据"
      message="确定要删除这条后勤数据吗？删除后不可恢复。"
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
import { formatMoney, getCurrentMonth, getRecentMonths } from '@/utils'

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
const recentMonths = getRecentMonths()

const pagination = reactive({
  page: 1,
  pageSize: 20,
  sortBy: '',
  sortDir: 'desc'
})

const filters = reactive({
  employeeId: null,
  month: getCurrentMonth()
})

const form = reactive({
  employeeId: null,
  month: getCurrentMonth(),
  housingDeduction: 0,
  mealAllowance: 0,
  utilitiesDeduction: 0
})

const rules = {
  employeeId: [
    { required: true, message: '请选择员工', trigger: 'change' }
  ],
  month: [
    { required: true, message: '请选择月份', trigger: 'change' }
  ]
}

const getNetAmountClass = (row) => {
  const netAmount = row.mealAllowance - row.housingDeduction - row.utilitiesDeduction
  return netAmount >= 0 ? 'text-green-600' : 'text-red-600'
}

const getFormNetAmountClass = () => {
  const netAmount = form.mealAllowance - form.housingDeduction - form.utilitiesDeduction
  return netAmount >= 0 ? 'text-green-600' : 'text-red-600'
}

const loadData = async () => {
  loading.value = true
  try {
    const params = {
      ...pagination,
      ...filters
    }
    const { data } = await api.logistics.list(params)
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
  filters.month = getCurrentMonth()
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
    month: getCurrentMonth(),
    housingDeduction: 0,
    mealAllowance: 0,
    utilitiesDeduction: 0
  })
  dialogVisible.value = true
}

const handleEdit = (row) => {
  isEdit.value = true
  currentRow.value = row
  Object.assign(form, {
    employeeId: row.employeeId,
    month: row.month,
    housingDeduction: row.housingDeduction,
    mealAllowance: row.mealAllowance,
    utilitiesDeduction: row.utilitiesDeduction
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
      await api.logistics.update(currentRow.value.logisticsDataId, form)
      ElMessage.success('后勤数据更新成功')
    } else {
      await api.logistics.create(form)
      ElMessage.success('后勤数据创建成功')
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
    await api.logistics.delete(currentRow.value.logisticsDataId)
    ElMessage.success('后勤数据删除成功')
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
