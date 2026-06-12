<template>
  <div class="login-page">
    <section class="product-panel" aria-label="平台概览">
      <div class="brand-row">
        <div class="brand-mark">慧</div>
        <div>
          <strong>慧动手</strong>
          <span>工业手势仿真培训平台</span>
        </div>
      </div>

      <div class="product-copy">
        <p class="eyebrow">Training Operations</p>
        <h1>把手部作业训练、任务分配和完成进度放到一个控制台里。</h1>
        <p>
          面向工厂员工的虚拟仿真训练系统，支持管理员派发训练、学员启动 Unity 训练任务，并沉淀训练记录。
        </p>
      </div>

      <div class="capability-grid">
        <div>
          <span>01</span>
          <strong>角色登录</strong>
          <p>管理员与学员分流进入不同工作台。</p>
        </div>
        <div>
          <span>02</span>
          <strong>训练分配</strong>
          <p>按学员派发电闸、阀门等工业操作任务。</p>
        </div>
        <div>
          <span>03</span>
          <strong>本地启动</strong>
          <p>连接 launcher 后直接启动虚拟仿真训练。</p>
        </div>
      </div>
    </section>

    <section class="login-panel" aria-label="登录表单">
      <div class="form-card">
        <div class="form-heading">
          <p class="eyebrow">{{ isRegister ? "Create Account" : "Sign In" }}</p>
          <h2>{{ isRegister ? "创建账号" : "欢迎回来" }}</h2>
          <span>{{ isRegister ? "创建后可返回登录进入对应工作台" : "请选择身份后登录系统" }}</span>
        </div>

        <div class="role-switch">
          <button :class="{ active: role === 'student' }" @click="role = 'student'">学员</button>
          <button :class="{ active: role === 'admin' }" @click="role = 'admin'">管理员</button>
        </div>

        <label class="form-item">
          <span>用户名</span>
          <input type="text" v-model="username" placeholder="请输入用户名" />
        </label>

        <label class="form-item">
          <span>密码</span>
          <input type="password" v-model="password" placeholder="请输入密码" />
        </label>

        <label v-if="isRegister" class="form-item">
          <span>确认密码</span>
          <input type="password" v-model="confirmPassword" placeholder="请再次输入密码" />
        </label>

        <div v-if="errorMsg" class="message error">{{ errorMsg }}</div>
        <div v-if="successMsg" class="message success">{{ successMsg }}</div>

        <button class="submit-btn" @click="handleSubmit">
          {{ isRegister ? "创建账号" : "登录工作台" }}
        </button>

        <div class="switch-area">
          <span>{{ isRegister ? "已有账号？" : "没有账号？" }}</span>
          <button class="text-btn" @click="isRegister ? switchToLogin() : switchToRegister()">
            {{ isRegister ? "返回登录" : "创建账号" }}
          </button>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
import { ref } from "vue";
import axios from "axios";
import { useRouter } from "vue-router";

const isRegister = ref(false);
const role = ref("student");
const username = ref("");
const password = ref("");
const confirmPassword = ref("");
const errorMsg = ref("");
const successMsg = ref("");
const router = useRouter();

function switchToRegister() {
  clearForm();
  isRegister.value = true;
}

function switchToLogin() {
  clearForm();
  isRegister.value = false;
}

function clearForm() {
  username.value = "";
  password.value = "";
  confirmPassword.value = "";
  errorMsg.value = "";
  successMsg.value = "";
}

async function handleSubmit() {
  errorMsg.value = "";
  successMsg.value = "";

  if (!username.value || !password.value) {
    errorMsg.value = "用户名和密码不能为空";
    return;
  }

  if (isRegister.value && password.value !== confirmPassword.value) {
    errorMsg.value = "两次输入的密码不一致";
    return;
  }

  try {
    if (isRegister.value) {
      await axios.post("http://127.0.0.1:8000/register", {
        username: username.value,
        password: password.value,
        role: role.value
      });

      isRegister.value = false;
      password.value = "";
      confirmPassword.value = "";
      successMsg.value = "账号创建成功，请登录";
      return;
    }

    const res = await axios.post("http://127.0.0.1:8000/login", {
      username: username.value,
      password: password.value,
      role: role.value
    });

    if (res.data.success) {
      localStorage.setItem("token", res.data.token);
      localStorage.setItem("username", res.data.username);
      localStorage.setItem("role", res.data.role);
      localStorage.setItem("user_id", res.data.user_id);
      router.replace(res.data.role === "admin" ? "/admin" : "/student");
    }
  } catch (err) {
    errorMsg.value = err.response?.data?.detail || err.message || "未知错误";
  }
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  display: grid;
  grid-template-columns: minmax(0, 1.15fr) minmax(420px, 0.85fr);
}

