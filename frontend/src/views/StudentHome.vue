<script setup>
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { getMyTasks } from "../api/task";

const router = useRouter();
const tasks = ref([]);
const loading = ref(false);
const errorMsg = ref("");
const username = localStorage.getItem("username") || "学员";
const userId = Number(localStorage.getItem("user_id"));

const doneCount = computed(() => {
  return tasks.value.filter((task) => normalizeStatus(task.status) === "done").length;
});

const pendingCount = computed(() => {
  return tasks.value.filter((task) => normalizeStatus(task.status) === "pending").length;
});

// 统一状态值
function normalizeStatus(status) {
  if (["done", "completed", "已完成"].includes(status)) return "done";
  if (["running", "进行中"].includes(status)) return "running";
  return "pending";
}

function statusText(status) {
  const normalized = normalizeStatus(status);
  if (normalized === "done") return "已完成";
  if (normalized === "running") return "进行中";
  return "未开始";
}

// 加载当前学员的训练任务
async function loadTasks() {
  if (!userId) { router.replace("/login"); return; }
  loading.value = true;
  errorMsg.value = "";
  try {
    const res = await getMyTasks(userId);
    tasks.value = res.data;
  } catch (error) {
    errorMsg.value = "任务加载失败，请稍后重试。";
  } finally {
    loading.value = false;
  }
}

// 点击"开始训练"：绑定 launcher + 启动训练 exe
async function startTraining(assignmentId, taskId) {
  if (!assignmentId) return;
  try {
    // 1. 绑定学员身份
    await fetch("http://127.0.0.1:9000/bind", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        student_id: userId,
        username,
        token: localStorage.getItem("token")
      })
    });

    // 2. 启动训练
    const res = await fetch("http://127.0.0.1:9000/start", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ assignment_id: assignmentId, task_id: taskId })
    });

    const data = await res.json();
    if (data.success) {
      alert("训练已启动！");
      loadTasks();
    } else {
      alert("启动失败，请确认 launcher 已运行");
    }
  } catch (e) {
    alert("无法连接 launcher (127.0.0.1:9000)，请先启动 launcher.py");
  }
}

// 退出登录
function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("username");
  localStorage.removeItem("role");
  localStorage.removeItem("user_id");
  router.replace("/login");
}

// 页面加载：获取任务列表
onMounted(() => {
  loadTasks();
});
</script>

<template>
  <div class="student-shell">
    <header class="student-header">
      <div>
        <p class="eyebrow">Training Center</p>
        <h1>我的训练任务</h1>
        <span>你好，<strong class="role-text">学员</strong> {{ username }}，请按照分配内容完成手势训练。</span>
      </div>

      <button class="logout-btn" @click="logout">退出登录</button>
    </header>

    <main class="student-main">
      <section class="stat-row">
        <div>
          <span>任务总数</span>
          <strong>{{ tasks.length }}</strong>
        </div>
        <div>
          <span>待开始</span>
          <strong>{{ pendingCount }}</strong>
        </div>
        <div>
          <span>已完成</span>
          <strong>{{ doneCount }}</strong>
        </div>
      </section>

      <div v-if="errorMsg" class="error-box">
        {{ errorMsg }}
      </div>

      <section class="task-grid">
        <article v-for="task in tasks" :key="task.assignment_id || task.id" class="task-card">
          <div class="task-card-head">
            <span :class="['status-pill', normalizeStatus(task.status)]">
              {{ statusText(task.status) }}
            </span>
            <small>#{{ task.assignment_id || task.id }}</small>
          </div>

          <h2>{{ task.title }}</h2>
          <p>{{ task.description || "暂无训练说明" }}</p>

          <button
            class="start-btn"
            @click="startTraining(task.assignment_id, task.id)"
            :disabled="normalizeStatus(task.status) !== 'pending'"
          >
            {{ normalizeStatus(task.status) === 'running' ? '训练中...' : normalizeStatus(task.status) === 'done' ? '已完成' : '开始训练' }}
          </button>
        </article>

        <div v-if="!tasks.length && !loading" class="empty-box">
          暂无训练任务，请等待管理员分配。
        </div>
      </section>
    </main>
  </div>
