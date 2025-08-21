<template>
  <div class="space-y-6">
    <PageHeader title="仪表盘" />

    <!-- 统计卡片 -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      <div class="page-card">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-gray-600 text-sm">员工总数</p>
            <p class="text-2xl font-bold text-gray-800">{{ stats.totalEmployees }}</p>
          </div>
          <el-icon class="text-3xl text-blue-500">
            <User />
          </el-icon>
        </div>
      </div>

      <div class="page-card">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-gray-600 text-sm">本月已确认工资单</p>
            <p class="text-2xl font-bold text-gray-800">{{ stats.confirmedPayrolls }}</p>
          </div>
          <el-icon class="text-3xl text-green-500">
            <DocumentChecked />
          </el-icon>
        </div>
      </div>

      <div class="page-card">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-gray-600 text-sm">本月已发放工资单</p>
            <p class="text-2xl font-bold text-gray-800">{{ stats.paidPayrolls }}</p>
          </div>
          <el-icon class="text-3xl text-orange-500">
            <Money />
          </el-icon>
        </div>
      </div>

      <div class="page-card">
        <div class="flex items-center justify-between">
          <div>
            <p class="text-gray-600 text-sm">本月工资总额</p>
            <p class="text-2xl font-bold text-gray-800">¥{{ formatMoney(stats.totalSalary) }}</p>
          </div>
          <el-icon class="text-3xl text-purple-500">
            <TrendCharts />
          </el-icon>
        </div>
      </div>
    </div>

    <!-- 图表区域 -->
    <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
      <div class="page-card">
        <h3 class="text-lg font-semibold mb-4">月度工资发放趋势</h3>
        <div ref="trendChartRef" class="h-80"></div>
      </div>

      <div class="page-card">
        <h3 class="text-lg font-semibold mb-4">部门分布</h3>
        <div ref="deptChartRef" class="h-80"></div>
      </div>
    </div>

    <!-- 最近动态 -->
    <div class="page-card">
      <h3 class="text-lg font-semibold mb-4">最近动态</h3>
      <el-timeline>
        <el-timeline-item
          v-for="item in recentActivities"
          :key="item.id"
          :timestamp="item.time"
          placement="top"
        >
          {{ item.content }}
        </el-timeline-item>
      </el-timeline>
    </div>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, nextTick } from 'vue'
import { User, DocumentChecked, Money, TrendCharts } from '@element-plus/icons-vue'
import * as echarts from 'echarts'
import PageHeader from '@/components/PageHeader.vue'
import api from '@/api'
import { formatMoney, getCurrentMonth } from '@/utils'

const trendChartRef = ref()
const deptChartRef = ref()

const stats = reactive({
  totalEmployees: 0,
  confirmedPayrolls: 0,
  paidPayrolls: 0,
  totalSalary: 0
})

const recentActivities = ref([
  { id: 1, content: '系统初始化完成', time: '2024-01-01 09:00' },
  { id: 2, content: '欢迎使用薪资管理系统', time: '2024-01-01 09:01' }
])

const loadStats = async () => {
  try {
    const { data } = await api.reports.summary({
      month: getCurrentMonth()
    })
    
    if (data.success && data.data) {
      const summaryData = data.data
      if (summaryData.length > 0) {
        stats.confirmedPayrolls = summaryData.filter(item => item.status >= 1).reduce((sum, item) => sum + item.count, 0)
        stats.paidPayrolls = summaryData.filter(item => item.status === 2).reduce((sum, item) => sum + item.count, 0)
        stats.totalSalary = summaryData.reduce((sum, item) => sum + (item.net || 0), 0)
      }
    }
  } catch (error) {
    console.error('加载统计数据失败:', error)
  }
}

const loadEmployeeCount = async () => {
  try {
    const { data } = await api.employees.list({ page: 1, pageSize: 1 })
    if (data.success && data.meta) {
      stats.totalEmployees = data.meta.total
    }
  } catch (error) {
    console.error('加载员工数量失败:', error)
  }
}

const initTrendChart = () => {
  const chart = echarts.init(trendChartRef.value)
  
  const option = {
    tooltip: {
      trigger: 'axis'
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
        name: '工资总额',
        type: 'line',
        data: [120000, 135000, 142000, 138000, 155000, 148000],
        smooth: true,
        itemStyle: {
          color: '#409EFF'
        }
      }
    ]
  }
  
  chart.setOption(option)
  
  window.addEventListener('resize', () => {
    chart.resize()
  })
}

const initDeptChart = () => {
  const chart = echarts.init(deptChartRef.value)
  
  const option = {
    tooltip: {
      trigger: 'item'
    },
    series: [
      {
        type: 'pie',
        radius: '60%',
        data: [
          { value: 35, name: '生产部' },
          { value: 20, name: '技术部' },
          { value: 15, name: '销售部' },
          { value: 10, name: '财务部' },
          { value: 10, name: '行政部' },
          { value: 10, name: '其他' }
        ],
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

onMounted(async () => {
  await Promise.all([
    loadStats(),
    loadEmployeeCount()
  ])
  
  await nextTick()
  initTrendChart()
  initDeptChart()
})
</script>
