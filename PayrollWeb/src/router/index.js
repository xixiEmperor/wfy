import { createRouter, createWebHistory } from 'vue-router'
import { useUserStore } from '@/stores/user'

const routes = [
  {
    path: '/login',
    name: 'Login',
    component: () => import('@/views/Auth/Login.vue'),
    meta: { requiresAuth: false }
  },
  {
    path: '/',
    component: () => import('@/layouts/MainLayout.vue'),
    meta: { requiresAuth: true },
    children: [
      {
        path: '',
        name: 'Dashboard',
        component: () => import('@/views/Dashboard/Index.vue'),
        meta: { title: '仪表盘', icon: 'Odometer' }
      },
      {
        path: 'departments',
        name: 'Departments',
        component: () => import('@/views/Departments/Index.vue'),
        meta: { title: '部门管理', icon: 'OfficeBuilding', roles: ['Admin', 'HR'] }
      },
      {
        path: 'workshops',
        name: 'Workshops',
        component: () => import('@/views/Workshops/Index.vue'),
        meta: { title: '车间管理', icon: 'House', roles: ['Admin', 'HR'] }
      },
      {
        path: 'employees',
        name: 'Employees',
        component: () => import('@/views/Employees/Index.vue'),
        meta: { title: '员工管理', icon: 'User' }
      },
      {
        path: 'attendance',
        name: 'Attendance',
        component: () => import('@/views/Attendance/Index.vue'),
        meta: { title: '考勤管理', icon: 'Clock' }
      },
      {
        path: 'logistics',
        name: 'Logistics',
        component: () => import('@/views/Logistics/Index.vue'),
        meta: { title: '后勤管理', icon: 'Box' }
      },
      {
        path: 'social-security',
        name: 'SocialSecurity',
        component: () => import('@/views/SocialSecurity/Index.vue'),
        meta: { title: '三金管理', icon: 'CreditCard' }
      },
      {
        path: 'payrolls',
        name: 'Payrolls',
        component: () => import('@/views/Payrolls/Index.vue'),
        meta: { title: '工资单管理', icon: 'Money' }
      },
      {
        path: 'salary-changes',
        name: 'SalaryChanges',
        component: () => import('@/views/SalaryChanges/Index.vue'),
        meta: { title: '工资变动', icon: 'TrendCharts' }
      },
      {
        path: 'year-end-bonus',
        name: 'YearEndBonus',
        component: () => import('@/views/YearEndBonus/Index.vue'),
        meta: { title: '年终奖金', icon: 'Present' }
      },
      {
        path: 'reports',
        name: 'Reports',
        component: () => import('@/views/Reports/Index.vue'),
        meta: { title: '统计报表', icon: 'DataAnalysis' }
      }
    ]
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

router.beforeEach((to, from, next) => {
  const userStore = useUserStore()
  
  if (to.meta.requiresAuth !== false && !userStore.isLoggedIn) {
    next('/login')
    return
  }
  
  if (to.name === 'Login' && userStore.isLoggedIn) {
    next('/')
    return
  }
  
  if (to.meta.roles && to.meta.roles.length > 0) {
    if (!userStore.hasAnyRole(to.meta.roles)) {
      ElMessage.error('您没有权限访问此页面')
      next('/')
      return
    }
  }
  
  next()
})

export default router
