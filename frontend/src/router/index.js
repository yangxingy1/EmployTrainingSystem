// 路由配置 —— 含角色保护和页面标题
import { createRouter, createWebHistory } from "vue-router";
import Login from "../views/Login.vue";
import AdminHome from "../views/AdminHome.vue";
import StudentHome from "../views/StudentHome.vue";
import RootLogin from "../views/RootLogin.vue";
import RootHome from "../views/RootHome.vue";

// 路由表: path -> 组件 + 元信息（角色、标题、游客访问）
const routes = [
  { path: "/", redirect: "/login" },
  {
    path: "/login",
    component: Login,
    meta: { title: "登录", guest: true }
  },
  {
    path: "/admin",
    component: AdminHome,
    meta: { title: "管理中心", requiresAuth: true, role: "admin" }
  },
  {
    path: "/student",
    component: StudentHome,
    meta: { title: "训练中心", requiresAuth: true, role: "student" }
  },
  {
    path: "/rootlogin",
    component: RootLogin,
    meta: { title: "Root登录", guest: true }
  },
  {
    path: "/roothome",
    component: RootHome,
    meta: { title: "Root控制台", requiresAuth: true, role: "root" }
  }
];

const router = createRouter({
  history: createWebHistory(),
  routes
});

// 全局导航守卫: 未登录跳登录页，角色不匹配跳对应首页
router.beforeEach((to, from, next) => {
  // 设置页面标题
  document.title = to.meta.title ? `${to.meta.title} - 慧动手` : "慧动手";

  // 游客页面（登录/注册）直接放行
  if (to.meta.guest) {
    return next();
  }

  const token = localStorage.getItem("token");
  const role = localStorage.getItem("role");

  // 无 token 跳转登录页
  if (!token) return next("/login");

  // 角色路由保护: 禁止越权访问
  if (to.meta.role && to.meta.role !== role) {
    if (role === "root") return next("/roothome");
    if (role === "admin") return next("/admin");
    return next("/student");
  }

  return next();
});

export default router;
