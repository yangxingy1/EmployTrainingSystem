<template>
  <div class="login-container">

    <!-- 左侧介绍区域 -->
    <div class="left-panel">
      <div class="left-content">
        <h1>慧动手</h1>

        <h2>
          基于手势识别的工厂员工手部作业虚拟仿真培训平台
        </h2>

        <p class="subtitle">
          面向工业场景的手部作业技能培训系统，
          结合虚拟仿真与手势识别技术，
          提升培训效率与操作规范性。
        </p>
      </div>
    </div>

    <!-- 右侧表单区域 -->
    <div class="right-panel">

      <div class="form-container">

        <h2>
          {{ isRegister ? "创建账号" : "登录" }}
        </h2>

        <!-- 身份 -->
        <div class="form-item">
          <label>身份</label>

          <select v-model="role">
            <option value="student">学员</option>
            <option value="admin">管理员</option>
          </select>
        </div>

        <!-- 用户名 -->
        <div class="form-item">
          <label>用户名</label>

          <input
              type="text"
              v-model="username"
              placeholder="请输入用户名"
          />
        </div>

        <!-- 密码 -->
        <div class="form-item">
          <label>密码</label>

          <input
              type="password"
              v-model="password"
              placeholder="请输入密码"
          />
        </div>

        <!-- 注册模式 -->
        <div
            v-if="isRegister"
            class="form-item"
        >
          <label>确认密码</label>

          <input
              type="password"
              v-model="confirmPassword"
              placeholder="请再次输入密码"
          />
        </div>

        <!-- 错误提示 -->
        <div
            v-if="errorMsg"
            class="error-text"
        >
          {{ errorMsg }}
        </div>

        <div
            v-if="successMsg"
            class="success-text"
        >
          {{ successMsg }}
        </div>

        <!-- 登录/注册按钮 -->
        <button
            class="submit-btn"
            @click="handleSubmit"
        >
          {{ isRegister ? "创建账号" : "登录" }}
        </button>

        <!-- 底部切换 -->
        <div class="switch-area">

          <span v-if="!isRegister">

            没有账号？

            <button
                class="text-btn"
                @click="switchToRegister"
            >
              创建账号
            </button>

          </span>

          <span v-else>

            已有账号？

            <button
                class="text-btn"
                @click="switchToLogin"
            >
              返回登录
            </button>

          </span>

        </div>

      </div>

    </div>

  </div>
</template>

<script setup>
import { ref } from "vue";
import axios from "axios";
import {useRouter} from 'vue-router'

// 当前是登录模式还是注册模式
const isRegister = ref(false);
const role = ref("student");
const username = ref("");
const password = ref("");
const confirmPassword = ref("");
const errorMsg = ref("");
const successMsg = ref("");
const router = useRouter()

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

// 登录/注册提交：先校验 -> 调接口 -> 存 token -> 跳转
async function handleSubmit() {
  errorMsg.value = ''
  successMsg.value = ''

  if (!username.value || !password.value) {
    errorMsg.value = '用户名和密码不能为空'
    return
  }

  if (isRegister.value && password.value !== confirmPassword.value) {
    errorMsg.value = '两次输入的密码不一致'
    return
  }

  try {
    if (isRegister.value) {
      await axios.post('http://127.0.0.1:8000/register', {
        username: username.value,
        password: password.value,
        role: role.value
      })

      isRegister.value = false
      password.value = ''
      confirmPassword.value = ''
      successMsg.value = '账号创建成功，请登录'
      return
    }

    const res = await axios.post('http://127.0.0.1:8000/login', {
      username: username.value,
      password: password.value,
      role: role.value
    })

    if (res.data.success) {

    localStorage.setItem(
        "token",
        res.data.token
    );

    localStorage.setItem(
        "username",
        res.data.username
    );

    localStorage.setItem(
        "role",
        res.data.role
    );

    localStorage.setItem(
        "user_id",
        res.data.user_id
    );


    if (res.data.role === "admin") {

        router.replace("/admin");

    } else {

        router.replace("/student");

    }
}

  } catch (err) {

    if (
        err.response &&
        err.response.data &&
        err.response.data.detail
    ) {

        errorMsg.value =
            err.response.data.detail;

    } else {

        if (err.response && err.response.data && err.response.data.detail) {
            errorMsg.value = err.response.data.detail
        } else {
              errorMsg.value = err.message || "未知错误"
        }

    }

}
}
</script>

