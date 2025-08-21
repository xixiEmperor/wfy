<template>
  <div class="space-y-6">
    <PageHeader title="年终奖金">
      <template #actions>
        <el-button
          v-if="userStore.hasAnyRole(['Admin', 'HR'])"
          type="primary"
          @click="handleAdd"
        >
          新增奖金记录
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
      <el-form-item label="年度">
        <el-select
          v-model="filters.year"
          placeholder="请选择年度"
          clearable
        >
          <el-option
            v-for="year in yearOptions"
            :key="year"
            :label="year + '年'"
            :value="year"
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
      <el-table-column prop="year" label="年度" width="100">
        <template #default="{ row }">
          {{ row.year }}年
        </template>
      </el-table-column>
      <el-table-column prop="amount" label="奖金金额" width="150">
        <template #default="{ row }">
          <span class="font-semibold text-green-600">
            ¥{{ formatMoney(row.amount) }}
          </span>
        </template>
      </el-table-column>
      <el-table-column prop="remark" label="备注" />
      <el-table-column prop="createdAt" label="创建时间" width="180">
        <template #default="{ row }">
          {{ formatDate(row.createdAt, 'YYYY-MM-DD HH:mm') }}
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
      :title="isEdit ? '编辑奖金记录' : '新增奖金记录'"
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
        
        <el-form-item label="年度" prop="year">
          <el-select
            v-model="form.year"
            placeholder="请选择年度"
            class="w-full"
          >
            <el-option
              v-for="year in yearOptions"
              :key="year"
              :label="year + '年'"
              :value="year"
            />
          </el-select>
        </el-form-item>

        <el-form-item label="奖金金额" prop="amount">
          <el-input-number
            v-model="form.amount"
            :min="0"
            :precision="2"
            :step="100"
            class="w-full"
            placeholder="元"
          />
        </el-form-item>

        <el-form-item label="备注">
          <el-input
            v-model="form.remark"
            type="textarea"
            :rows="3"
            placeholder="请输入备注信息"
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
      title="删除奖金记录"
      message="确定要删除这条奖金记录吗？删除后不可恢复。"
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

const currentYear = dayjs().year()
const yearOptions = Array.from({ length: 5 }, (_, i) => currentYear - i)

const pagination = reactive({
  page: 1,
  pageSize: 20,
  sortBy: '',
  sortDir: 'desc'
})

const filters = reactive({
  employeeId: null,
  year: null
})

const form = reactive({
  employeeId: null,
  year: currentYear,
  amount: 0,
  remark: ''
})

const rules = {
  employeeId: [
    { required: true, message: '请选择员工', trigger: 'change' }
  ],
  year: [
    { required: true, message: '请选择年度', trigger: 'change' }
  ],
  amount: [
    { required: true, message: '请输入奖金金额', trigger: 'blur' }
  ]
}

const loadData = async () => {
  loading.value = true
  try {
    const params = {
      ...pagination,
      ...filters
    }
    const { data } = await api.yearEndBonus.list(params)
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
  filters.year = null
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
    year: currentYear,
    amount: 0,
    remark: ''
  })
  dialogVisible.value = true
}

const handleEdit = (row) => {
  isEdit.value = true
  currentRow.value = row
  Object.assign(form, {
    employeeId: row.employeeId,
    year: row.year,
    amount: row.amount,
    remark: row.remark || ''
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
      await api.yearEndBonus.update(currentRow.value.yearEndBonusId, form)
      ElMessage.success('奖金记录更新成功')
    } else {
      await api.yearEndBonus.create(form)
      ElMessage.success('奖金记录创建成功')
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
    await api.yearEndBonus.delete(currentRow.value.yearEndBonusId)
    ElMessage.success('奖金记录删除成功')
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
