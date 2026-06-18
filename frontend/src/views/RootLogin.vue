<template>
  <div class="login-page">
    <section class="product-panel">
      <div class="brand-row">
        <div class="brand-mark">R</div>
        <div>
          <strong>Root Console</strong>
          <span>平台根管理员控制中心</span>
        </div>
      </div>

      <div class="product-copy">
        <p class="eyebrow">Root Management</p>
        <h1>平台级管理入口</h1>
        <p>
          Root账号用于维护公司、管理员以及未来训练项目总库，
          不属于任何公司体系。
        </p>
      </div>
    </section>

    <section class="login-panel">
      <div class="form-card">

        <div class="form-heading">
          <p class="eyebrow">Root Sign In</p>
          <h2>Root 登录</h2>
          <span>仅开发与运营人员可访问</span>
        </div>

        <label class="form-item">
          <span>账号</span>
          <input
            type="text"
            v-model="username"
            placeholder="请输入Root账号"
          />
        </label>

        <label class="form-item">
          <span>密码</span>
          <input
            type="password"
            v-model="password"
            placeholder="请输入密码"
          />
        </label>

        <div
          v-if="errorMsg"
          class="message error"
        >
          {{ errorMsg }}
        </div>

        <button
          class="submit-btn"
          @click="login"
        >
          登录Root控制台
        </button>

      </div>
    </section>
  </div>
</template>

<script setup>
// Root 登录页 —— 平台级管理员专用登录入口
import { ref } from "vue";
import api from "../api/http";
import { useRouter } from "vue-router";

const router = useRouter();

const username = ref("");
const password = ref("");
const errorMsg = ref("");

// 登录: 调用 /root/login 接口，成功后跳转 /roothome
async function login() {
  errorMsg.value = "";
  try {
    const res = await api.post(
      "/root/login",
      {
        username: username.value,
        password: password.value
      }
    );

    if (res.data.success) {
      localStorage.setItem("token", res.data.token);
      localStorage.setItem("username", res.data.username);
      localStorage.setItem("role", "root");
      localStorage.setItem("user_id", res.data.user_id);
      router.replace("/roothome");
    }
  } catch (err) {
    errorMsg.value =
      err.response?.data?.detail ||
      "登录失败";
  }
}
</script>

<style scoped>
/* ---- Root 登录页 ---- */
.login-page {
  min-height: 100vh;
  display: grid;
  grid-template-columns: minmax(0, 1.15fr) minmax(420px, 0.85fr);
}

/* ---- 左侧品牌面板 ---- */
.product-panel {
  display: flex;
  flex-direction: column;
  padding: 48px clamp(34px, 6vw, 86px);
  color: #d9e7ea;
  background: linear-gradient(140deg, rgba(18,28,52,0.96), rgba(7,12,24,0.98));
}

.brand-row {
  display: flex;
  align-items: center;
  gap: 14px;
}

.brand-mark {
  width: 44px;
  height: 44px;
  display: grid;
  place-items: center;
  border-radius: var(--radius-lg);
  background: #f0c674;
  color: #121212;
  font-size: 22px;
  font-weight: 900;
}

.brand-row strong,
.brand-row span {
  display: block;
}

.brand-row strong {
  color: #ffffff;
  font-size: 20px;
}

.brand-row span {
  margin-top: 2px;
  color: rgba(255,255,255,0.65);
  font-size: 13px;
}

.eyebrow {
  color: #f0c674;
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
}

.product-copy {
  max-width: 650px;
  margin-top: 120px;
}

.product-copy h1 {
  margin: 14px 0 18px;
  color: #ffffff;
  font-size: clamp(34px, 5vw, 58px);
  line-height: 1.12;
}

.product-copy p:last-child {
  max-width: 580px;
  color: rgba(255,255,255,0.72);
  font-size: 17px;
  line-height: 1.8;
}

/* ---- 右侧登录面板 ---- */
.login-panel {
  display: grid;
  place-items: center;
  padding: 32px;
  background: var(--page-bg);
}

.form-card {
  width: min(100%, 430px);
  padding: 32px;
  border-radius: var(--radius-xl);
  border: 1px solid var(--border);
  background: var(--surface);
  box-shadow: var(--shadow);
}

.form-heading h2 {
  margin: 8px 0 6px;
  font-size: 32px;
}

.form-heading span {
  color: var(--text-muted);
}

.form-item {
  display: grid;
  gap: 8px;
  margin-bottom: 16px;
}

.form-item span {
  color: var(--heading);
  font-size: 14px;
  font-weight: 800;
}

.form-item input {
  height: 46px;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 0 12px;
  font-size: 15px;
  outline: none;
  transition: all var(--transition);
}

.form-item input:focus {
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(47,111,115,0.14);
}

.message {
  margin-bottom: 14px;
  padding: 10px 12px;
  border-radius: var(--radius);
  font-size: 14px;
  font-weight: 700;
}

.message.error {
  color: var(--danger);
  background: var(--danger-soft);
  border: 1px solid #ffd1cc;
}

.submit-btn {
  width: 100%;
  height: 48px;
  border-radius: var(--radius-lg);
  color: #ffffff;
  background: linear-gradient(135deg, #23395d, #355c9c);
  font-size: 16px;
  font-weight: 700;
  transition: all var(--transition);
}

.submit-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 12px 24px rgba(35,57,93,0.35);
}

@media (max-width: 980px) {
  .login-page { grid-template-columns: 1fr; }
  .product-panel { min-height: auto; }
}

@media (max-width: 680px) {
  .product-panel { padding: 28px 20px; }
  .login-panel { padding: 20px; }
  .form-card { padding: 24px; }
}
</style>
