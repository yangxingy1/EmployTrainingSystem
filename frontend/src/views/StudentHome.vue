<script setup>
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { cancelTrainingAttempt, getMyTasks, getStudentTrainingHistory, startTrainingAttempt } from "../api/task";
import { BACKEND_API, getLauncherStatus, getTrainingStatus, launcherErrorMessage, startUnityEntry, startUnityTraining } from "../api/launcher";

const router = useRouter();
const tasks = ref([]);
const history = ref([]);
const loading = ref(false);
const startingId = ref(null);
const errorMsg = ref("");
const activeAttempt = ref(null);
const expandedAttempts = ref(new Set());
const expandedSubResults = ref(new Set());
const username = localStorage.getItem("username") || "学员";
const userId = Number(localStorage.getItem("user_id"));

const doneCount = computed(() => tasks.value.filter((task) => normalizeStatus(task.status) === "done").length);
const runningCount = computed(() => tasks.value.filter((task) => normalizeStatus(task.status) === "running").length);
const pendingCount = computed(() => tasks.value.filter((task) => normalizeStatus(task.status) === "pending").length);
const completionRate = computed(() => tasks.value.length ? Math.round((doneCount.value / tasks.value.length) * 100) : 0);

function normalizeStatus(status) {
  if (["done", "completed", "已完成"].includes(status)) return "done";
  if (["running", "进行中"].includes(status)) return "running";
  return "pending";
}

function statusText(status) {
  const normalized = normalizeStatus(status);
  if (normalized === "done") return "已完成";
  if (normalized === "running") return "训练中";
  return "待开始";
}

function subStatusText(status) {
  return status === "done" ? "已完成" : "未完成";
}

function severityText(severity) {
  if (severity === "safety") return "安全";
  if (severity === "warning") return "警告";
  return "普通";
}

function formatTime(value) {
  return value ? new Date(value).toLocaleString() : "-";
}

function attemptKey(item) {
  return `attempt-${item.attempt_id}`;
}

function subKey(item, sub) {
  return `${item.attempt_id}-${sub.sub_task_id}`;
}

function isAttemptExpanded(item) {
  return expandedAttempts.value.has(attemptKey(item));
}

function isSubExpanded(item, sub) {
  return expandedSubResults.value.has(subKey(item, sub));
}

function toggleSet(target, key) {
  const next = new Set(target.value);
  if (next.has(key)) next.delete(key);
  else next.add(key);
  target.value = next;
}

function toggleAttempt(item) {
  toggleSet(expandedAttempts, attemptKey(item));
}

function toggleSub(item, sub) {
  toggleSet(expandedSubResults, subKey(item, sub));
}

async function loadTasks() {
  if (!userId) {
    router.replace("/login");
    return;
  }
  loading.value = true;
  errorMsg.value = "";
  try {
    const [taskRes, historyRes] = await Promise.all([
      getMyTasks(userId),
      getStudentTrainingHistory(userId),
    ]);
    tasks.value = taskRes.data || [];
    history.value = historyRes.data || [];
  } catch (error) {
    errorMsg.value = "数据加载失败，请稍后重试。";
  } finally {
    loading.value = false;
  }
}

async function startTraining(task) {
  if (!task.assignment_id || startingId.value) return;
  startingId.value = task.assignment_id;
  try {
    const res = await startTrainingAttempt({
      student_id: userId,
      assignment_id: task.assignment_id,
    });
    activeAttempt.value = res.data;
    expandedAttempts.value = new Set([attemptKey(res.data)]);
    await loadTasks();
  } catch (error) {
    alert(error.response?.data?.detail || "创建训练记录失败");
  } finally {
    startingId.value = null;
  }
}

