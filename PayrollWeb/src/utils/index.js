import { saveAs } from 'file-saver'
import * as XLSX from 'xlsx'
import dayjs from 'dayjs'

// 格式化金额
export const formatMoney = (amount) => {
  if (amount == null) return '0.00'
  return Number(amount).toFixed(2)
}

// 格式化日期
export const formatDate = (date, format = 'YYYY-MM-DD') => {
  if (!date) return ''
  return dayjs(date).format(format)
}

// 导出Excel
export const exportToExcel = (data, filename = 'export') => {
  const ws = XLSX.utils.json_to_sheet(data)
  const wb = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(wb, ws, 'Sheet1')
  const excelBuffer = XLSX.write(wb, { bookType: 'xlsx', type: 'array' })
  const blob = new Blob([excelBuffer], { type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet' })
  saveAs(blob, `${filename}_${dayjs().format('YYYY-MM-DD')}.xlsx`)
}

// 导出CSV
export const exportToCSV = (data, filename = 'export') => {
  if (!data.length) return
  
  const headers = Object.keys(data[0])
  const csvContent = [
    headers.join(','),
    ...data.map(row => headers.map(header => `"${row[header] || ''}"`).join(','))
  ].join('\n')
  
  const blob = new Blob(['\ufeff' + csvContent], { type: 'text/csv;charset=utf-8' })
  saveAs(blob, `${filename}_${dayjs().format('YYYY-MM-DD')}.csv`)
}

// 下载模板
export const downloadTemplate = (columns, filename = 'template') => {
  const headers = columns.map(col => col.label || col.prop)
  const sampleData = [{}]
  columns.forEach(col => {
    sampleData[0][col.label || col.prop] = col.example || ''
  })
  
  exportToExcel(sampleData, `${filename}_模板`)
}

// 解析上传的Excel/CSV文件
export const parseUploadFile = (file) => {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    
    reader.onload = (e) => {
      try {
        const data = e.target.result
        let jsonData = []
        
        if (file.name.endsWith('.csv')) {
          // 处理CSV
          const text = new TextDecoder('utf-8').decode(data)
          const lines = text.split('\n').filter(line => line.trim())
          if (lines.length < 2) {
            reject(new Error('文件格式错误'))
            return
          }
          
          const headers = lines[0].split(',').map(h => h.trim().replace(/['"]/g, ''))
          jsonData = lines.slice(1).map(line => {
            const values = line.split(',').map(v => v.trim().replace(/['"]/g, ''))
            const obj = {}
            headers.forEach((header, index) => {
              obj[header] = values[index] || ''
            })
            return obj
          })
        } else {
          // 处理Excel
          const workbook = XLSX.read(data, { type: 'array' })
          const sheetName = workbook.SheetNames[0]
          const worksheet = workbook.Sheets[sheetName]
          jsonData = XLSX.utils.sheet_to_json(worksheet)
        }
        
        resolve(jsonData)
      } catch (error) {
        reject(new Error('文件解析失败'))
      }
    }
    
    reader.onerror = () => reject(new Error('文件读取失败'))
    reader.readAsArrayBuffer(file)
  })
}

// 生成当前月份字符串
export const getCurrentMonth = () => {
  return dayjs().format('YYYY-MM')
}

// 获取最近几个月的选项
export const getRecentMonths = (count = 12) => {
  const months = []
  for (let i = 0; i < count; i++) {
    const month = dayjs().subtract(i, 'month').format('YYYY-MM')
    months.push({
      label: month,
      value: month
    })
  }
  return months
}

// 性别选项
export const genderOptions = [
  { label: '男', value: '男' },
  { label: '女', value: '女' }
]

// 工资单状态
export const payrollStatusOptions = [
  { label: '草稿', value: 0, type: 'info' },
  { label: '已确认', value: 1, type: 'warning' },
  { label: '已发放', value: 2, type: 'success' }
]

export const getPayrollStatusText = (status) => {
  const option = payrollStatusOptions.find(opt => opt.value === status)
  return option ? option.label : '未知'
}

export const getPayrollStatusType = (status) => {
  const option = payrollStatusOptions.find(opt => opt.value === status)
  return option ? option.type : 'info'
}
