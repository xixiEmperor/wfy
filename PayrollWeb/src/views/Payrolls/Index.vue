<template>
  <div class="space-y-6">
    <PageHeader title="工资单管理">
      <template #actions>
        <el-button
          v-if="userStore.hasAnyRole(['Admin', 'HR'])"
          @click="showGenerateDialog = true"
        >
          生成工资单
        </el-button>
        <el-button
          v-if="userStore.hasAnyRole(['Admin', 'HR'])"
          type="primary"
          @click="handleAdd"
        >
          新增工资单
        </el-button>
      </template>
    </PageHeader>

    <FilterBar :filters="filters" @search="loadData" @reset="handleReset">
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
      <el-form-item label="状态">
        <el-select
          v-model="filters.status"
          placeholder="请选择状态"
          clearable
        >
          <el-option
            v-for="option in payrollStatusOptions"
            :key="option.value"
            :label="option.label"
            :value="option.value"
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
      <el-table-column prop="grossAmount" label="应发工资" width="120">
        <template #default="{ row }">
          ¥{{ formatMoney(row.grossAmount) }}
        </template>
      </el-table-column>
      <el-table-column prop="deductions" label="扣款" width="120">
        <template #default="{ row }">
          ¥{{ formatMoney(row.deductions) }}
        </template>
      </el-table-column>
      <el-table-column prop="netAmount" label="实发工资" width="120">
        <template #default="{ row }">
          <span class="font-semibold text-green-600">
            ¥{{ formatMoney(row.netAmount) }}
          </span>
        </template>
      </el-table-column>
      <el-table-column prop="status" label="状态" width="100">
        <template #default="{ row }">
          <el-tag :type="getPayrollStatusType(row.status)">
            {{ getPayrollStatusText(row.status) }}
          </el-tag>
        </template>
      </el-table-column>
      <el-table-column label="操作" width="300" fixed="right">
        <template #default="{ row }">
          <el-button
            type="primary"
            link
            @click="handleViewItems(row)"
          >
            查看明细
          </el-button>
          <el-button
            v-if="userStore.hasAnyRole(['Admin', 'HR']) && row.status === 0"
            type="primary"
            link
            @click="handleEdit(row)"
          >
            编辑
          </el-button>
          <el-button
            v-if="userStore.hasAnyRole(['Admin', 'HR']) && row.status === 0"
            type="warning"
            link
            @click="handleConfirm(row)"
          >
            确认
          </el-button>
          <el-button
            v-if="userStore.hasAnyRole(['Admin', 'HR']) && row.status === 1"
            type="success"
            link
            @click="handlePay(row)"
          >
            发放
          </el-button>
          <el-button
            v-if="userStore.hasRole('Admin') && row.status === 0"
            type="danger"
            link
            @click="handleDelete(row)"
          >
            删除
          </el-button>
        </template>
      </el-table-column>
    </DataTable>

    <!-- 生成工资单对话框 -->
    <el-dialog
      v-model="showGenerateDialog"
      title="生成工资单"
      width="600px"
    >
      <el-form
        ref="generateFormRef"
        :model="generateForm"
        :rules="generateRules"
        label-width="120px"
      >
        <el-form-item label="生成月份" prop="month">
          <el-select
            v-model="generateForm.month"
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

        <el-form-item label="员工范围">
          <el-select
            v-model="generateForm.employeeIds"
            placeholder="留空生成所有员工"
            class="w-full"
            multiple
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

        <el-form-item label="加班倍率" prop="overtimeFactor">
          <el-input-number
            v-model="generateForm.overtimeFactor"
            :min="1"
            :max="3"
            :precision="1"
            :step="0.1"
            class="w-full"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="showGenerateDialog = false">取消</el-button>
        <el-button
          type="primary"
          :loading="generating"
          @click="handleGenerate"
        >
          生成
        </el-button>
      </template>
    </el-dialog>

    <!-- 新增/编辑对话框 -->
    <el-dialog
      v-model="dialogVisible"
      :title="isEdit ? '编辑工资单' : '新增工资单'"
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
          </el-col>
        </el-row>

        <el-row :gutter="20">
          <el-col :span="8">
            <el-form-item label="应发工资" prop="grossAmount">
              <el-input-number
                v-model="form.grossAmount"
                :min="0"
                :precision="2"
                class="w-full"
                @change="calculateNetAmount"
              />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="扣款" prop="deductions">
              <el-input-number
                v-model="form.deductions"
                :min="0"
                :precision="2"
                class="w-full"
                @change="calculateNetAmount"
              />
            </el-form-item>
          </el-col>
          <el-col :span="8">
            <el-form-item label="实发工资">
              <div class="text-lg font-semibold text-green-600">
                ¥{{ formatMoney(form.netAmount) }}
              </div>
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

    <!-- 工资明细抽屉 -->
    <el-drawer
      v-model="showItemsDrawer"
      title="工资明细"
      size="50%"
    >
      <div class="space-y-4">
        <div class="flex justify-between items-center">
          <h3 class="text-lg font-semibold">
            {{ currentPayroll?.employee?.fullName }} - {{ currentPayroll?.month }}
          </h3>
          <el-button
            v-if="userStore.hasAnyRole(['Admin', 'HR']) && currentPayroll?.status === 0"
            type="primary"
            @click="showAddItemDialog = true"
          >
            新增明细项
          </el-button>
        </div>

        <el-table :data="payrollItems" border>
          <el-table-column prop="itemType" label="类型" width="120" />
          <el-table-column prop="itemName" label="项目" />
          <el-table-column prop="amount" label="金额" width="120">
            <template #default="{ row }">
              <span :class="row.amount >= 0 ? 'text-green-600' : 'text-red-600'">
                ¥{{ formatMoney(row.amount) }}
              </span>
            </template>
          </el-table-column>
          <el-table-column
            v-if="userStore.hasAnyRole(['Admin', 'HR']) && currentPayroll?.status === 0"
            label="操作"
            width="150"
          >
            <template #default="{ row }">
              <el-button
                type="primary"
                link
                @click="handleEditItem(row)"
              >
                编辑
              </el-button>
              <el-button
                v-if="userStore.hasRole('Admin')"
                type="danger"
                link
                @click="handleDeleteItem(row)"
              >
                删除
              </el-button>
            </template>
          </el-table-column>
        </el-table>
      </div>
    </el-drawer>

    <!-- 新增/编辑明细项对话框 -->
    <el-dialog
      v-model="showAddItemDialog"
      :title="isEditItem ? '编辑明细项' : '新增明细项'"
      width="500px"
    >
      <el-form
        ref="itemFormRef"
        :model="itemForm"
        :rules="itemRules"
        label-width="80px"
      >
        <el-form-item label="类型" prop="itemType">
          <el-select v-model="itemForm.itemType" placeholder="请选择类型" class="w-full">
            <el-option label="固定" value="Fixed" />
            <el-option label="考勤" value="Attendance" />
            <el-option label="奖金" value="Bonus" />
            <el-option label="罚款" value="Penalty" />
            <el-option label="津贴" value="Allowance" />
            <el-option label="社保" value="SocialSecurity" />
            <el-option label="后勤" value="Logistics" />
            <el-option label="其他" value="Other" />
          </el-select>
        </el-form-item>
        <el-form-item label="项目名" prop="itemName">
          <el-input v-model="itemForm.itemName" placeholder="请输入项目名称" />
        </el-form-item>
        <el-form-item label="金额" prop="amount">
          <el-input-number
            v-model="itemForm.amount"
            :precision="2"
            class="w-full"
            placeholder="正数为收入，负数为扣除"
          />
        </el-form-item>
      </el-form>

      <template #footer>
        <el-button @click="showAddItemDialog = false">取消</el-button>
        <el-button type="primary" :loading="submittingItem" @click="handleSubmitItem">
          确定
        </el-button>
      </template>
    </el-dialog>

    <ConfirmDialog
      ref="confirmDialogRef"
      :title="confirmTitle"
      :message="confirmMessage"
      @confirm="confirmAction"
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
import { formatMoney, getCurrentMonth, getRecentMonths, payrollStatusOptions, getPayrollStatusText, getPayrollStatusType } from '@/utils'