async function launchTraining(task) {
  if (!task.assignment_id || startingId.value) return;
  startingId.value = task.assignment_id;
  let createdAttempt = null;

  try {
    const [launcherStatus, trainingStatus] = await Promise.all([
      getLauncherStatus(),
      getTrainingStatus(),
    ]);

    if (launcherStatus.data?.running || trainingStatus.data?.running) {
      alert("Unity 训练已在运行，请先完成或关闭当前训练。");
      return;
    }

    if (launcherStatus.data?.exe_exists === false) {
      alert("Unity 训练程序不存在，请检查 launcher 配置中的 trainer_exe。");
      return;
    }

    const res = await startTrainingAttempt({
      student_id: userId,
      assignment_id: task.assignment_id,
    });
    createdAttempt = res.data;

    const launchRes = await startUnityTraining({
      student_id: userId,
      username,
      assignment_id: task.assignment_id,
      task_id: createdAttempt.task_id || task.id,
      attempt_id: createdAttempt.attempt_id,
      scene_name: createdAttempt.scene_name || task.scene_name,
      backend_url: BACKEND_API,
    });

    if (!launchRes.data?.success) {
      await cancelCreatedAttempt(createdAttempt, launchRes.data?.message || "Launcher start failed");
      alert(launcherErrorMessage({ response: { data: launchRes.data } }));
      return;
    }

    activeAttempt.value = createdAttempt;
    expandedAttempts.value = new Set([attemptKey(createdAttempt)]);
    await loadTasks();
  } catch (error) {
    if (createdAttempt) {
      await cancelCreatedAttempt(createdAttempt, error.message || "Launcher request failed");
    }
    alert(createdAttempt ? launcherErrorMessage(error) : (error.response?.data?.detail || launcherErrorMessage(error)));
  } finally {
    startingId.value = null;
  }
}

async function launchTrainingSystem() {
  if (startingId.value) return;
  startingId.value = "entry";

  try {
    const [launcherStatus, trainingStatus] = await Promise.all([
      getLauncherStatus(),
      getTrainingStatus(),
    ]);

    if (launcherStatus.data?.running || trainingStatus.data?.running) {
      alert("Unity 训练已在运行，请先完成或关闭当前训练。");
      return;
    }

    if (launcherStatus.data?.exe_exists === false) {
      alert("Unity 训练程序不存在，请检查 launcher 配置中的 trainer_exe。");
      return;
    }

    const launchRes = await startUnityEntry({
      student_id: userId,
      username,
      backend_url: BACKEND_API,
    });

    if (!launchRes.data?.success) {
      alert(launcherErrorMessage({ response: { data: launchRes.data } }));
    }
  } catch (error) {
    alert(launcherErrorMessage(error));
  } finally {
    startingId.value = null;
  }
}

async function cancelCreatedAttempt(attempt, reason) {
  if (!attempt?.attempt_id) return;
  try {
    await cancelTrainingAttempt(attempt.attempt_id, {
      student_id: userId,
      reason,
    });
    await loadTasks();
  } catch (cancelError) {
    console.warn("Cancel training attempt failed", cancelError);
  }
}

function logout() {
  localStorage.removeItem("token");
  localStorage.removeItem("username");
  localStorage.removeItem("role");
  localStorage.removeItem("user_id");
  localStorage.removeItem("company_id");
  router.replace("/login");
}

onMounted(loadTasks);
</script>

