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
        <p>面向工厂员工的虚拟仿真培训系统，支持管理员派发训练、学员启动 Unity 训练任务，并沉淀训练记录。</p>
      </div>
      <div class="capability-grid">
        <div><span>01</span><strong>角色登录</strong><p>管理员与学员分流进入不同工作台。</p></div>
        <div><span>02</span><strong>训练分配</strong><p>按学员派发工业操作任务。</p></div>
        <div><span>03</span><strong>本地启动</strong><p>连接 launcher 后直接启动虚拟仿真训练。</p></div>
      </div>
    </section>
    <section class="login-panel" aria-label="登录表单">
      <div class="form-card">
        <div class="form-heading">
          <p class="eyebrow">{{ isRegister ? "Create Account" : "Sign In" }}</p>
          <h2>{{ isRegister ? "注册学员" : "欢迎回来" }}</h2>
          <span>{{ isRegister ? "仅支持学员注册，管理员请联系 Root 创建" : "请选择身份后登录系统" }}</span>
        </div>
        <div class="role-switch">
          <button :class="{ active: role === 'student' }" @click="setRole('student')">学员</button>
          <button v-if="!isRegister" :class="{ active: role === 'admin' }" @click="setRole('admin')">管理员</button>
        </div>
        <label class="form-item">
          <span>所属公司</span>
          <select v-model="companyId">
            <option :value="null" disabled>请选择公司</option>
            <option v-for="c in companies" :key="c.id" :value="c.id">{{ c.name }}</option>
          </select>
        </label>
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
          {{ isRegister ? "注册学员" : "登录工作台" }}
        </button>
        <div class="switch-area">
          <span>{{ isRegister ? "已有账号？" : "没有账号？" }}</span>
          <button class="text-btn" @click="isRegister ? switchToLogin() : switchToRegister()">
            {{ isRegister ? "返回登录" : "注册学员" }}
          </button>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup>
// 登录/注册页 —— 学员注册 + 学员/管理员登录 + 公司选择
import { ref, onMounted } from "vue";
import axios from "axios";
import { useRouter } from "vue-router";

const isRegister = ref(false);
const role = ref("student");
const username = ref("");
const password = ref("");
const confirmPassword = ref("");
const companyId = ref(null);
const companies = ref([]);
const errorMsg = ref("");
const successMsg = ref("");
const router = useRouter();

// 加载公司列表供下拉选择
async function loadCompanies() {
  try { const res = await axios.get("http://127.0.0.1:8000/companies"); companies.value = res.data; } catch (e) {}
}

function switchToRegister() { clearForm(); isRegister.value = true; role.value = "student"; }
function switchToLogin() { clearForm(); isRegister.value = false; }
function clearForm() { username.value = ""; password.value = ""; confirmPassword.value = ""; companyId.value = null; errorMsg.value = ""; successMsg.value = ""; }
function setRole(r) { role.value = r; }

// 登录/注册提交: 校验 -> 调接口 -> 存 token -> 跳转
async function handleSubmit() {
  errorMsg.value = ""; successMsg.value = "";
  if (!username.value || !password.value) { errorMsg.value = "用户名和密码不能为空"; return; }
  if (isRegister.value && password.value !== confirmPassword.value) { errorMsg.value = "两次输入的密码不一致"; return; }
  if (!companyId.value) { errorMsg.value = "请选择所属公司"; return; }
  try {
    if (isRegister.value) {
      await axios.post("http://127.0.0.1:8000/register", { username: username.value, password: password.value, role: "student", company_id: companyId.value });
      successMsg.value = "注册成功！请登录。";
      switchToLogin();
      return;
    }
    const res = await axios.post("http://127.0.0.1:8000/login", { username: username.value, password: password.value, company_id: companyId.value });
    if (res.data.success) {
      localStorage.setItem("token", res.data.token); localStorage.setItem("username", res.data.username);
      localStorage.setItem("role", res.data.role); localStorage.setItem("user_id", res.data.user_id);
      if (res.data.company_id) localStorage.setItem("company_id", res.data.company_id);
      router.replace(res.data.role === "admin" ? "/admin" : "/student");
    }
  } catch (err) { errorMsg.value = err.response?.data?.detail || "登录失败"; }
}
onMounted(() => { loadCompanies(); });
</script>

<style scoped>
/* ---- 登录页布局 ---- */
.login-page {
  min-height: 100vh;
  display: grid;
  grid-template-columns: minmax(0, 1.15fr) minmax(420px, 0.85fr);
}

