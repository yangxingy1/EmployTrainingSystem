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

const doneCount = computed(() => tasks.value.filter((task) => normalizeStatus(task.status) === "done").length);
const runningCount = computed(() => tasks.value.filter((task) => normalizeStatus(task.status) === "running").length);
const pendingCount = computed(() => tasks.value.filter((task) => normalizeStatus(task.status) === "pending").length);
const completionRate = computed(() => {
  return tasks.value.length ? Math.round((doneCount.value / tasks.value.length) * 100) : 0;
});

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

async function loadTasks() {
  if (!userId) {
    router.replace("/login");
    return;
  }
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

async function startTraining(assignmentId, taskId) {
  if (!assignmentId) return;
  try {
    await fetch("http://127.0.0.1:9000/bind", {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        student_id: userId,
        username,
        token: localStorage.getItem("token")
      })
    });

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

function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("username");
  localStorage.removeItem("role");
  localStorage.removeItem("user_id");
  router.replace("/login");
}

onMounted(() => {
  loadTasks();
});
</script>

<template>
  <div class="student-shell">
    <header class="student-header">
      <div class="brand-area">
        <div class="brand-mark">慧</div>
        <div>
          <strong>慧动手</strong>
          <span>学员训练中心</span>
        </div>
      </div>

      <div class="user-area">
        <div>
          <span>当前学员</span>
          <strong>{{ username }}</strong>
        </div>
        <button class="ghost-btn" @click="loadTasks">刷新</button>
        <button class="logout-btn" @click="logout">退出</button>
      </div>
    </header>

    <main class="student-main">
      <section class="overview">
        <div>
          <p class="eyebrow">Student Workspace</p>
          <h1>我的训练任务</h1>
          <span>按管理员分配的内容完成训练，启动前请确认本地 launcher 已运行。</span>
        </div>

        <div class="completion-card">
          <span>完成率</span>
          <strong>{{ completionRate }}%</strong>
          <div class="progress-line">
            <span :style="{ width: `${completionRate}%` }"></span>
          </div>
        </div>
      </section>

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
          <span>进行中</span>
          <strong>{{ runningCount }}</strong>
        </div>
        <div>
          <span>已完成</span>
          <strong>{{ doneCount }}</strong>
        </div>
      </section>

      <div v-if="errorMsg" class="error-box">{{ errorMsg }}</div>

      <section class="task-panel">
        <div class="panel-heading">
          <div>
            <h2>训练列表</h2>
            <p>{{ loading ? "正在加载任务..." : "选择未开始任务进入 Unity 训练。" }}</p>
          </div>
        </div>

        <div class="task-list">
          <article v-for="task in tasks" :key="task.assignment_id || task.id" class="task-row">
            <div class="task-title">
              <span :class="['status-pill', normalizeStatus(task.status)]">
                {{ statusText(task.status) }}
              </span>
              <div>
                <h3>{{ task.title }}</h3>
                <p>{{ task.description || "暂无训练说明" }}</p>
              </div>
            </div>

            <div class="task-actions">
              <small>#{{ task.assignment_id || task.id }}</small>
              <button
                class="start-btn"
                @click="startTraining(task.assignment_id, task.id)"
                :disabled="normalizeStatus(task.status) !== 'pending'"
              >
                {{ normalizeStatus(task.status) === "running" ? "训练中" : normalizeStatus(task.status) === "done" ? "已完成" : "开始训练" }}
              </button>
            </div>
          </article>

          <div v-if="!tasks.length && !loading" class="empty-box">
            暂无训练任务，请等待管理员分配。
          </div>
        </div>
      </section>
    </main>
  </div>
</template>

<style scoped>
.student-shell {
  min-height: 100vh;
  background: var(--page-bg);
}

.student-header {
  min-height: 72px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 16px clamp(20px, 4vw, 40px);
  border-bottom: 1px solid var(--border);
  background: var(--surface);
}

.brand-area {
  display: flex;
  align-items: center;
  gap: 12px;
}

.brand-mark {
  width: 40px;
  height: 40px;
  display: grid;
  place-items: center;
  border-radius: var(--radius);
  color: #ffffff;
  background: var(--primary);
  font-size: 20px;
  font-weight: 900;
}

.brand-area strong,
.brand-area span {
  display: block;
}

.brand-area span {
  margin-top: 2px;
  color: var(--text-muted);
  font-size: 12px;
}

.user-area {
  display: flex;
  align-items: center;
  gap: 10px;
}

.user-area div {
  padding-right: 6px;
  text-align: right;
}

.user-area span,
.user-area strong {
  display: block;
}

.user-area span {
  color: var(--text-muted);
  font-size: 12px;
}