<template>
  <div class="student-shell">
    <header class="student-header">
      <div class="brand-area">
        <div class="brand-mark">T</div>
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
          <span>训练结果按项目、子项目和步骤错误分层沉淀，方便后续复盘。</span>
        </div>

        <div class="completion-card">
          <span>完成率</span>
          <strong>{{ completionRate }}%</strong>
          <div class="progress-line"><span :style="{ width: `${completionRate}%` }"></span></div>
        </div>
      </section>

      <section v-if="activeAttempt" class="active-attempt">
        <div>
          <strong>当前训练 #{{ activeAttempt.attempt_id }}</strong>
          <span>{{ activeAttempt.task_title }} / {{ activeAttempt.scene_name }}</span>
        </div>
        <p>Unity Play 配置 studentId={{ userId }} 后，会向这条训练会话继续上传子项目结果。</p>
      </section>

      <section class="stat-row">
        <div><span>任务总数</span><strong>{{ tasks.length }}</strong></div>
        <div><span>待开始</span><strong>{{ pendingCount }}</strong></div>
        <div><span>训练中</span><strong>{{ runningCount }}</strong></div>
        <div><span>已完成</span><strong>{{ doneCount }}</strong></div>
      </section>

      <div v-if="errorMsg" class="error-box">{{ errorMsg }}</div>

      <section class="task-panel">
        <div class="panel-heading">
          <div>
            <h2>训练列表</h2>
            <p>{{ loading ? "正在加载任务..." : "当前开放 lead-train1 和 train2。" }}</p>
          </div>
        </div>

        <article class="training-system-entry">
          <div>
            <strong>进入训练系统</strong>
            <span>从 Unity entry 场景进入大厅，自由选择正式训练或自由练习。</span>
          </div>
          <button class="start-btn" :disabled="startingId === 'entry'" @click="launchTrainingSystem">
            {{ startingId === "entry" ? "启动中" : "进入系统" }}
          </button>
        </article>

        <div class="task-list">
          <article v-for="task in tasks" :key="task.assignment_id || task.id" class="task-row">
            <div class="task-title">
              <span :class="['status-pill', normalizeStatus(task.status)]">{{ statusText(task.status) }}</span>
              <div>
                <h3>{{ task.title }}</h3>
                <p>{{ task.description || "暂无训练说明" }}</p>
                <small>{{ task.scene_name }}</small>
              </div>
            </div>

            <div class="task-actions">
              <small>#{{ task.assignment_id || task.id }}</small>
              <button class="start-btn" :disabled="startingId === task.assignment_id" @click="launchTraining(task)">
                {{ normalizeStatus(task.status) === "done" ? "再次训练" : startingId === task.assignment_id ? "创建中" : "开始训练" }}
              </button>
            </div>
          </article>

          <div v-if="!tasks.length && !loading" class="empty-box">暂无训练任务，请等待管理员分配。</div>
        </div>
      </section>

      <section class="task-panel history-panel">
        <div class="panel-heading">
          <div>
            <h2>学习历史</h2>
            <p>点击一条历史记录查看子项目成绩，再展开子项目查看每一步错误详情。</p>
          </div>
        </div>

        <div class="history-list">
          <article v-for="item in history" :key="item.attempt_id" class="history-card">
            <button class="history-main" @click="toggleAttempt(item)">
              <div>
                <strong>#{{ item.attempt_id }} {{ item.task_title || item.scene_name }}</strong>
                <span>{{ item.scene_name }} · {{ item.completed_sub_count || 0 }}/{{ item.total_sub_count || item.sub_results?.length || 0 }} 子项目完成</span>
              </div>
              <div class="score-box">
                <strong>{{ item.score ?? "-" }}</strong>
                <span>{{ formatTime(item.finished_at || item.started_at) }}</span>
              </div>
            </button>

            <div v-if="isAttemptExpanded(item)" class="sub-list">
              <section v-for="sub in item.sub_results" :key="sub.sub_task_id" class="sub-card" :class="{ pending: sub.status !== 'done' }">
                <button class="sub-main" @click="toggleSub(item, sub)">
                  <div>
                    <strong>{{ sub.sub_task_name }}</strong>
                    <span>{{ subStatusText(sub.status) }} · 错误 {{ sub.error_count || 0 }} / 安全 {{ sub.safety_error_count || 0 }}</span>
                  </div>
                  <div class="sub-score">
                    <strong>{{ sub.score ?? "-" }}</strong>
                    <span>{{ sub.train_time ? `${sub.train_time}s` : "-" }}</span>
                  </div>
                </button>

                <div v-if="isSubExpanded(item, sub)" class="detail-box">
                  <div class="detail-actions">
                    <p>{{ sub.summary || "暂无摘要" }}</p>
                    <button disabled>AI 分析</button>
                  </div>

                  <div class="step-grid">
                    <div v-for="step in sub.steps" :key="`${sub.sub_task_id}-step-${step.index}`" class="step-row">
                      <strong>{{ Number(step.index) + 1 }}. {{ step.name || step.stepName || "未命名步骤" }}</strong>
                      <span>{{ step.expectedAction || step.expected_action || "未记录期望动作" }}</span>
                      <small>{{ step.completed ? "已完成" : "未完成" }} · 错误 {{ step.mistakeCount || step.mistake_count || 0 }}</small>
                    </div>
                  </div>

                  <div class="error-list">
                    <article v-for="(error, index) in sub.errors" :key="`${sub.sub_task_id}-error-${index}`" class="error-item">
                      <div>
                        <strong>{{ error.stepName || error.step_name || `错误 ${index + 1}` }}</strong>
                        <span>{{ error.reason || "未记录原因" }}</span>
                      </div>
                      <p>{{ error.consequence || "暂无后果说明" }}</p>
                      <small>{{ severityText(error.severity) }} · {{ error.time ? `${Number(error.time).toFixed(1)}s` : "-" }}</small>
                    </article>
                    <div v-if="!sub.errors?.length" class="muted-box">暂无错误详情。</div>
                  </div>
                </div>
              </section>
            </div>
          </article>

          <div v-if="!history.length && !loading" class="empty-box">暂无学习历史。</div>
        </div>
      </section>
    </main>
  </div>
