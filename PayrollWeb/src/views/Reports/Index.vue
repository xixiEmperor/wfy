<template>
  <div class="space-y-6">
    <PageHeader title="统计报表">
      <template #actions>
        <el-button
          type="success"
          :disabled="!reportData.length"
          @click="exportReport"
        >
          导出报表
        </el-button>
      </template>
    </PageHeader>

    <div class="page-card">
      <h3 class="text-lg font-semibold mb-4">查询条件</h3>
      <el-form :model="queryForm" class="form-inline">
        <el-form-item label="部门">
          <el-select
            v-model="queryForm.departmentId"
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
            v-model="queryForm.workshopId"
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
            v-model="queryForm.month"
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
        <el-form-item>
          <el-button
            type="primary"
            @click="loadReportData"
          >
            查询
          </el-button>
          <el-button @click="resetQuery">
            重置
          </el-button>
        </el-form-item>
      </el-form>
    </div>

    <!-- 统计图表 -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <div class="page-card">
        <h3 class="text-lg font-semibold mb-4">月度工资趋势</h3>
        <div ref="trendChartRef" class="h-80"></div>
      </div>

      <div class="page-card">
        <h3 class="text-lg font-semibold mb-4">部门工资分布</h3>
        <div ref="deptChartRef" class="h-80"></div>
      </div>
    </div>

    <!-- 汇总数据表格 -->
    <div class="page-card">
      <h3 class="text-lg font-semibold mb-4">汇总数据</h3>
      <el-table
        :data="reportData"
        :loading="loading"
        border
        show-summary
        :summary-method="getSummaries"
      >
        <el-table-column prop="month" label="月份" width="100" />
        <el-table-column prop="departmentName" label="部门" width="120" />
        <el-table-column prop="workshopName" label="车间" width="120" />
        <el-table-column prop="employeeCount" label="人数" width="80" />
        <el-table-column prop="totalGross" label="应发总额" width="120">
          <template #default="{ row }">
            ¥{{ formatMoney(row.totalGross) }}
          </template>
        </el-table-column>
        <el-table-column prop="totalDeductions" label="扣款总额" width="120">
          <template #default="{ row }">
            ¥{{ formatMoney(row.totalDeductions) }}
          </template>
        </el-table-column>
        <el-table-column prop="totalNet" label="实发总额" width="120">
          <template #default="{ row }">
            <span class="font-semibold text-green-600">
              ¥{{ formatMoney(row.totalNet) }}
            </span>
          </template>
        </el-table-column>
        <el-table-column prop="avgSalary" label="平均工资" width="120">
          <template #default="{ row }">
            ¥{{ formatMoney(row.avgSalary) }}
          </template>
        </el-table-column>
        <el-table-column label="操作" width="120" fixed="right">
          <template #default="{ row }">
            <el-button
              type="primary"
              link
              @click="viewEmployeeDetails(row)"
            >
              查看明细
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </div>

    <!-- 员工明细对话框 -->
    <el-dialog
      v-model="showEmployeeDialog"
      :title="`员工明细 - ${currentRowDetail?.departmentName} ${currentRowDetail?.workshopName || ''} ${currentRowDetail?.month}`"
      width="80%"
    >
      <el-table
        :data="employeeDetails"
        :loading="loadingDetails"
        border
      >
        <el-table-column prop="employeeNo" label="工号" width="100" />
        <el-table-column prop="fullName" label="姓名" width="100" />
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
        <el-table-column label="操作" width="120">
          <template #default="{ row }">
            <el-button
              type="primary"
              link
              @click="viewEmployeeHistory(row.employeeId)"
            >
              查看历史
            </el-button>
          </template>
        </el-table-column>
      </el-table>
    </el-dialog>

    <!-- 员工历史对话框 -->
    <el-dialog
      v-model="showHistoryDialog"
      title="员工工资历史"
      width="80%"
    >
      <div class="space-y-4">
        <div class="page-card">
          <h4 class="text-base font-semibold mb-3">工资单历史</h4>
          <el-table
            :data="employeeHistory.payrolls"
            :loading="loadingHistory"
            border
            max-height="300"
          >
            <el-table-column prop="month" label="月份" width="100" />
            <el-table-column prop="grossAmount" label="应发工资" width="120">
              <template #default="{ row }">
                ¥{{ formatMoney(row.grossAmount) }}
              </template>
            </el-table-column>
            <el-table-column prop="netAmount" label="实发工资" width="120">
              <template #default="{ row }">
                ¥{{ formatMoney(row.netAmount) }}
              </template>
            </el-table-column>
            <el-table-column prop="status" label="状态" width="100">
              <template #default="{ row }">
                <el-tag :type="getPayrollStatusType(row.status)">
                  {{ getPayrollStatusText(row.status) }}
                </el-tag>
              </template>
            </el-table-column>
          </el-table>
        </div>

        <div class="page-card">
          <h4 class="text-base font-semibold mb-3">年终奖历史</h4>
          <el-table
            :data="employeeHistory.bonuses"
            border
            max-height="300"
          >
            <el-table-column prop="year" label="年度" width="100" />
            <el-table-column prop="amount" label="奖金金额" width="120">
              <template #default="{ row }">
                ¥{{ formatMoney(row.amount) }}
              </template>
            </el-table-column>
            <el-table-column prop="remark" label="备注" />
          </el-table>
        </div>
      </div>
    </el-dialog>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, nextTick } from 'vue'