.ghost-btn,
.logout-btn,
.start-btn {
  height: 38px;
  padding: 0 16px;
  border-radius: var(--radius-sm);
  font-weight: 800;
  transition: background var(--transition), box-shadow var(--transition), transform var(--transition);
}

.ghost-btn {
  color: var(--primary-strong);
  background: var(--primary-soft);
}

.logout-btn {
  color: var(--text);
  background: var(--surface);
  border: 1px solid var(--border);
}

.start-btn {
  min-width: 102px;
  color: #ffffff;
  background: var(--primary);
}

.start-btn:hover:not(:disabled),
.ghost-btn:hover,
.logout-btn:hover {
  transform: translateY(-1px);
  box-shadow: var(--shadow-sm);
}

.start-btn:hover:not(:disabled) {
  background: var(--primary-strong);
}

.start-btn:disabled {
  opacity: 0.48;
}

.student-main {
  max-width: 1180px;
  margin: 0 auto;
  padding: 28px clamp(18px, 4vw, 36px) 42px;
}

.overview {
  display: grid;
  grid-template-columns: minmax(0, 1fr) 260px;
  gap: 18px;
  align-items: stretch;
  margin-bottom: 18px;
}

.overview > div:first-child,
.completion-card,
.stat-row div,
.task-panel {
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--surface);
  box-shadow: var(--shadow-sm);
}

.overview > div:first-child {
  padding: 24px;
}

.eyebrow {
  color: var(--accent);
  font-size: 12px;
  font-weight: 900;
  letter-spacing: 0;
  text-transform: uppercase;
}

.overview h1 {
  margin: 8px 0 8px;
  font-size: 32px;
}

.overview span {
  color: var(--text-muted);
}

.completion-card {
  padding: 22px;
}

.completion-card > span {
  display: block;
  color: var(--text-muted);
}

.completion-card strong {
  display: block;
  margin: 10px 0 14px;
  color: var(--heading);
  font-size: 38px;
  line-height: 1;
}

.progress-line {
  height: 9px;
  border-radius: 999px;
  overflow: hidden;
  background: #e0e8ec;
}

.progress-line span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, var(--primary), var(--accent));
  transition: width 0.4s ease;
}

.stat-row {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
  margin-bottom: 18px;
}

.stat-row div {
  padding: 18px;
  border-left: 4px solid var(--primary);
}

.stat-row div:nth-child(2) {
  border-left-color: var(--warning);
}

.stat-row div:nth-child(4) {
  border-left-color: var(--success);
}

.stat-row span {
  color: var(--text-muted);
}

.stat-row strong {
  display: block;
  margin-top: 8px;
  color: var(--heading);
  font-size: 30px;
}

.error-box {
  margin-bottom: 16px;
  padding: 12px 14px;
  color: var(--danger);
  background: var(--danger-soft);
  border: 1px solid #ffd1cc;
  border-radius: var(--radius-sm);
  font-weight: 700;
}

.task-panel {
  padding: 20px;
}

.panel-heading {
  margin-bottom: 14px;
}

.panel-heading h2 {
  margin-bottom: 4px;
  font-size: 22px;
}

.panel-heading p {
  color: var(--text-muted);
}

.task-list {
  display: grid;
  gap: 10px;
}

.task-row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 16px;
  align-items: center;
  padding: 16px;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--surface-soft);
}

.task-title {
  display: grid;
  grid-template-columns: 90px minmax(0, 1fr);
  gap: 14px;
  align-items: start;
}

.task-title h3 {
  margin-bottom: 6px;
  font-size: 18px;
}

.task-title p {
  color: var(--text-muted);
  line-height: 1.65;
}

.task-actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.task-actions small {
  color: var(--text-muted);
}

.status-pill {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 28px;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 13px;
  font-weight: 900;
  white-space: nowrap;
}

.status-pill.pending {
  color: var(--warning);
  background: var(--warning-soft);
}

.status-pill.running {
  color: var(--primary-strong);
  background: var(--primary-soft);
}

.status-pill.done {
  color: var(--success);
  background: var(--success-soft);
}

.empty-box {
  min-height: 160px;
  display: grid;
  place-items: center;
  color: var(--text-muted);
  border: 1px dashed var(--border);
  border-radius: var(--radius);
  background: var(--surface-soft);
}

@media (max-width: 860px) {
  .student-header,
  .user-area {
    align-items: stretch;
    flex-direction: column;
  }

  .user-area div {
    text-align: left;
  }

  .overview,
  .stat-row,
  .task-row {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 620px) {
  .task-title {
    grid-template-columns: 1fr;
  }

  .task-actions {
    align-items: stretch;
    flex-direction: column;
  }
}
</style>