const userStore = useUserStore()
const formRef = ref()
const generateFormRef = ref()
const itemFormRef = ref()
const confirmDialogRef = ref()

const loading = ref(false)
const submitting = ref(false)
const generating = ref(false)
const submittingItem = ref(false)
const dialogVisible = ref(false)
const showGenerateDialog = ref(false)
const showItemsDrawer = ref(false)
const showAddItemDialog = ref(false)
const isEdit = ref(false)
const isEditItem = ref(false)
const currentRow = ref(null)
const currentPayroll = ref(null)
const currentItem = ref(null)
const confirmTitle = ref('')
const confirmMessage = ref('')
const confirmAction = ref(() => {})

const tableData = ref([])
const total = ref(0)
const employees = ref([])
const departments = ref([])
const workshops = ref([])
const payrollItems = ref([])
const recentMonths = getRecentMonths()

const pagination = reactive({
  page: 1,
  pageSize: 20,
  sortBy: '',
  sortDir: 'desc'
})

const filters = reactive({
  departmentId: null,
  workshopId: null,
  month: getCurrentMonth(),
  status: null
})

const generateForm = reactive({
  month: getCurrentMonth(),
  employeeIds: [],
  overtimeFactor: 1.5
})

const form = reactive({
  employeeId: null,
  month: getCurrentMonth(),
  grossAmount: 0,
  deductions: 0,
  netAmount: 0
})

