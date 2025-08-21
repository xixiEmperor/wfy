import axios from 'axios'
import { ElMessage } from 'element-plus'
import { useUserStore } from '@/stores/user'
import router from '@/router'

const baseURL = import.meta.env.VITE_API_BASE + '/api'

const request = axios.create({
  baseURL,
  timeout: 30000,
  headers: {
    'Content-Type': 'application/json'
  }
})

// 请求拦截器
request.interceptors.request.use(
  (config) => {
    const userStore = useUserStore()
    if (userStore.token) {
      config.headers.Authorization = `Bearer ${userStore.token}`
    }
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器
request.interceptors.response.use(
  (response) => {
    return response
  },
  (error) => {
    if (error.response?.status === 401) {
      const userStore = useUserStore()
      userStore.clearAuth()
      router.push('/login')
      ElMessage.error('登录已过期，请重新登录')
    } else if (error.response?.data?.message) {
      ElMessage.error(error.response.data.message)
    } else {
      ElMessage.error('网络错误，请稍后重试')
    }
    return Promise.reject(error)
  }
)

// API 模块
const auth = {
  login: (data) => request.post('/auth/login', data)
}

const departments = {
  list: (params) => request.get('/departments', { params }),
  create: (data) => request.post('/departments', data),
  update: (id, data) => request.put(`/departments/${id}`, data),
  delete: (id) => request.delete(`/departments/${id}`)
}

const workshops = {
  list: (params) => request.get('/workshops', { params }),
  create: (data) => request.post('/workshops', data),
  update: (id, data) => request.put(`/workshops/${id}`, data),
  delete: (id) => request.delete(`/workshops/${id}`)
}

const employees = {
  list: (params) => request.get('/employees', { params }),
  create: (data) => request.post('/employees', data),
  update: (id, data) => request.put(`/employees/${id}`, data),
  delete: (id) => request.delete(`/employees/${id}`)
}

const attendance = {
  list: (params) => request.get('/attendance', { params }),
  create: (data) => request.post('/attendance', data),
  update: (id, data) => request.put(`/attendance/${id}`, data),
  delete: (id) => request.delete(`/attendance/${id}`)
}

const logistics = {
  list: (params) => request.get('/logistics', { params }),
  create: (data) => request.post('/logistics', data),
  update: (id, data) => request.put(`/logistics/${id}`, data),
  delete: (id) => request.delete(`/logistics/${id}`)
}

const socialSecurity = {
  list: (params) => request.get('/socialsecurity', { params }),
  create: (data) => request.post('/socialsecurity', data),
  update: (id, data) => request.put(`/socialsecurity/${id}`, data),
  delete: (id) => request.delete(`/socialsecurity/${id}`)
}

const payrolls = {
  list: (params) => request.get('/payrolls', { params }),
  generate: (data) => request.post('/payrolls/generate', data),
  create: (data) => request.post('/payrolls', data),
  update: (id, data) => request.put(`/payrolls/${id}`, data),
  delete: (id) => request.delete(`/payrolls/${id}`),
  confirm: (id) => request.post(`/payrolls/${id}/confirm`),
  pay: (id) => request.post(`/payrolls/${id}/pay`),
  items: (id) => request.get(`/payrolls/${id}/items`),
  createItem: (id, data) => request.post(`/payrolls/${id}/items`, data),
  updateItem: (id, itemId, data) => request.put(`/payrolls/${id}/items/${itemId}`, data),
  deleteItem: (id, itemId) => request.delete(`/payrolls/${id}/items/${itemId}`)
}

const salaryChanges = {
  list: (params) => request.get('/salarychanges', { params }),
  create: (data) => request.post('/salarychanges', data),
  update: (id, data) => request.put(`/salarychanges/${id}`, data),
  delete: (id) => request.delete(`/salarychanges/${id}`)
}

const yearEndBonus = {
  list: (params) => request.get('/yearendbonuses', { params }),
  create: (data) => request.post('/yearendbonuses', data),
  update: (id, data) => request.put(`/yearendbonuses/${id}`, data),
  delete: (id) => request.delete(`/yearendbonuses/${id}`)
}

const reports = {
  summary: (params) => request.get('/reports/summary', { params }),
  employeeHistory: (employeeId) => request.get(`/reports/employee/${employeeId}`)
}

export default {
  auth,
  departments,
  workshops,
  employees,
  attendance,
  logistics,
  socialSecurity,
  payrolls,
  salaryChanges,
  yearEndBonus,
  reports
}
