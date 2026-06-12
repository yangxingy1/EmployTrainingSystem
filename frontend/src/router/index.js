// 路由配置 —— 含角色保护和页面标题
import { createRouter, createWebHistory } from 'vue-router'

import Login from '../views/Login.vue'
import AdminHome from '../views/AdminHome.vue'
import StudentHome from '../views/StudentHome.vue'

const routes = [
  { path: '/', redirect: '/login' },
  {
    path: '/login',
    component: Login,
    meta: { title: '登录', guest: true }
  },
  {
    path: '/admin',
    component: AdminHome,
    meta: { title: '管理中心', requiresAuth: true, role: 'admin' }
  },
  {
    path: '/student',
    component: StudentHome,
    meta: { title: '训练中心', requiresAuth: true, role: 'student' }
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

// 全局导航守卫：未登录跳登录页，角色不匹配跳对应首页
router.beforeEach((to, from, next) => {
  document.title = to.meta.title ? `${to.meta.title} - 慧动手` : '慧动手'

  if (to.path === '/login') return next()

  const token = localStorage.getItem('token')
  const role = localStorage.getItem('role')

  if (!token) return next('/login')

  // 角色路由保护
  if (to.meta.role && to.meta.role !== role) {
    return next(role === 'admin' ? '/admin' : '/student')
  }

  return next()
})

export default router
