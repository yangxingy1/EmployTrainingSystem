<script setup>
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { cancelTrainingAttempt, getMyTasks, getStudentTrainingHistory, startTrainingAttempt } from "../api/task";
import { BACKEND_API, getLauncherStatus, getTrainingStatus, launcherErrorMessage, startUnityEntry, startUnityTraining } from "../api/launcher";
import ChangePasswordModal from "../components/ChangePasswordModal.vue";

const router = useRouter();
const tasks = ref([]);
const history = ref([]);
const loading = ref(false);
const startingId = ref(null);
const errorMsg = ref("");
const activeAttempt = ref(null);
const expandedHistorySubs = ref(new Set());
const expandedHistoryRecords = ref(new Set());
const showChangePassword = ref(false);
const username = localStorage.getItem("username") || "学员";
const userId = Number(localStorage.getItem("user_id"));

const DEMO_CONFIGS = [
  { key: "demo01", title: "Demo 01", subtitle: "lead-train1", sceneName: "lead-train1" },
  { key: "demo02", title: "Demo 02", subtitle: "train2", sceneName: "train2" },
];

const DEMO_SUBPROJECT_FALLBACKS = {
  "lead-train1": [
    { sub_task_id: "lead_train1_electrical_cabinet_gesture", name: "配电柜主断路器" },
    { sub_task_id: "lead_train1_gesture", name: "Breaker Shutdown" },
    { sub_task_id: "lead_train1_cnc_gesture", name: "CNC 标准加工" },
  ],
  train2: [
    { sub_task_id: "default_sub_task", name: "Demo 02 综合训练" },
  ],
};

const doneCount = computed(() => tasks.value.filter((task) => normalizeStatus(task.status) === "done").length);
const runningCount = computed(() => tasks.value.filter((task) => normalizeStatus(task.status) === "running").length);
const pendingCount = computed(() => tasks.value.filter((task) => normalizeStatus(task.status) === "pending").length);
const completionRate = computed(() => tasks.value.length ? Math.round((doneCount.value / tasks.value.length) * 100) : 0);
const historyDemos = computed(() => DEMO_CONFIGS.map((demo) => buildDemoHistory(demo)));

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

function toggleSet(target, key) {
  const next = new Set(target.value);
  if (next.has(key)) next.delete(key);
  else next.add(key);
  target.value = next;
}

function buildDemoHistory(demo) {
  const attempts = history.value.filter((item) => item.scene_name === demo.sceneName);
  const subMap = new Map();

  for (const fallback of DEMO_SUBPROJECT_FALLBACKS[demo.sceneName] || []) {
    ensureHistorySub(subMap, fallback.sub_task_id, fallback.name, fallback.description);
  }

  for (const attempt of attempts) {
    for (const item of attempt.sub_items || []) {
      ensureHistorySub(subMap, item.sub_task_id, item.name, item.description);
    }

    for (const sub of attempt.sub_results || []) {
      const subItem = ensureHistorySub(
        subMap,
        sub.sub_task_id,
        sub.sub_task_name,
        sub.catalog?.description
      );
      if (sub.status === "done" && Number(sub.score || 0) > 0) {
        subItem.records.push({ attempt, sub });
      }
    }
  }

  const subProjects = Array.from(subMap.values()).map((item) => ({
    ...item,
    records: item.records.sort((a, b) => b.attempt.attempt_id - a.attempt.attempt_id),
    averageScore: averageScore(item.records.map((record) => record.sub.score)),
  }));
  const records = subProjects.flatMap((item) => item.records);

  return {
    ...demo,
    attempts,
    subProjects,
    recordCount: records.length,
  };
}

function ensureHistorySub(map, id, name, description = "") {
  const key = id || "default_sub_task";
  if (!map.has(key)) {
    map.set(key, {
      id: key,
      name: name || key,
      description: description || "",
      records: [],
    });
  }
  return map.get(key);
}

function averageScore(values) {
  const valid = values.filter((value) => typeof value === "number");
  if (!valid.length) return null;
  return Math.round(valid.reduce((sum, value) => sum + value, 0) / valid.length);
}

function historySubKey(demo, subProject) {
  return `${demo.key}-${subProject.id}`;
}

function isHistorySubExpanded(demo, subProject) {
  return expandedHistorySubs.value.has(historySubKey(demo, subProject));
}

function toggleHistorySub(demo, subProject) {
  toggleSet(expandedHistorySubs, historySubKey(demo, subProject));
}

function historyRecordKey(record) {
  return `${record.attempt.attempt_id}-${record.sub.id || record.sub.sub_task_id}`;
}

function isHistoryRecordExpanded(record) {
  return expandedHistoryRecords.value.has(historyRecordKey(record));
}

function toggleHistoryRecord(record) {
  toggleSet(expandedHistoryRecords, historyRecordKey(record));
}

