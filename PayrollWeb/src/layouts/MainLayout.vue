<template>
  <el-container class="min-h-screen">
    <el-aside :width="sidebarWidth" class="bg-gray-800 text-white transition-all duration-300">
      <div class="h-16 flex items-center justify-center border-b border-gray-700">
        <h1 class="text-lg font-bold text-white">
          {{ collapsed ? '薪资' : '薪资管理系统' }}
        </h1>
      </div>
      <el-menu
        :default-active="$route.name"
        :collapse="collapsed"
        background-color="#374151"
        text-color="#f3f4f6"
        active-text-color="#60a5fa"
        router
        class="border-0"
      >
        <template v-for="item in menuItems" :key="item.name">
          <el-menu-item
            v-if="canAccessRoute(item)"
            :index="item.name"
            :route="{ name: item.name }"
          >
            <el-icon><component :is="item.meta.icon" /></el-icon>
            <template #title>{{ item.meta.title }}</template>
          </el-menu-item>
        </template>
      </el-menu>
    </el-aside>

    <el-container>
      <el-header class="bg-white shadow-sm flex items-center justify-between px-4">
        <div class="flex items-center gap-4">
          <el-button
            :icon="Fold"
            text
            @click="toggleSidebar"
          />
          <el-breadcrumb separator="/">
            <el-breadcrumb-item :to="{ name: 'Dashboard' }">首页</el-breadcrumb-item>
            <el-breadcrumb-item v-if="$route.meta.title">
              {{ $route.meta.title }}
            </el-breadcrumb-item>
          </el-breadcrumb>
        </div>

        <div class="flex items-center gap-4">
          <span class="text-gray-600">{{ userStore.userName }}</span>
          <el-tag size="small" type="info">{{ roleText }}</el-tag>
          <el-button
            :icon="SwitchButton"
            size="small"
            @click="handleLogout"
          >
            退出
          </el-button>
        </div>
      </el-header>

      <el-main class="bg-gray-50 p-6">
        <RouterView />
      </el-main>
    </el-container>
  </el-container>
</template>

<script setup>
import { computed } from 'vue'
import { useRouter } from 'vue-router'
import { ElMessageBox } from 'element-plus'
import { Fold, SwitchButton } from '@element-plus/icons-vue'
import { useUserStore } from '@/stores/user'
import { useAppStore } from '@/stores/app'

const router = useRouter()
const userStore = useUserStore()
const appStore = useAppStore()

const collapsed = computed(() => appStore.sidebarCollapsed)
const sidebarWidth = computed(() => collapsed.value ? '64px' : '200px')

const roleText = computed(() => {
  const roles = userStore.roles
  if (roles.includes('Admin')) return '管理员'
  if (roles.includes('HR')) return 'HR'
  return '普通用户'
})

const menuItems = computed(() => {
  return router.getRoutes()
    .filter(route => route.path !== '/login' && route.meta?.title)
    .sort((a, b) => {
      const order = ['Dashboard', 'Departments', 'Workshops', 'Employees', 'Attendance', 'Logistics', 'SocialSecurity', 'Payrolls', 'SalaryChanges', 'YearEndBonus', 'Reports']
      return order.indexOf(a.name) - order.indexOf(b.name)
    })
})

const canAccessRoute = (route) => {
  if (!route.meta?.roles) return true
  return userStore.hasAnyRole(route.meta.roles)
}

const toggleSidebar = () => {
  appStore.toggleSidebar()
}

const handleLogout = async () => {
  try {
    await ElMessageBox.confirm(
      '确定要退出登录吗？',
      '确认退出',
      {
        confirmButtonText: '确定',
        cancelButtonText: '取消',
        type: 'warning'
      }
    )
    userStore.logout()
    router.push('/login')
  } catch {
    // 用户取消
  }
}
</script>