</template>

<style scoped>
.student-shell {
  min-height: 100vh;
  background:
    linear-gradient(180deg, rgba(47, 111, 115, 0.06), transparent 240px),
    var(--page-bg);
}

.student-header {
  min-height: 92px;
  padding: 24px 34px;
  color: white;
  background: linear-gradient(135deg, #1a3a4a, #2c4f6e);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
}

.eyebrow {
  color: #ffd9bf;
  font-size: 12px;
  font-weight: 800;
  letter-spacing: 0;
}

.student-header h1 {
  margin: 4px 0 8px;
  color: white;
  font-size: 28px;
}

.student-header span {
  color: rgba(255, 255, 255, 0.76);
}

.student-header .role-text {
  color: #ffd9bf;
  font-size: 17px;
}

.logout-btn,
.start-btn {
  height: 40px;
  border-radius: var(--radius-sm);
  padding: 0 18px;
  font-weight: 800;
  cursor: pointer;
  transition: all var(--transition);
}

.logout-btn {
  color: #263b4a;
  background: white;
}

.logout-btn:hover {
  background: #f0f4f8;
}

.start-btn {
  width: 100%;
  margin-top: 18px;
  color: #e8f5f3;
  background: #1a5c60;
  box-shadow: 0 6px 16px rgba(26, 92, 96, 0.22);
}

.start-btn:hover:not(:disabled) {
  background: #14484b;
  box-shadow: 0 10px 24px rgba(26, 92, 96, 0.35);
}

.start-btn:disabled {
  opacity: 0.45;
  cursor: not-allowed;
  box-shadow: none;
}

.student-main {
  max-width: 1160px;
  margin: 0 auto;
  padding: 28px;
}

.stat-row {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 16px;
  margin-bottom: 20px;
}

.stat-row div,
.task-card,
.empty-box,
.error-box {
  border-radius: var(--radius);
  background: var(--surface);
  border: 1px solid var(--border);
  box-shadow: var(--shadow-sm);
  transition: all var(--transition);
}

.stat-row div {
  padding: 22px;
}

.stat-row div:hover {
  box-shadow: var(--shadow);
  transform: translateY(-2px);
}

.stat-row span,
.task-card p,
.task-card small {
  color: var(--text-muted);
}

.stat-row strong {
  display: block;
  margin-top: 10px;
  color: var(--heading);
  font-size: 32px;
}

.error-box {
  margin-bottom: 18px;
  padding: 14px 18px;
  color: var(--danger);
  background: #fff1ef;
}

.task-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
  gap: 18px;
}

.task-card {
  padding: 22px;
}

.task-card:hover {
  box-shadow: var(--shadow);
  transform: translateY(-3px);
}

.task-card-head {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
}

.task-card h2 {
  margin-bottom: 10px;
  font-size: 21px;
}

.task-card p {
  min-height: 48px;
  line-height: 1.7;
}

.status-pill {
  display: inline-flex;
  min-height: 28px;
  align-items: center;
  padding: 4px 12px;
  border-radius: var(--radius-full);
  font-size: 13px;
  font-weight: 800;
}

.status-pill.pending {
  color: var(--warning);
  background: #fff7e6;
}

.status-pill.running {
  color: var(--primary-strong);
  background: #e9f4f3;
}

.status-pill.done {
  color: var(--success);
  background: #e8f6ef;
}

.empty-box {
  grid-column: 1 / -1;
  min-height: 180px;
  display: grid;
  place-items: center;
  color: var(--text-muted);
  font-size: 16px;
}

@media (max-width: 720px) {
  .student-header,
  .stat-row {
    grid-template-columns: 1fr;
  }

  .student-header {
    flex-direction: column;
    align-items: flex-start;
    padding: 20px;
  }

  .student-main {
    padding: 18px;
  }
}
</style>
