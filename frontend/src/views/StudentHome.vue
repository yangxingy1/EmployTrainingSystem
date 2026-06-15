<script setup>
// 学员训练中心 —— 查看任务、启动 Unity 训练
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { getMyTasks } from "../api/task";
import { api, LAUNCHER_URL } from "../config";

const router = useRouter();
const tasks = ref([]);           // 学员分配到的训练任务列表
const loading = ref(false);
const errorMsg = ref("");
const username = localStorage.getItem("username") || "学员";
const userId = Number(localStorage.getItem("user_id"));

// 计算属性: 各状态任务数量
const doneCount = computed(() => tasks.value.filter((task) => normalizeStatus(task.status) === "done").length);
const runningCount = computed(() => tasks.value.filter((task) => normalizeStatus(task.status) === "running").length);
const pendingCount = computed(() => tasks.value.filter((task) => normalizeStatus(task.status) === "pending").length);
const completionRate = computed(() => {
  return tasks.value.length ? Math.round((doneCount.value / tasks.value.length) * 100) : 0;
});

// 统一状态值: 兼容旧数据中的中文状态
function normalizeStatus(status) {
  if (["done", "completed", "已完成"].includes(status)) return "done";
  if (["running", "进行中"].includes(status)) return "running";
  return "pending";
}

// 状态中文显示文字
function statusText(status) {
  const normalized = normalizeStatus(status);
  if (normalized === "done") return "已完成";
  if (normalized === "running") return "进行中";
  return "未开始";
}

// 从后端加载当前学员的任务列表
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

// 启动训练: 先 bind launcher 绑定学员信息, 再 start 启动 Unity exe
async function startTraining(assignmentId, taskId) {
  if (!assignmentId) return;
  try {
    // 第一步: 向 launcher 绑定学员身份
    await fetch(`${LAUNCHER_URL}/bind`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({
        student_id: userId,
        username,
        token: localStorage.getItem("token")
      })
    });

    // 第二步: 通知 launcher 启动 Unity 训练程序
    const res = await fetch(`${LAUNCHER_URL}/start`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ assignment_id: assignmentId, task_id: taskId })
    });

    const data = await res.json();
    if (data.success) {
      alert("训练已启动！");
      loadTasks();   // 刷新任务状态
    } else {
      alert("启动失败，请确认 launcher 已运行");
    }
  } catch (e) {
    alert(`无法连接 launcher (${LAUNCHER_URL})，请先启动 launcher.py`);
  }
}

// 退出登录 —— 清除 localStorage 并跳转登录页
function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("username");
  localStorage.removeItem("role");
  localStorage.removeItem("user_id");
  router.replace("/login");
}

// 页面挂载时立即加载训练任务
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
/* ---- 学员训练中心 ---- */
.student-shell { min-height: 100vh; background: var(--page-bg); }

/* ---- 顶栏 ---- */
.student-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 18px clamp(18px, 4vw, 36px);
  border-bottom: 1px solid var(--border);
  background: var(--surface);
  box-shadow: 0 1px 3px rgba(16,24,32,0.06);
}

.student-header .brand-area { display: flex; align-items: center; gap: 12px; }

.student-header .brand-mark {
  width: 38px; height: 38px;
  display: grid; place-items: center;
  border-radius: var(--radius-lg);
  color: #0d3636;
  background: #dff5f1;
  font-size: 20px; font-weight: 900;
}

.student-header strong { display: block; color: var(--heading); font-size: 17px; }
.student-header .brand-area span { display: block; color: var(--text-muted); font-size: 12px; }

/* ---- 按钮 ---- */
.ghost-btn, .logout-btn, .start-btn {
  height: 38px; padding: 0 16px;
  border-radius: var(--radius);
  font-weight: 700;
  transition: all var(--transition);
}