/* ---- 左侧品牌面板 ---- */
.product-panel {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  gap: 44px;
  padding: 48px clamp(34px, 6vw, 86px);
  color: #d9e7ea;
  background: linear-gradient(140deg, rgba(20,112,111,0.84), rgba(21,33,43,0.96)),
              linear-gradient(90deg, transparent 0 48px, rgba(255,255,255,0.05) 48px 49px, transparent 49px 96px);
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
  color: #0d3636;
  background: #dff5f1;
  font-size: 22px;
  font-weight: 900;
}

.brand-row strong {
  color: #ffffff;
  font-size: 20px;
  display: block;
}

.brand-row span {
  margin-top: 2px;
  color: rgba(255,255,255,0.68);
  font-size: 13px;
  display: block;
}

.eyebrow {
  color: #f2b173;
  font-size: 12px;
  font-weight: 800;
  text-transform: uppercase;
}

.product-copy { max-width: 680px; }
.product-copy h1 {
  margin: 14px 0 18px;
  color: #ffffff;
  font-size: clamp(34px, 5vw, 58px);
  line-height: 1.12;
}

.product-copy p:last-child {
  max-width: 600px;
  color: rgba(255,255,255,0.74);
  font-size: 17px;
  line-height: 1.8;
}

/* ---- 能力卡片 ---- */
.capability-grid {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 12px;
}

.capability-grid div {
  min-height: 132px;
  padding: 18px;
  border: 1px solid rgba(255,255,255,0.14);
  border-radius: var(--radius-lg);
  background: rgba(255,255,255,0.07);
  transition: background var(--transition);
}

.capability-grid div:hover {
  background: rgba(255,255,255,0.12);
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
  color: rgba(255,255,255,0.66);
  font-size: 13px;
  line-height: 1.6;
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
  border: 1px solid var(--border);
  border-radius: var(--radius-xl);
  background: var(--surface);
  box-shadow: var(--shadow);
}

.form-heading h2 {
  margin: 8px 0 6px;
  font-size: 32px;
}

.form-heading span {
  color: var(--text-muted);
  font-size: 13px;
}

/* ---- 角色切换 ---- */
.role-switch {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 4px;
  margin: 24px 0 20px;
  padding: 4px;
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  background: var(--surface-muted);
}

.role-switch button {
  width: 100%;
  height: 38px;
  border-radius: var(--radius);
  color: #5a6872;
  background: transparent;
  font-weight: 700;
  transition: all var(--transition);
}

.role-switch button:hover:not(.active) {
  color: var(--text);
  background: rgba(255,255,255,0.5);
}

.role-switch button.active {
  color: #ffffff;
  background: var(--primary);
  box-shadow: 0 4px 14px rgba(20,112,111,0.28);
}

.role-switch button:only-child {
  grid-column: 1 / -1;
}

/* ---- 表单项目 ---- */
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

.form-item input,
.form-item select {
  height: 46px;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 0 12px;
  font-size: 15px;
  outline: none;
  transition: all var(--transition);
}

.form-item input:focus,
.form-item select:focus {
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(47,111,115,0.14);
}

/* ---- 消息提示 ---- */
.message {
  margin-bottom: 14px;
  padding: 10px 12px;
  border-radius: var(--radius);
  font-size: 14px;
  font-weight: 700;
}

.message.error {
  color: var(--danger);
  background: #fff1ef;
  border: 1px solid #ffd1cc;
}

.message.success {
  color: var(--success);
  background: #e8f6ef;
  border: 1px solid #c3e6d6;
}

/* ---- 提交按钮 ---- */
.submit-btn {
  width: 100%;
  height: 48px;
  border-radius: var(--radius-lg);
  color: #ffffff;
  background: var(--primary);
  font-size: 16px;
  font-weight: 700;
  transition: all var(--transition);
}

.submit-btn:hover {
  background: var(--primary-strong);
  box-shadow: 0 8px 22px rgba(20,112,111,0.24);
  transform: translateY(-1px);
}

/* ---- 切换区域 ---- */
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

.text-btn:hover {
  color: var(--primary);
}

@media (max-width: 980px) {
  .login-page { grid-template-columns: 1fr; }
  .product-panel { min-height: auto; }
}
@media (max-width: 680px) {
  .product-panel { padding: 28px 20px; }
  .capability-grid { grid-template-columns: 1fr; }
  .login-panel { padding: 20px; }
  .form-card { padding: 24px; }
}
</style>