<style scoped>

.login-container {
  display: flex;
  width: 100%;
  height: 100vh;
  overflow: hidden;
}

/* 左侧 */

.left-panel {
  flex: 1.2;
  background: linear-gradient(135deg, #1a3a4a 0%, #2f4f6f 50%, #3e6b89 100%);
  color: white;

  display: flex;
  align-items: center;
  justify-content: center;

  padding: 60px;
}

.left-content {
  max-width: 650px;
}

.left-content h1 {
  color: white;
  font-size: 72px;
  font-weight: 700;
  margin-bottom: 20px;
  letter-spacing: 4px;
}

.left-content h2 {
  color: white;
  font-size: 32px;
  line-height: 1.5;
  font-weight: 500;
  margin-bottom: 30px;
}

.subtitle {
  font-size: 18px;
  line-height: 1.8;
  color: #d9e2ec;
}

/* 右侧 */

.right-panel {
  flex: 0.8;

  background: linear-gradient(180deg, #f0f4f8 0%, #f5f7fa 100%);

  display: flex;
  align-items: center;
  justify-content: center;
}

.form-container {

  width: 420px;

  background: white;

  padding: 44px 40px;

  border-radius: var(--radius-lg);

  box-shadow: var(--shadow-lg);
  transition: box-shadow var(--transition);
}

.form-container:hover {
  box-shadow: 0 20px 56px rgba(31, 41, 55, 0.15);
}

/* 登录标题 */

.form-container h2 {
  font-size: 40px;
  font-weight: 700;
  color: #2f4f6f;

  margin-bottom: 30px;
}

/* 表单项 */

.form-item {
  display: flex;
  flex-direction: column;
  margin-bottom: 18px;
}

.form-item label {
  margin-bottom: 8px;
  font-size: 15px;
  font-weight: 600;
  color: #444;
}

.form-item input,
.form-item select {

  height: 48px;

  border: 1px solid #d0d7de;

  border-radius: var(--radius-sm);

  padding: 0 14px;

  font-size: 15px;

  outline: none;

  transition: all var(--transition);
}

.form-item input:focus,
.form-item select:focus {

  border-color: #3e6b89;

  box-shadow: 0 0 0 3px rgba(62, 107, 137, 0.15);
}

/* 错误提示 */

.error-text {

  color: #d9534f;

  font-size: 14px;

  margin-bottom: 15px;
}

.success-text {

  color: #27845f;

  font-size: 14px;

  margin-bottom: 15px;
}

/* 主按钮 */

.submit-btn {

  width: 100%;

  height: 50px;

  border: none;

  border-radius: var(--radius-sm);

  background: linear-gradient(135deg, #3e6b89, #2f4f6f);

  color: white;

  font-size: 17px;

  font-weight: 600;

  cursor: pointer;

  transition: all var(--transition);
  box-shadow: 0 4px 14px rgba(47, 79, 111, 0.25);
}

.submit-btn:hover {
  background: linear-gradient(135deg, #355b75, #1a3a4a);
  box-shadow: 0 6px 20px rgba(47, 79, 111, 0.35);
  transform: translateY(-2px);
}

.submit-btn:active {
  transform: translateY(0);
}

/* 底部切换 */

.switch-area {

  margin-top: 20px;

  text-align: center;

  font-size: 14px;

  color: #666;
}

.text-btn {

  border: none;

  background: transparent;

  color: #3e6b89;

  cursor: pointer;

  font-size: 14px;

  margin-left: 4px;
}

.text-btn:hover {
  text-decoration: underline;
  transform: none;
}

/* 小屏适配 */

@media (max-width: 900px) {

  .left-panel {
    display: none;
  }

  .right-panel {
    flex: 1;
  }

}
</style>