.product-panel {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 44px;
  padding: 48px clamp(34px, 6vw, 86px);
  color: #d9e7ea;
  background:
    linear-gradient(140deg, rgba(20, 112, 111, 0.84), rgba(21, 33, 43, 0.96)),
    linear-gradient(90deg, transparent 0 48px, rgba(255, 255, 255, 0.05) 48px 49px, transparent 49px 96px);
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
  border-radius: var(--radius);
  color: #0d3636;
  background: #dff5f1;
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
  color: rgba(255, 255, 255, 0.68);
  font-size: 13px;
}

.eyebrow {
  color: #f2b173;
  font-size: 12px;
  font-weight: 800;
  letter-spacing: 0;
  text-transform: uppercase;
}

.product-copy {
  max-width: 680px;
}

.product-copy h1 {
  margin: 14px 0 18px;
  color: #ffffff;
  font-size: clamp(34px, 5vw, 58px);
  line-height: 1.12;
}

.product-copy p:last-child {
  max-width: 600px;
  color: rgba(255, 255, 255, 0.74);
  font-size: 17px;
  line-height: 1.8;
}

.capability-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
}

.capability-grid div {
  min-height: 132px;
  padding: 18px;
  border: 1px solid rgba(255, 255, 255, 0.14);
  border-radius: var(--radius);
  background: rgba(255, 255, 255, 0.07);
}

.capability-grid span {
  color: #f2b173;
  font-weight: 900;
  font-size: 12px;
}

.capability-grid strong {
  display: block;
  margin: 10px 0 8px;
  color: #ffffff;
}

.capability-grid p {
  color: rgba(255, 255, 255, 0.66);
  font-size: 13px;
  line-height: 1.6;
}

.login-panel {
  display: grid;
  place-items: center;
  padding: 32px;
  background: var(--page-bg);
}

.form-card {
  width: min(100%, 430px);
  padding: 32px;
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
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

.role-switch {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 6px;
  margin: 24px 0 20px;
  padding: 5px;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--surface-soft);
}

.role-switch button {
  height: 38px;
  border-radius: var(--radius-sm);
  color: var(--text-muted);
  background: transparent;
  font-weight: 800;
  transition: color var(--transition), background var(--transition), box-shadow var(--transition);
}

.role-switch button.active {
  color: #ffffff;
  background: var(--primary);
  box-shadow: 0 8px 18px rgba(20, 112, 111, 0.20);
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
}

.message {
  margin-bottom: 14px;
  padding: 10px 12px;
  border-radius: var(--radius-sm);
  font-size: 14px;
  font-weight: 700;
}

.message.error {
  color: var(--danger);
  background: var(--danger-soft);
  border: 1px solid #ffd1cc;
}

.message.success {
  color: var(--success);
  background: var(--success-soft);
  border: 1px solid #bfdfcf;
}

.submit-btn {
  width: 100%;
  height: 48px;
  border-radius: var(--radius-sm);
  color: #ffffff;
  background: var(--primary);
  font-size: 16px;
  font-weight: 900;
  transition: background var(--transition), box-shadow var(--transition), transform var(--transition);
}

.submit-btn:hover {
  background: var(--primary-strong);
  box-shadow: 0 12px 22px rgba(20, 112, 111, 0.24);
  transform: translateY(-1px);
}

.switch-area {
  display: flex;
  justify-content: center;
  gap: 6px;
  margin-top: 18px;
  color: var(--text-muted);
  font-size: 14px;
}

.text-btn {
  color: var(--primary-strong);
  background: transparent;
  font-weight: 800;
}

@media (max-width: 980px) {
  .login-page {
    grid-template-columns: 1fr;
  }

  .product-panel {
    min-height: auto;
  }
}

@media (max-width: 680px) {
  .product-panel {
    padding: 28px 20px;
  }

  .capability-grid {
    grid-template-columns: 1fr;
  }

  .login-panel {
    padding: 20px;
  }

  .form-card {
    padding: 24px;
  }
}
</style>