import { ElMessage } from 'element-plus'
import * as echarts from 'echarts'
import PageHeader from '@/components/PageHeader.vue'
import { useUserStore } from '@/stores/user'
import api from '@/api'
import { formatMoney, getCurrentMonth, getRecentMonths, exportToCSV, getPayrollStatusText, getPayrollStatusType } from '@/utils'

const userStore = useUserStore()
const trendChartRef = ref()
const deptChartRef = ref()

const loading = ref(false)
const loadingDetails = ref(false)
const loadingHistory = ref(false)
const showEmployeeDialog = ref(false)
const showHistoryDialog = ref(false)

const reportData = ref([])
const employeeDetails = ref([])
const employeeHistory = ref({ payrolls: [], bonuses: [] })
const departments = ref([])
const workshops = ref([])
const currentRowDetail = ref(null)
const recentMonths = getRecentMonths()

const queryForm = reactive({
  departmentId: null,
  workshopId: null,
  month: getCurrentMonth()
})

const loadReportData = async () => {
  loading.value = true
  try {
    const { data } = await api.reports.summary(queryForm)
    if (data.success) {
      // 模拟报表数据结构
      reportData.value = [
        {
          month: queryForm.month || getCurrentMonth(),
          departmentName: '生产部',
          workshopName: '一车间',
          employeeCount: 25,
          totalGross: 125000,
          totalDeductions: 15000,
          totalNet: 110000,
          avgSalary: 4400
        },
        {
          month: queryForm.month || getCurrentMonth(),
          departmentName: '技术部',
          workshopName: '',
          employeeCount: 15,
          totalGross: 90000,
          totalDeductions: 12000,
          totalNet: 78000,
          avgSalary: 5200
        }
      ]
      
      await nextTick()
      initCharts()
    }
  } catch (error) {
    console.error('加载报表数据失败:', error)
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

const resetQuery = () => {
  Object.assign(queryForm, {
    departmentId: null,
    workshopId: null,
    month: getCurrentMonth()
  })
}

const getSummaries = (param) => {
  const { columns, data } = param
  const sums = []
  
  columns.forEach((column, index) => {
    if (index === 0) {
      sums[index] = '合计'
      return
    }
    
    const values = data.map(item => Number(item[column.property]))
    if (!values.every(value => Number.isNaN(value))) {
      if (['employeeCount'].includes(column.property)) {
        sums[index] = values.reduce((prev, curr) => prev + curr, 0)
      } else if (['totalGross', 'totalDeductions', 'totalNet'].includes(column.property)) {
        sums[index] = '¥' + formatMoney(values.reduce((prev, curr) => prev + curr, 0))
      } else if (column.property === 'avgSalary') {
        const totalNet = data.reduce((sum, item) => sum + item.totalNet, 0)
        const totalCount = data.reduce((sum, item) => sum + item.employeeCount, 0)
        sums[index] = totalCount > 0 ? '¥' + formatMoney(totalNet / totalCount) : '¥0.00'
      } else {
        sums[index] = ''
      }
    } else {
      sums[index] = ''
    }
  })
  
  return sums
}

const exportReport = () => {
  const exportData = reportData.value.map(row => ({
    '月份': row.month,
    '部门': row.departmentName,
    '车间': row.workshopName || '',
    '人数': row.employeeCount,
    '应发总额': formatMoney(row.totalGross),
    '扣款总额': formatMoney(row.totalDeductions),
    '实发总额': formatMoney(row.totalNet),
    '平均工资': formatMoney(row.avgSalary)
  }))
  
  exportToCSV(exportData, '工资汇总报表')
}

const viewEmployeeDetails = async (row) => {
  currentRowDetail.value = row
  loadingDetails.value = true
  showEmployeeDialog.value = true
  
  try {
    // 模拟员工明细数据
    employeeDetails.value = [
      {
        employeeNo: 'E001',
        fullName: '张三',
        grossAmount: 5000,
        deductions: 600,
        netAmount: 4400,
        status: 2,
        employeeId: 1
      }
    ]
  } catch (error) {
    console.error('加载员工明细失败:', error)
  } finally {
    loadingDetails.value = false
  }
}

const viewEmployeeHistory = async (employeeId) => {
  loadingHistory.value = true
  showHistoryDialog.value = true
  
  try {
    const { data } = await api.reports.employeeHistory(employeeId)
    if (data.success) {
      employeeHistory.value = data.data
    }
  } catch (error) {
    console.error('加载员工历史失败:', error)
  } finally {
    loadingHistory.value = false
  }
}

const initCharts = () => {
  initTrendChart()
  initDeptChart()
}

const initTrendChart = () => {
  if (!trendChartRef.value) return
  
  const chart = echarts.init(trendChartRef.value)
  
  const option = {
    tooltip: {
      trigger: 'axis'
    },
    legend: {
      data: ['应发工资', '实发工资']
    },
    xAxis: {
      type: 'category',
      data: ['1月', '2月', '3月', '4月', '5月', '6月']
    },
    yAxis: {
      type: 'value',
      axisLabel: {
        formatter: '¥{value}'
      }
    },
    series: [
      {
        name: '应发工资',
        type: 'line',
        data: [120000, 135000, 142000, 138000, 155000, 148000],
        smooth: true,
        itemStyle: { color: '#409EFF' }
      },
      {
        name: '实发工资',
        type: 'line',
        data: [105000, 118000, 125000, 122000, 138000, 130000],
        smooth: true,
        itemStyle: { color: '#67C23A' }
      }
    ]
  }
  
  chart.setOption(option)
  
  window.addEventListener('resize', () => {
    chart.resize()
  })
}

const initDeptChart = () => {
  if (!deptChartRef.value) return
  
  const chart = echarts.init(deptChartRef.value)
  
  const option = {
    tooltip: {
      trigger: 'item',
      formatter: '{a} <br/>{b}: ¥{c} ({d}%)'
    },
    series: [
      {
        name: '部门工资分布',
        type: 'pie',
        radius: '60%',
        data: reportData.value.map(item => ({
          value: item.totalNet,
          name: item.departmentName
        })),
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowOffsetX: 0,
            shadowColor: 'rgba(0, 0, 0, 0.5)'
          }
        }
      }
    ]
  }
  
  chart.setOption(option)
  
  window.addEventListener('resize', () => {
    chart.resize()
  })
}

onMounted(() => {
  loadDepartments()
  loadWorkshops()
  loadReportData()
})
</script>
