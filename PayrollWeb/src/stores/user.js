import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import { ElMessage } from 'element-plus'
import { jwtDecode } from 'jwt-decode'
import api from '@/api'

export const useUserStore = defineStore('user', () => {
  const token = ref(localStorage.getItem('token') || '')
  const userInfo = ref(null)

  const isLoggedIn = computed(() => !!token.value)
  
  const roles = computed(() => {
    if (!userInfo.value) return []
    return userInfo.value.roles || []
  })

  const userName = computed(() => userInfo.value?.userName || '')

  const hasRole = (role) => roles.value.includes(role)
  const hasAnyRole = (roleList) => roleList.some(role => roles.value.includes(role))

  const setToken = (newToken) => {
    token.value = newToken
    localStorage.setItem('token', newToken)
    
    try {
      const decoded = jwtDecode(newToken)
      userInfo.value = {
        userName: decoded.unique_name || decoded.sub,
        userId: decoded.sub,
        roles: decoded.role ? (Array.isArray(decoded.role) ? decoded.role : [decoded.role]) : []
      }
    } catch (error) {
      console.error('Token decode failed:', error)
    }
  }

  const clearAuth = () => {
    token.value = ''
    userInfo.value = null
    localStorage.removeItem('token')
  }

  const login = async (credentials) => {
    try {
      const { data } = await api.auth.login(credentials)
      if (data.success) {
        setToken(data.data.accessToken)
        ElMessage.success('登录成功')
        return true
      } else {
        ElMessage.error(data.message || '登录失败')
        return false
      }
    } catch (error) {
      ElMessage.error('网络错误，请稍后重试')
      return false
    }
  }

  const logout = () => {
    clearAuth()
    ElMessage.success('已退出登录')
  }

  // 初始化时恢复用户信息
  if (token.value) {
    try {
      const decoded = jwtDecode(token.value)
      if (decoded.exp * 1000 > Date.now()) {
        userInfo.value = {
          userName: decoded.unique_name || decoded.sub,
          userId: decoded.sub,
          roles: decoded.role ? (Array.isArray(decoded.role) ? decoded.role : [decoded.role]) : []
        }
      } else {
        clearAuth()
      }
    } catch (error) {
      clearAuth()
    }
  }

  return {
    token,
    userInfo,
    isLoggedIn,
    roles,
    userName,
    hasRole,
    hasAnyRole,
    login,
    logout,
    setToken,
    clearAuth
  }
})