.ghost-btn { color: var(--primary-strong); background: var(--primary-soft); }
.ghost-btn:hover { color: #ffffff; background: var(--primary); }

.logout-btn { color: var(--text); background: var(--surface); border: 1px solid var(--border); }
.logout-btn:hover { border-color: var(--danger); color: var(--danger); }

.start-btn { min-width: 102px; color: #ffffff; background: var(--primary); }
.start-btn:hover:not(:disabled) { background: var(--primary-strong); box-shadow: 0 6px 18px rgba(20,112,111,0.24); }
.start-btn:disabled { opacity: 0.45; }

.ghost-btn:hover, .logout-btn:hover, .start-btn:hover:not(:disabled) { transform: translateY(-1px); }

/* ---- 主内容 ---- */
.student-main { max-width: 1180px; margin: 0 auto; padding: 28px clamp(18px, 4vw, 36px) 42px; }

/* ---- 概览 ---- */
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
  border-radius: var(--radius-lg);
  background: var(--surface);
  box-shadow: var(--shadow-sm);
}

.overview > div:first-child { padding: 24px; }

.eyebrow { color: var(--accent); font-size: 12px; font-weight: 900; text-transform: uppercase; }
.overview h1 { margin: 8px 0 8px; font-size: 32px; }
.overview > div:first-child > span { color: var(--text-muted); }

.completion-card { padding: 22px; }
.completion-card > span { display: block; color: var(--text-muted); }
.completion-card strong { display: block; margin: 10px 0 14px; color: var(--heading); font-size: 38px; line-height: 1; }

.progress-line { height: 9px; border-radius: 999px; overflow: hidden; background: #e0e8ec; }
.progress-line span { display: block; height: 100%; border-radius: inherit; background: linear-gradient(90deg, var(--primary), var(--accent)); transition: width 0.4s ease; }

/* ---- 统计行 ---- */
.stat-row {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
  margin-bottom: 18px;
}

.stat-row div { padding: 18px; border-left: 4px solid var(--primary); }
.stat-row div:nth-child(2) { border-left-color: var(--warning); }
.stat-row div:nth-child(4) { border-left-color: var(--success); }
.stat-row span { color: var(--text-muted); }
.stat-row strong { display: block; margin-top: 8px; color: var(--heading); font-size: 30px; }

/* ---- 错误提示 ---- */
.error-box {
  margin-bottom: 16px; padding: 12px 14px;
  color: var(--danger); background: var(--danger-soft);
  border: 1px solid #ffd1cc; border-radius: var(--radius);
  font-weight: 700;
}

/* ---- 任务面板 ---- */
.task-panel { padding: 20px; }
.panel-heading { margin-bottom: 14px; }
.panel-heading h2 { margin-bottom: 4px; font-size: 22px; }
.panel-heading p { color: var(--text-muted); }

.task-list { display: grid; gap: 10px; }

.task-row {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 16px; align-items: center;
  padding: 16px;
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  background: var(--surface-soft);
  transition: border-color var(--transition);
}

.task-row:hover { border-color: var(--primary-soft); }

.task-title { display: grid; grid-template-columns: 90px minmax(0, 1fr); gap: 14px; align-items: start; }
.task-title h3 { margin-bottom: 6px; font-size: 18px; }
.task-title p { color: var(--text-muted); line-height: 1.65; }

.task-actions { display: flex; align-items: center; gap: 12px; }
.task-actions small { color: var(--text-muted); }

/* ---- 状态标签 ---- */
.status-pill {
  display: inline-flex; align-items: center; justify-content: center;
  min-height: 28px; padding: 4px 12px;
  border-radius: var(--radius-full);
  font-size: 13px; font-weight: 800; white-space: nowrap;
}

.status-pill.pending { color: var(--warning); background: var(--warning-soft); }
.status-pill.running { color: var(--primary-strong); background: var(--primary-soft); }
.status-pill.done { color: var(--success); background: var(--success-soft); }

.empty-box {
  min-height: 160px; display: grid; place-items: center;
  color: var(--text-muted);
  border: 1px dashed var(--border); border-radius: var(--radius-lg);
  background: var(--surface-soft);
}

/* ---- 响应式 ---- */
@media (max-width: 860px) {
  .student-header, .user-area { align-items: stretch; flex-direction: column; }
  .user-area div { text-align: left; }
  .overview, .stat-row, .task-row { grid-template-columns: 1fr; }
}
@media (max-width: 620px) {
  .task-title { grid-template-columns: 1fr; }
  .task-actions { align-items: stretch; flex-direction: column; }
}
</style>