const itemForm = reactive({
  itemType: '',
  itemName: '',
  amount: 0
})

const generateRules = {
  month: [
    { required: true, message: '请选择月份', trigger: 'change' }
  ],
  overtimeFactor: [
    { required: true, message: '请输入加班倍率', trigger: 'blur' }
  ]
}

const rules = {
  employeeId: [
    { required: true, message: '请选择员工', trigger: 'change' }
  ],
  month: [
    { required: true, message: '请选择月份', trigger: 'change' }
  ],
  grossAmount: [
    { required: true, message: '请输入应发工资', trigger: 'blur' }
  ]
}

const itemRules = {
  itemType: [
    { required: true, message: '请选择类型', trigger: 'change' }
  ],
  itemName: [
    { required: true, message: '请输入项目名称', trigger: 'blur' }
  ],
  amount: [
    { required: true, message: '请输入金额', trigger: 'blur' }
  ]
}

const calculateNetAmount = () => {
  form.netAmount = form.grossAmount - form.deductions
}

const loadData = async () => {
  loading.value = true
  try {
    const params = {
      ...pagination,
      ...filters
    }
    const { data } = await api.payrolls.list(params)
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
  filters.departmentId = null
  filters.workshopId = null
  filters.month = getCurrentMonth()
  filters.status = null
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

const handleGenerate = async () => {
  if (!generateFormRef.value) return
  
  const valid = await generateFormRef.value.validate().catch(() => false)
  if (!valid) return
  
  generating.value = true
  try {
    const { data } = await api.payrolls.generate(generateForm)
    if (data.success) {
      ElMessage.success(`成功生成 ${data.data.length} 条工资单`)
      showGenerateDialog.value = false
      loadData()
    }
  } catch (error) {
    console.error('生成失败:', error)
  } finally {
    generating.value = false
  }
}

const handleAdd = () => {
  isEdit.value = false
  Object.assign(form, {
    employeeId: null,
    month: getCurrentMonth(),
    grossAmount: 0,
    deductions: 0,
    netAmount: 0
  })
  dialogVisible.value = true
}

const handleEdit = (row) => {
  isEdit.value = true
  currentRow.value = row
  Object.assign(form, {
    employeeId: row.employeeId,
    month: row.month,
    grossAmount: row.grossAmount,
    deductions: row.deductions,
    netAmount: row.netAmount
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
      await api.payrolls.update(currentRow.value.payrollId, form)
      ElMessage.success('工资单更新成功')
    } else {
      await api.payrolls.create(form)
      ElMessage.success('工资单创建成功')
    }
    
    dialogVisible.value = false
    loadData()
  } catch (error) {
    console.error('操作失败:', error)
  } finally {
    submitting.value = false
  }
}

const handleConfirm = (row) => {
  currentRow.value = row
  confirmTitle.value = '确认工资单'
  confirmMessage.value = '确定要确认这条工资单吗？确认后不可修改。'
  confirmAction.value = async () => {
    try {
      await api.payrolls.confirm(row.payrollId)
      ElMessage.success('工资单确认成功')
      confirmDialogRef.value.hide()
      loadData()
    } catch (error) {
      console.error('确认失败:', error)
    }
  }
  confirmDialogRef.value.show()
}

const handlePay = (row) => {
  currentRow.value = row
  confirmTitle.value = '发放工资'
  confirmMessage.value = '确定要发放这条工资单吗？'
  confirmAction.value = async () => {
    try {
      await api.payrolls.pay(row.payrollId)
      ElMessage.success('工资发放成功')
      confirmDialogRef.value.hide()
      loadData()
    } catch (error) {
      console.error('发放失败:', error)
    }
  }
  confirmDialogRef.value.show()
}

const handleDelete = (row) => {
  currentRow.value = row
  confirmTitle.value = '删除工资单'
  confirmMessage.value = '确定要删除这条工资单吗？删除后不可恢复。'
  confirmAction.value = async () => {
    try {
      await api.payrolls.delete(row.payrollId)
      ElMessage.success('工资单删除成功')
      confirmDialogRef.value.hide()
      loadData()
    } catch (error) {
      console.error('删除失败:', error)
    }
  }
  confirmDialogRef.value.show()
}

const handleViewItems = async (row) => {
  currentPayroll.value = row
  try {
    const { data } = await api.payrolls.items(row.payrollId)
    if (data.success) {
      payrollItems.value = data.data
      showItemsDrawer.value = true
    }
  } catch (error) {
    console.error('加载明细失败:', error)
  }
}

const handleEditItem = (item) => {
  isEditItem.value = true
  currentItem.value = item
  Object.assign(itemForm, {
    itemType: item.itemType,
    itemName: item.itemName,
    amount: item.amount
  })
  showAddItemDialog.value = true
}

const handleSubmitItem = async () => {
  if (!itemFormRef.value) return
  
  const valid = await itemFormRef.value.validate().catch(() => false)
  if (!valid) return
  
  submittingItem.value = true
  try {
    if (isEditItem.value) {
      await api.payrolls.updateItem(currentPayroll.value.payrollId, currentItem.value.payrollItemId, itemForm)
      ElMessage.success('明细项更新成功')
    } else {
      await api.payrolls.createItem(currentPayroll.value.payrollId, itemForm)
      ElMessage.success('明细项创建成功')
    }
    
    showAddItemDialog.value = false
    handleViewItems(currentPayroll.value)
  } catch (error) {
    console.error('操作失败:', error)
  } finally {
    submittingItem.value = false
  }
}

const handleDeleteItem = (item) => {
  currentItem.value = item
  confirmTitle.value = '删除明细项'
  confirmMessage.value = '确定要删除这个明细项吗？'
  confirmAction.value = async () => {
    try {
      await api.payrolls.deleteItem(currentPayroll.value.payrollId, item.payrollItemId)
      ElMessage.success('明细项删除成功')
      confirmDialogRef.value.hide()
      handleViewItems(currentPayroll.value)
    } catch (error) {
      console.error('删除失败:', error)
    }
  }
  confirmDialogRef.value.show()
}

onMounted(() => {
  loadEmployees()
  loadDepartments()
  loadWorkshops()
  loadData()
})
</script>