</template>

<style scoped>
.student-shell { min-height: 100vh; background: var(--page-bg); }
.student-header { display: flex; align-items: center; justify-content: space-between; gap: 16px; padding: 18px clamp(18px, 4vw, 36px); border-bottom: 1px solid var(--border); background: var(--surface); box-shadow: 0 1px 3px rgba(16,24,32,0.06); }
.brand-area { display: flex; align-items: center; gap: 12px; }
.brand-mark { width: 38px; height: 38px; display: grid; place-items: center; border-radius: var(--radius-lg); color: #0d3636; background: #dff5f1; font-size: 20px; font-weight: 900; }
.brand-area strong, .user-area strong { display: block; color: var(--heading); }
.brand-area span, .user-area span { display: block; color: var(--text-muted); font-size: 12px; }
.user-area { display: flex; align-items: center; gap: 10px; text-align: right; }
.ghost-btn, .logout-btn, .start-btn { min-height: 38px; padding: 0 16px; border-radius: var(--radius); font-weight: 800; transition: all var(--transition); }
.ghost-btn { color: var(--primary-strong); background: var(--primary-soft); }
.logout-btn { color: var(--text); background: var(--surface); border: 1px solid var(--border); }
.start-btn { min-width: 102px; color: #fff; background: var(--primary); }
.start-btn:disabled { opacity: 0.5; cursor: not-allowed; }
.student-main { max-width: 1180px; margin: 0 auto; padding: 28px clamp(18px, 4vw, 36px) 42px; }
.overview, .stat-row, .task-row, .active-attempt { display: grid; gap: 16px; }
.overview { grid-template-columns: minmax(0, 1fr) 240px; align-items: stretch; margin-bottom: 18px; }
.eyebrow { color: var(--accent); font-size: 12px; font-weight: 900; text-transform: uppercase; }
.overview h1 { margin: 6px 0; font-size: 32px; }
.overview span, .panel-heading p, .task-title p { color: var(--text-muted); line-height: 1.6; }
.completion-card, .task-panel, .active-attempt { padding: 20px; border: 1px solid var(--border); border-radius: var(--radius-lg); background: var(--surface); box-shadow: var(--shadow-sm); }
.completion-card strong { display: block; margin: 8px 0; font-size: 34px; color: var(--heading); }
.progress-line { height: 8px; overflow: hidden; border-radius: 999px; background: #e8eef2; }
.progress-line span { display: block; height: 100%; background: var(--primary); }
.active-attempt { grid-template-columns: 280px minmax(0, 1fr); align-items: center; margin-bottom: 18px; border-left: 4px solid var(--accent); }
.active-attempt span { display: block; margin-top: 4px; color: var(--text-muted); }
.stat-row { grid-template-columns: repeat(4, minmax(0, 1fr)); margin-bottom: 18px; }
.stat-row > div { padding: 16px; border: 1px solid var(--border); border-radius: var(--radius-lg); background: var(--surface); }
.stat-row span { color: var(--text-muted); }
.stat-row strong { display: block; margin-top: 6px; color: var(--heading); font-size: 28px; }
.error-box, .empty-box, .muted-box { padding: 14px; border-radius: var(--radius); background: var(--danger-soft); color: var(--danger); font-weight: 700; }
.muted-box { color: var(--text-muted); background: #f4f7f8; }
.panel-heading { display: flex; justify-content: space-between; gap: 16px; margin-bottom: 16px; }
.panel-heading h2 { margin-bottom: 4px; font-size: 22px; }
.training-system-entry { display: flex; align-items: center; justify-content: space-between; gap: 16px; margin-bottom: 14px; padding: 16px; border: 1px solid #b7d7d4; border-radius: var(--radius); background: #eef9f7; }
.training-system-entry strong { display: block; color: var(--heading); font-size: 18px; }
.training-system-entry span { display: block; margin-top: 4px; color: var(--text-muted); line-height: 1.5; }
.task-list, .history-list, .sub-list, .error-list { display: grid; gap: 10px; }
.task-row { grid-template-columns: minmax(0, 1fr) auto; align-items: center; padding: 16px; border: 1px solid var(--border); border-radius: var(--radius); background: #fff; }
.task-title { display: grid; grid-template-columns: 92px minmax(0, 1fr); gap: 14px; align-items: start; }
.task-title h3 { margin-bottom: 6px; font-size: 18px; }
.task-title small { color: var(--accent); font-weight: 800; }
.status-pill { display: inline-grid; place-items: center; height: 28px; border-radius: 999px; font-size: 12px; font-weight: 900; }
.status-pill.pending { color: #8a5a00; background: #fff4d6; }
.status-pill.running { color: #075a86; background: #dff2ff; }
.status-pill.done { color: #16653f; background: #dbf7e8; }
.task-actions { display: flex; align-items: center; gap: 12px; }
.history-panel { margin-top: 18px; }
.history-card { overflow: hidden; border: 1px solid var(--border); border-radius: var(--radius-lg); background: #fff; }
.history-main, .sub-main { width: 100%; display: grid; grid-template-columns: minmax(0, 1fr) 120px; gap: 12px; align-items: center; padding: 16px; text-align: left; background: transparent; }
.history-main:hover, .sub-main:hover { background: #f8fbfb; }
.history-main strong, .sub-main strong { display: block; color: var(--heading); }
.history-main span, .sub-main span, .score-box span, .sub-score span { display: block; margin-top: 4px; color: var(--text-muted); font-size: 12px; }
.score-box, .sub-score { text-align: right; }
.score-box strong, .sub-score strong { display: block; color: var(--heading); font-size: 28px; }
.sub-list { padding: 0 14px 14px; }
.sub-card { border: 1px solid var(--border); border-radius: var(--radius); background: #fbfdfd; }
.sub-card.pending { background: #f8fafb; }
.detail-box { display: grid; gap: 12px; padding: 0 16px 16px; }
.detail-actions { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 12px; border-radius: var(--radius); background: #f4f8f8; }
.detail-actions p { white-space: pre-line; color: var(--text); line-height: 1.6; }
.detail-actions button { height: 32px; padding: 0 12px; border-radius: var(--radius); color: var(--text-muted); background: #e6ecef; font-weight: 800; }
.step-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 8px; }
.step-row { padding: 10px; border: 1px solid var(--border); border-radius: var(--radius); background: #fff; }
.step-row strong, .step-row span, .step-row small { display: block; }
.step-row span, .step-row small { margin-top: 4px; color: var(--text-muted); }
.error-item { display: grid; grid-template-columns: minmax(0, 1fr) minmax(220px, 1.2fr) 88px; gap: 12px; padding: 12px; border: 1px solid #f0c9c2; border-radius: var(--radius); background: #fffafa; }
.error-item span, .error-item p, .error-item small { color: var(--text-muted); line-height: 1.5; }
@media (max-width: 760px) {
  .student-header, .user-area { align-items: stretch; flex-direction: column; text-align: left; }
  .overview, .stat-row, .task-row, .active-attempt, .history-main, .sub-main, .error-item { grid-template-columns: 1fr; }
  .task-title, .step-grid { grid-template-columns: 1fr; }
  .training-system-entry { align-items: stretch; flex-direction: column; }
  .task-actions { align-items: stretch; flex-direction: column; }
  .score-box, .sub-score { text-align: left; }
}
</style>