function durationText(seconds) {
  if (!seconds) return "-";
  if (seconds < 60) return `${seconds}s`;
  return `${Math.floor(seconds / 60)}m ${seconds % 60}s`;
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
        <button class="ghost-btn" @click="showChangePassword=true">修改密码</button>
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
            <p>按 Demo 查看所有子项目，展开子项目后可逐次查看评价、分数和步骤错误。</p>
          </div>
        </div>

        <div class="demo-history-grid">
          <article v-for="demo in historyDemos" :key="demo.key" class="demo-history-card">
            <header class="demo-history-head">
              <div>
                <span>{{ demo.subtitle }}</span>
                <strong>{{ demo.title }}</strong>
              </div>
              <div class="demo-history-stats">
                <strong>{{ demo.recordCount }}</strong>
                <small>次子项目记录</small>
              </div>
            </header>

            <div class="sub-project-list">
              <section v-for="subProject in demo.subProjects" :key="subProject.id" class="sub-project-card">
                <button class="sub-project-main" @click="toggleHistorySub(demo, subProject)">
                  <div>
                    <strong>{{ subProject.name }}</strong>
                    <span>{{ subProject.description || "暂无子项目说明" }}</span>
                  </div>
                  <div class="sub-project-stat">
                    <strong>{{ subProject.averageScore ?? "-" }}</strong>
                    <span>平均分 / {{ subProject.records.length }} 次</span>
                  </div>
                </button>

                <div v-if="isHistorySubExpanded(demo, subProject)" class="sub-record-list">
                  <article v-for="record in subProject.records" :key="`${record.attempt.attempt_id}-${record.sub.id || record.sub.sub_task_id}`" class="sub-record-card" :class="{ active: isHistoryRecordExpanded(record) }">
                    <button class="record-summary" @click="toggleHistoryRecord(record)">
                      <div class="record-title">
                        <span class="record-badge">训练 #{{ record.attempt.attempt_id }}</span>
                        <strong>{{ record.attempt.task_title || demo.title }}</strong>
                        <span>{{ formatTime(record.sub.finished_at || record.attempt.finished_at || record.attempt.started_at) }}</span>
                      </div>
                      <div class="record-score">
                        <small>得分</small>
                        <strong>{{ record.sub.score ?? "-" }}</strong>
                        <span>{{ durationText(record.sub.train_time) }}</span>
                      </div>
                      <span class="record-toggle">{{ isHistoryRecordExpanded(record) ? "收起详情" : "查看详情" }}</span>
                    </button>

                    <div v-if="isHistoryRecordExpanded(record)" class="record-detail">
                      <p class="record-comment">{{ record.sub.summary || "暂无学习评价" }}</p>

                      <div class="record-meta">
                        <span>错误 {{ record.sub.error_count || 0 }}</span>
                        <span>安全错误 {{ record.sub.safety_error_count || 0 }}</span>
                        <span>{{ subStatusText(record.sub.status) }}</span>
                      </div>

                      <div class="step-grid">
                        <div v-for="step in record.sub.steps" :key="`${record.attempt.attempt_id}-${record.sub.sub_task_id}-step-${step.index}`" class="step-row">
                          <strong>{{ Number(step.index) + 1 }}. {{ step.name || step.stepName || "未命名步骤" }}</strong>
                          <span>{{ step.expectedAction || step.expected_action || "未记录期望动作" }}</span>
                          <small>{{ step.completed ? "已完成" : "未完成" }} · 错误 {{ step.mistakeCount || step.mistake_count || 0 }}</small>
                        </div>
                      </div>

                      <div class="error-list compact">
                        <article v-for="(error, index) in record.sub.errors" :key="`${record.attempt.attempt_id}-${record.sub.sub_task_id}-error-${index}`" class="error-item">
                          <div>
                            <strong>{{ error.stepName || error.step_name || `错误 ${index + 1}` }}</strong>
                            <span>{{ error.reason || "未记录原因" }}</span>
                          </div>
                          <p>{{ error.consequence || "暂无后果说明" }}</p>
                          <small>{{ severityText(error.severity) }} · {{ error.time ? `${Number(error.time).toFixed(1)}s` : "-" }}</small>
                        </article>
                        <div v-if="!record.sub.errors?.length" class="muted-box">暂无错误详情。</div>
                      </div>
                    </div>
                  </article>

                  <div v-if="!subProject.records.length" class="muted-box">这个子项目还没有训练记录。</div>
                </div>
              </section>

              <div v-if="!demo.subProjects.length" class="empty-box">暂无子项目记录。</div>
            </div>
          </article>
        </div>
      </section>
    </main>

    <ChangePasswordModal v-if="showChangePassword" @close="showChangePassword=false" />
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
.demo-history-grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 16px; }
.demo-history-card { min-width: 0; border: 1px solid var(--border); border-radius: var(--radius-lg); background: #fff; overflow: hidden; }
.demo-history-head { display: flex; align-items: center; justify-content: space-between; gap: 12px; padding: 16px; border-bottom: 1px solid var(--border); background: #f8fbfb; }
.demo-history-head span { display: block; margin-bottom: 4px; color: var(--accent); font-size: 12px; font-weight: 900; }
.demo-history-head strong { display: block; color: var(--heading); font-size: 20px; }
.demo-history-stats { text-align: right; }
.demo-history-stats strong { font-size: 28px; line-height: 1; }
.demo-history-stats small { display: block; margin-top: 4px; color: var(--text-muted); white-space: nowrap; }
.sub-project-list { display: grid; gap: 10px; padding: 14px; }
.sub-project-card { border: 1px solid var(--border); border-radius: var(--radius); background: #fff; overflow: hidden; }
.sub-project-main { width: 100%; display: grid; grid-template-columns: minmax(0, 1fr) 64px; gap: 12px; align-items: center; padding: 14px; text-align: left; background: transparent; }
.sub-project-main:hover { background: #f6faf9; }
.sub-project-main strong { display: block; color: var(--heading); }
.sub-project-main span { display: block; margin-top: 4px; color: var(--text-muted); font-size: 12px; line-height: 1.5; }
.sub-project-stat { text-align: right; }
.sub-project-stat strong { display: block; color: var(--primary-strong); font-size: 26px; line-height: 1; }
.sub-project-stat span { margin-top: 3px; white-space: nowrap; }
.sub-record-list { display: grid; gap: 12px; padding: 0 12px 12px; }
.sub-record-card { display: grid; gap: 12px; padding: 12px; border: 1px solid #d7e5e4; border-radius: var(--radius); background: linear-gradient(180deg, #ffffff 0%, #f8fbfb 100%); box-shadow: 0 8px 18px rgba(16, 52, 52, 0.06); transition: border-color var(--transition), box-shadow var(--transition), transform var(--transition); }
.sub-record-card:hover { border-color: #b9d8d5; box-shadow: 0 12px 24px rgba(16, 52, 52, 0.09); transform: translateY(-1px); }
.sub-record-card.active { border-color: var(--primary); box-shadow: 0 12px 26px rgba(20, 112, 111, 0.14); }
.record-summary { width: 100%; display: grid; grid-template-columns: minmax(0, 1fr) 82px auto; gap: 14px; align-items: center; padding: 0; border: 0; background: transparent; text-align: left; }
.record-title { min-width: 0; display: grid; gap: 5px; }
.record-badge { width: fit-content; padding: 4px 8px; border-radius: var(--radius-full); color: var(--primary-strong); background: var(--primary-soft); font-size: 12px; font-weight: 900; }
.record-summary strong { display: block; color: var(--heading); font-size: 15px; }
.record-summary span { display: block; color: var(--text-muted); font-size: 12px; }
.record-score { min-width: 74px; padding: 8px 10px; border-radius: var(--radius); text-align: center; background: #eef8f6; }
.record-score small { display: block; color: var(--text-muted); font-size: 11px; font-weight: 900; }
.record-score strong { display: block; color: var(--primary-strong); font-size: 30px; line-height: 1; }
.record-toggle { justify-self: end; min-width: 74px; color: var(--accent); font-size: 12px; font-weight: 900; text-align: right; }
.record-detail { display: grid; gap: 12px; padding-top: 12px; border-top: 1px dashed #c9dedd; }
.record-comment { padding: 12px 14px; border-left: 3px solid var(--primary); border-radius: var(--radius); color: var(--text); background: #f2f7f7; line-height: 1.6; white-space: pre-line; }
.record-meta { display: flex; flex-wrap: wrap; gap: 8px; }
.record-meta span { padding: 5px 10px; border-radius: var(--radius-full); color: var(--text-muted); background: #eef3f5; font-size: 12px; font-weight: 800; }
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
.error-list.compact { gap: 8px; }
@media (max-width: 760px) {
  .student-header, .user-area { align-items: stretch; flex-direction: column; text-align: left; }
  .overview, .stat-row, .task-row, .active-attempt, .history-main, .sub-main, .error-item, .demo-history-grid, .sub-project-main { grid-template-columns: 1fr; }
  .task-title, .step-grid { grid-template-columns: 1fr; }
  .training-system-entry { align-items: stretch; flex-direction: column; }
  .task-actions { align-items: stretch; flex-direction: column; }
  .score-box, .sub-score, .demo-history-stats, .sub-project-stat { text-align: left; }
  .record-summary { grid-template-columns: 1fr; }
  .record-score { text-align: left; }
  .record-toggle { justify-self: start; text-align: left; }
}
</style>
