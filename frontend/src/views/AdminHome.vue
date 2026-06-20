<template>
  <div class="admin-shell">
    <aside class="sidebar">
      <div class="brand-area">
        <div class="brand-mark">慧</div>
        <div><strong>慧动手</strong><span>管理中心</span></div>
      </div>
      <nav class="side-menu">
        <button v-for="item in menus" :key="item.key" class="menu-button" :class="{ active: currentMenu === item.key }" @click="currentMenu = item.key">
          <span class="menu-icon">{{ item.mark }}</span>
          <span><strong>{{ item.title }}</strong><small>{{ item.desc }}</small></span>
        </button>
      </nav>
    </aside>

    <main class="content-area">
      <header class="top-bar">
        <div>
          <p class="eyebrow">Admin Console</p>
          <h1>{{ currentMenuMeta.title }}</h1>
          <span>{{ currentMenuMeta.subtitle }}</span>
        </div>
        <div class="user-area">
          <div><span>管理员</span><strong>{{ username }}</strong></div>
          <button class="ghost-btn" @click="loadDashboard">刷新</button>
          <button class="ghost-btn" @click="showChangePassword=true">修改密码</button>
          <button class="logout-btn" @click="logout">退出</button>
        </div>
      </header>

      <div v-if="dashboardError" class="inline-alert error">{{ dashboardError }}</div>

      <section class="stat-grid">
        <div class="stat-card"><span>学员总数</span><strong>{{ students.length }}</strong><small>可分配训练对象</small></div>
        <div class="stat-card accent"><span>训练项目</span><strong>{{ companyTasks.length }}</strong><small>本公司可用</small></div>
        <div class="stat-card"><span>分配记录</span><strong>{{ assignments.length }}</strong><small>累计派发任务</small></div>
        <div class="stat-card success"><span>完成率</span><strong>{{ completionRate }}%</strong><small>已完成 / 已分配</small></div>
      </section>

      <AssignTraining v-if="currentMenu === 'assign'" :students="students" :tasks="companyTasks" :assignments="assignments" @assigned="loadDashboard" />

      <section v-else-if="currentMenu === 'task'" class="workspace-panel">
        <div class="panel-header">
          <div>
            <h2>本公司训练项目</h2>
            <p>当前只开放 lead-train1 和 train2，可从总库添加到本公司。</p>
          </div>
          <button class="primary-btn" @click="openTaskLibrary">从总库添加</button>
        </div>
        <div class="table-wrap">
          <table>
            <thead><tr><th>训练项目</th><th>场景</th><th>说明</th><th>分配次数</th></tr></thead>
            <tbody>
              <tr v-for="task in companyTasks" :key="task.id">
                <td><strong>{{ task.title }}</strong></td>
                <td>{{ task.scene_name }}</td>
                <td>{{ task.description || "暂无说明" }}</td>
                <td>{{ taskAssignedCount(task.id) }}</td>
              </tr>
              <tr v-if="!companyTasks.length"><td colspan="4" class="empty-cell">暂无训练项目，请点击“从总库添加”。</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <section v-else-if="currentMenu === 'student'" class="workspace-panel">
        <div class="panel-header">
          <div><h2>学员总览</h2><p>按学员查看任务分配与完成进度。</p></div>
          <button class="primary-btn" @click="showStudentCreate=true">注册学员</button>
        </div>
        <div class="table-wrap">
          <table>
            <thead><tr><th>学员</th><th>已完成</th><th>完成率</th><th>近期任务</th></tr></thead>
            <tbody>
              <tr v-for="student in students" :key="student.id">
                <td><strong>{{ student.username }}</strong></td>
                <td>{{ studentCompleted(student.id) }} / {{ studentAssigned(student.id) }}</td>
                <td>
                  <div class="progress-cell">
                    <div class="progress-line"><span :style="{ width: `${studentRate(student.id)}%` }"></span></div>
                    <strong>{{ studentRate(student.id) }}%</strong>
                  </div>
                </td>
                <td>{{ studentLatestStatus(student.id) }}</td>
              </tr>
              <tr v-if="!students.length"><td colspan="4" class="empty-cell">暂无学员</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <section v-else-if="currentMenu === 'analytics'" class="workspace-panel">
        <div class="panel-header">
          <div>
            <h2>训练数据分析</h2>
            <p>按学员查看 Demo 学习历史，并汇总本公司训练完成情况。</p>
          </div>
        </div>

        <section class="analysis-grid">
          <div><span>训练会话</span><strong>{{ analytics.summary.attempt_count }}</strong></div>
          <div><span>完成会话</span><strong>{{ analytics.summary.completed_count }}</strong></div>
          <div><span>完成子项</span><strong>{{ analytics.summary.completed_sub_count || 0 }}</strong></div>
          <div><span>平均分</span><strong>{{ analytics.summary.average_score }}</strong></div>
          <div><span>安全错误</span><strong>{{ analytics.summary.safety_error_count }}</strong></div>
        </section>

        <section class="chart-grid">
          <article class="chart-card">
            <header>
              <strong>任务完成率</strong>
              <span>{{ completedAssignmentCount }} / {{ assignments.length }}</span>
            </header>
            <div class="big-rate">{{ completionRate }}%</div>
            <div class="progress-line"><span :style="{ width: percentWidth(completionRate) }"></span></div>
          </article>

          <article class="chart-card">
            <header>
              <strong>Demo 训练分布</strong>
              <span>按训练次数</span>
            </header>
            <div class="bar-list">
              <div v-for="row in sceneChartRows" :key="row.key" class="bar-row">
                <div>
                  <strong>{{ row.title }}</strong>
                  <span>{{ row.attemptCount }} 次 · 均分 {{ row.averageScore || "-" }}</span>
                </div>
                <div class="mini-bar"><span :style="{ width: row.width }"></span></div>
              </div>
            </div>
          </article>

          <article class="chart-card">
            <header>
              <strong>学员完成概览</strong>
              <span>{{ students.length }} 人</span>
            </header>
            <div class="bar-list">
              <div v-for="row in studentChartRows" :key="row.id" class="bar-row">
                <div>
                  <strong>{{ row.username }}</strong>
                  <span>{{ row.done }} / {{ row.total }} · {{ row.rate }}%</span>
                </div>
                <div class="mini-bar"><span :style="{ width: percentWidth(row.rate) }"></span></div>
              </div>
            </div>
          </article>
        </section>

        <div class="analysis-picker-panel">
          <div>
            <strong>学员学习历史</strong>
            <span>选择本公司学员后查看 Demo 与子项目训练记录</span>
          </div>
          <label class="analysis-select">
            <span>选择学员</span>
            <select v-model="selectedAnalyticsStudentId">
              <option value="" disabled>请选择学员</option>
              <option v-for="student in students" :key="student.id" :value="String(student.id)">{{ student.username }}</option>
            </select>
          </label>
        </div>

        <div v-if="selectedAnalyticsStudent" class="selected-student-title">
          <strong>{{ selectedAnalyticsStudent.username }}</strong>
          <span>学习历史</span>
        </div>

        <div v-if="selectedAnalyticsStudent" class="demo-history-grid">
          <article v-for="demo in selectedStudentHistoryDemos" :key="demo.key" class="demo-history-card">
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
                <button class="sub-project-main" @click="toggleAnalysisSub(demo, subProject)">
                  <div>
                    <strong>{{ subProject.name }}</strong>
                    <span>{{ subProject.description || "暂无子项目说明" }}</span>
                  </div>
                  <div class="sub-project-stat">
                    <strong>{{ subProject.averageScore ?? "-" }}</strong>
                    <span>平均分 / {{ subProject.records.length }} 次</span>
                  </div>
                </button>

                <div v-if="isAnalysisSubExpanded(demo, subProject)" class="sub-record-list">
                  <article v-for="record in subProject.records" :key="analysisRecordKey(record)" class="sub-record-card" :class="{ active: isAnalysisRecordExpanded(record) }">
                    <button class="record-summary" @click="toggleAnalysisRecord(record)">
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
                      <span class="record-toggle">{{ isAnalysisRecordExpanded(record) ? "收起详情" : "查看详情" }}</span>
                    </button>

                    <div v-if="isAnalysisRecordExpanded(record)" class="record-detail">
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
            </div>
          </article>
        </div>

        <div v-else class="empty-cell">
          暂无可查看的学员
        </div>
      </section>
    </main>

    <div v-if="showTaskLibrary" class="dialog-mask" @click.self="showTaskLibrary=false">
      <div class="dialog wide">
        <h3>从项目总库添加</h3>
        <p class="dialog-desc">勾选需要添加到本公司的训练项目，已添加项目不会重复显示。</p>
        <div class="dialog-table-wrap">
          <table>
            <thead><tr><th>选择</th><th>训练名称</th><th>场景</th><th>说明</th></tr></thead>
            <tbody>
              <tr v-for="task in availableGlobalTasks" :key="task.id" @click="toggleGlobalTask(task.id)" :class="{ selected: selectedGlobalTasks.includes(task.id) }">
                <td><input type="checkbox" :checked="selectedGlobalTasks.includes(task.id)" @click.stop="toggleGlobalTask(task.id)" /></td>
                <td><strong>{{ task.title }}</strong></td>
                <td>{{ task.scene_name }}</td>
                <td>{{ task.description || "暂无" }}</td>
              </tr>
              <tr v-if="!availableGlobalTasks.length"><td colspan="4" class="empty-cell">总库中暂无更多可用项目</td></tr>
            </tbody>
          </table>
        </div>
        <div class="dialog-actions">
          <span class="pick-count">已选 {{ selectedGlobalTasks.length }} 项</span>
          <div>
            <button class="cancel-btn" @click="showTaskLibrary=false">取消</button>
            <button class="confirm-btn" :disabled="!selectedGlobalTasks.length || addingTasks" @click="addSelectedTasks">
              {{ addingTasks ? "添加中..." : "确认添加" }}
            </button>
          </div>
        </div>
      </div>
    </div>

    <div v-if="showStudentCreate" class="dialog-mask" @click.self="showStudentCreate=false">
      <div class="dialog">
        <h3>注册学员</h3>
        <p class="dialog-desc">新学员会自动归属到当前管理员所在公司。</p>
        <label class="form-item">
          <span>学员账号</span>
          <input v-model.trim="newStudent.username" type="text" placeholder="请输入学员账号" />
        </label>
        <label class="form-item">
          <span>初始密码</span>
          <input v-model="newStudent.password" type="password" placeholder="请输入初始密码" />
        </label>
        <div class="dialog-actions">
          <button class="cancel-btn" @click="showStudentCreate=false">取消</button>
          <button class="confirm-btn" :disabled="creatingStudent" @click="createStudent">
            {{ creatingStudent ? "注册中..." : "确认注册" }}
          </button>
        </div>
      </div>
    </div>

    <ChangePasswordModal v-if="showChangePassword" @close="showChangePassword=false" />
  </div>
</template>

<script setup>
import { computed, onMounted, reactive, ref } from "vue";
import { useRouter } from "vue-router";
import { getUsers, getAssignments, getCompanyTrainingAnalytics } from "../api/task";
import api from "../api/http";
import AssignTraining from "../components/admin/AssignTraining.vue";
import ChangePasswordModal from "../components/ChangePasswordModal.vue";

const router = useRouter();
const students = ref([]);
const companyTasks = ref([]);
const assignments = ref([]);
const analytics = ref({
  summary: { attempt_count: 0, completed_count: 0, completed_sub_count: 0, average_score: 0, error_count: 0, safety_error_count: 0 },
  by_scene: [],
  attempts: []
});
const dashboardError = ref("");
const username = localStorage.getItem("username") || "管理员";
const adminCompanyId = Number(localStorage.getItem("company_id"));
const selectedAnalyticsStudentId = ref("");
const expandedAnalysisSubs = ref(new Set());
const expandedAnalysisRecords = ref(new Set());
const showStudentCreate = ref(false);
const creatingStudent = ref(false);
const newStudent = reactive({ username: "", password: "" });
const showChangePassword = ref(false);

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

const menus = [
  { key: "assign", title: "训练分配", desc: "学员训练派发", mark: "分" },
  { key: "task", title: "训练项目", desc: "本公司可用训练", mark: "训" },
  { key: "student", title: "学员总览", desc: "状态与进度", mark: "学" },
  { key: "analytics", title: "训练分析", desc: "成绩与错误", mark: "析" }
];
const currentMenu = ref("assign");
const currentMenuMeta = computed(() => menus.find(m => m.key === currentMenu.value) || menus[0]);

const completedAssignmentCount = computed(() =>
  assignments.value.filter(a => a.status === "done" || a.status === "已完成").length
);
const completionRate = computed(() => {
  if (!assignments.value.length) return 0;
  return Math.round((completedAssignmentCount.value / assignments.value.length) * 100);
});

const showTaskLibrary = ref(false);
const globalTasks = ref([]);
const selectedGlobalTasks = ref([]);
const addingTasks = ref(false);

const availableGlobalTasks = computed(() => {
  const existingIds = companyTasks.value.map(t => t.id);
  return globalTasks.value.filter(t => !existingIds.includes(t.id));
});

const selectedAnalyticsStudent = computed(() =>
  students.value.find(student => String(student.id) === String(selectedAnalyticsStudentId.value))
);

const selectedStudentAttempts = computed(() =>
  (analytics.value.attempts || []).filter(item => String(item.student_id) === String(selectedAnalyticsStudentId.value))
);

const selectedStudentHistoryDemos = computed(() =>
  DEMO_CONFIGS.map(demo => buildDemoHistory(demo, selectedStudentAttempts.value))
);

const sceneChartRows = computed(() => {
  const rows = DEMO_CONFIGS.map((demo) => {
    const attempts = (analytics.value.attempts || []).filter(item => item.scene_name === demo.sceneName);
    const scores = attempts.flatMap(validSubRecordsFromAttempt).map(sub => Number(sub.score || 0));
    return {
      ...demo,
      attemptCount: attempts.length,
      averageScore: averageScore(scores) || 0,
    };
  });
  const max = Math.max(1, ...rows.map(row => row.attemptCount));
  return rows.map(row => ({
    ...row,
    width: percentWidth(Math.round((row.attemptCount / max) * 100)),
  }));
});

const studentChartRows = computed(() =>
  students.value.map(student => {
    const total = studentAssigned(student.id);
    const done = studentCompleted(student.id);
    return {
      id: student.id,
      username: student.username,
      total,
      done,
      rate: total ? Math.round((done / total) * 100) : 0,
    };
  })
);

function studentAssigned(sid) { return assignments.value.filter(a => a.user_id === sid).length; }
function studentCompleted(sid) { return assignments.value.filter(a => a.user_id === sid && (a.status === "done" || a.status === "已完成")).length; }
function studentRate(sid) { const t = studentAssigned(sid); return t ? Math.round((studentCompleted(sid) / t) * 100) : 0; }
function studentLatestStatus(sid) { const list = assignments.value.filter(a => a.user_id === sid).slice(0, 2); return list.map(a => a.task_title).join("、") || "暂无"; }
function taskAssignedCount(tid) { return assignments.value.filter(a => a.task_id === tid).length; }
function formatTime(value) { return value ? new Date(value).toLocaleString() : "-"; }
function severityText(severity) { return severity === "safety" ? "安全" : severity === "warning" ? "警告" : "普通"; }
function subStatusText(status) { return status === "done" ? "已完成" : "未完成"; }
function durationText(seconds) {
  if (!seconds) return "-";
  if (seconds < 60) return `${seconds}s`;
  return `${Math.floor(seconds / 60)}m ${seconds % 60}s`;
}
function percentWidth(value) { return `${Math.max(0, Math.min(100, value || 0))}%`; }
function toggleSet(target, key) {
  const next = new Set(target.value);
  if (next.has(key)) next.delete(key);
  else next.add(key);
  target.value = next;
}

function buildDemoHistory(demo, attemptsSource) {
  const attempts = attemptsSource.filter((item) => item.scene_name === demo.sceneName);
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
    averageScore: averageScore(item.records.map((record) => Number(record.sub.score || 0))),
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
  const valid = values.filter((value) => Number.isFinite(value));
  if (!valid.length) return null;
  return Math.round(valid.reduce((sum, value) => sum + value, 0) / valid.length);
}

function validSubRecordsFromAttempt(attempt) {
  return (attempt.sub_results || []).filter(sub => sub.status === "done" && Number(sub.score || 0) > 0);
}

function analysisSubKey(demo, subProject) {
  return `${demo.key}-${subProject.id}`;
}

function isAnalysisSubExpanded(demo, subProject) {
  return expandedAnalysisSubs.value.has(analysisSubKey(demo, subProject));
}

function toggleAnalysisSub(demo, subProject) {
  toggleSet(expandedAnalysisSubs, analysisSubKey(demo, subProject));
}

function analysisRecordKey(record) {
  return `${record.attempt.attempt_id}-${record.sub.id || record.sub.sub_task_id}`;
}

function isAnalysisRecordExpanded(record) {
  return expandedAnalysisRecords.value.has(analysisRecordKey(record));
}

function toggleAnalysisRecord(record) {
  toggleSet(expandedAnalysisRecords, analysisRecordKey(record));
}

function toggleGlobalTask(taskId) {
  const idx = selectedGlobalTasks.value.indexOf(taskId);
  if (idx >= 0) selectedGlobalTasks.value.splice(idx, 1);
  else selectedGlobalTasks.value.push(taskId);
}

async function openTaskLibrary() {
  selectedGlobalTasks.value = [];
  try {
    const r = await api.get("/task/global/list");
    globalTasks.value = r.data || [];
  } catch (e) {
    alert("加载总库失败");
  }
  showTaskLibrary.value = true;
}

async function addSelectedTasks() {
  if (!selectedGlobalTasks.value.length) return;
  addingTasks.value = true;
  let ok = 0;
  for (const tid of selectedGlobalTasks.value) {
    try {
      await api.post(`/task/company/${adminCompanyId}/add`, { task_id: tid });
      ok++;
    } catch (e) {}
  }
  addingTasks.value = false;
  showTaskLibrary.value = false;
  if (ok > 0) await loadDashboard();
}

async function loadDashboard() {
  dashboardError.value = "";
  try {
    const [usersRes, assignmentsRes, analyticsRes] = await Promise.all([
      getUsers(),
      getAssignments(),
      adminCompanyId ? getCompanyTrainingAnalytics(adminCompanyId) : Promise.resolve({ data: analytics.value })
    ]);
    const companyStudents = (usersRes.data || []).filter(u => u.role === "student" && u.company_id === adminCompanyId);
    const companyStudentIds = new Set(companyStudents.map((student) => student.id));
    students.value = companyStudents;
    assignments.value = (assignmentsRes.data || []).filter((assignment) => companyStudentIds.has(assignment.user_id));
    analytics.value = analyticsRes.data || analytics.value;
    if (!companyStudentIds.has(Number(selectedAnalyticsStudentId.value))) {
      selectedAnalyticsStudentId.value = companyStudents[0] ? String(companyStudents[0].id) : "";
    }
    if (adminCompanyId) {
      const r = await api.get(`/task/company/${adminCompanyId}`);
      companyTasks.value = r.data || [];
    }
  } catch (e) {
    dashboardError.value = "数据加载失败";
  }
}

async function createStudent() {
  if (!newStudent.username || !newStudent.password) {
    alert("请输入学员账号和密码");
    return;
  }
  creatingStudent.value = true;
  try {
    await api.post("/admin/students", {
      username: newStudent.username,
      password: newStudent.password,
    });
    newStudent.username = "";
    newStudent.password = "";
    showStudentCreate.value = false;
    await loadDashboard();
  } catch (error) {
    alert(error.response?.data?.detail || "注册失败");
  } finally {
    creatingStudent.value = false;
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

onMounted(loadDashboard);
</script>

<style scoped>
.admin-shell { min-height: 100vh; display: grid; grid-template-columns: 252px minmax(0, 1fr); background: var(--page-bg); }
.sidebar { position: sticky; top: 0; height: 100vh; padding: 22px 16px; border-right: 1px solid rgba(255,255,255,0.06); background: var(--ink); color: rgba(255,255,255,0.72); }
.brand-area { display: flex; align-items: center; gap: 12px; margin-bottom: 28px; padding: 0 8px; }
.brand-mark { width: 42px; height: 42px; display: grid; place-items: center; border-radius: var(--radius-lg); color: var(--ink); background: #dff5f1; font-size: 22px; font-weight: 900; }
.brand-area strong { display: block; color: #ffffff; font-size: 18px; }
.brand-area span { display: block; margin-top: 2px; font-size: 12px; }
.side-menu { display: grid; gap: 6px; }
.menu-button { width: 100%; display: grid; grid-template-columns: 38px 1fr; gap: 10px; align-items: center; min-height: 60px; padding: 10px; border-radius: var(--radius-lg); color: rgba(255,255,255,0.70); background: transparent; text-align: left; transition: all var(--transition); }
.menu-button:hover { color: #ffffff; background: rgba(255,255,255,0.08); }
.menu-button.active { color: #ffffff; background: rgba(255,255,255,0.10); box-shadow: inset 3px 0 0 #f2b173; }
.menu-icon { width: 38px; height: 38px; display: grid; place-items: center; border-radius: var(--radius); color: #dff5f1; background: rgba(20,112,111,0.38); font-weight: 900; }
.menu-button strong, .menu-button small { display: block; }
.menu-button small { margin-top: 2px; color: rgba(255,255,255,0.48); font-size: 12px; }
.content-area { min-width: 0; padding: 26px clamp(20px, 3vw, 34px); }
.top-bar { display: flex; align-items: flex-start; justify-content: space-between; gap: 18px; margin-bottom: 22px; }
.eyebrow { color: var(--accent); font-size: 12px; font-weight: 900; text-transform: uppercase; }
.top-bar h1 { margin: 6px 0; font-size: 30px; }
.top-bar span, .panel-header p { color: var(--text-muted); }
.user-area { display: flex; align-items: center; gap: 10px; }
.user-area div { min-width: 100px; padding-right: 8px; text-align: right; }
.user-area span { display: block; color: var(--text-muted); font-size: 12px; }
.user-area strong { display: block; }
.ghost-btn, .logout-btn { height: 38px; padding: 0 16px; border-radius: var(--radius); font-weight: 700; transition: all var(--transition); }
.ghost-btn { color: var(--primary-strong); background: var(--primary-soft); }
.logout-btn { color: var(--text); background: var(--surface); border: 1px solid var(--border); }
.inline-alert { margin-bottom: 16px; padding: 12px 14px; border-radius: var(--radius); font-weight: 700; }
.inline-alert.error { color: var(--danger); background: var(--danger-soft); border: 1px solid #ffd1cc; }
.stat-grid { display: grid; grid-template-columns: repeat(4, minmax(0, 1fr)); gap: 14px; margin-bottom: 20px; }
.stat-card { min-height: 118px; padding: 18px; border: 1px solid var(--border); border-left: 4px solid var(--primary); border-radius: var(--radius-lg); background: var(--surface); box-shadow: var(--shadow-sm); }
.stat-card.accent { border-left-color: var(--accent); }
.stat-card.success { border-left-color: var(--success); }
.stat-card span, .stat-card small { display: block; color: var(--text-muted); }
.stat-card strong { display: block; margin: 10px 0 4px; color: var(--heading); font-size: 34px; line-height: 1; }
.workspace-panel { padding: 20px; border: 1px solid var(--border); border-radius: var(--radius-lg); background: var(--surface); box-shadow: var(--shadow-sm); }
.panel-header { display: flex; align-items: flex-start; justify-content: space-between; gap: 16px; margin-bottom: 16px; }
.panel-header h2 { margin-bottom: 6px; font-size: 22px; }
.primary-btn { height: 38px; padding: 0 18px; border-radius: var(--radius); color: #fff; background: var(--primary); font-weight: 700; }
.table-wrap { width: 100%; overflow: auto; border: 1px solid var(--border); border-radius: var(--radius-lg); }
th, td { padding: 13px 14px; border-bottom: 1px solid var(--border); text-align: left; vertical-align: middle; }
th { color: var(--text-muted); background: var(--surface-soft); font-size: 13px; font-weight: 900; }
tbody tr:hover { background: #f8fbfb; }
tr:last-child td { border-bottom: 0; }
.empty-cell { min-height: 82px; padding: 18px; color: var(--text-muted); text-align: center; }
.progress-cell { display: grid; grid-template-columns: minmax(120px, 1fr) 44px; gap: 10px; align-items: center; max-width: 260px; }
.progress-line { height: 8px; border-radius: 999px; overflow: hidden; background: #e0e8ec; }
.progress-line span { display: block; height: 100%; border-radius: inherit; background: linear-gradient(90deg, var(--primary), var(--accent)); transition: width 0.4s ease; }
.analysis-grid { display: grid; grid-template-columns: repeat(5, minmax(0, 1fr)); gap: 12px; margin-bottom: 16px; }
.analysis-grid > div { padding: 14px; border: 1px solid var(--border); border-radius: var(--radius); background: #f8fbfc; }
.analysis-grid span { display: block; color: var(--text-muted); font-size: 12px; }
.analysis-grid strong { display: block; margin-top: 6px; color: var(--heading); font-size: 26px; }
.analysis-picker-panel { display: flex; align-items: center; justify-content: space-between; gap: 16px; margin-bottom: 16px; padding: 14px 16px; border: 1px solid #d7e5e4; border-radius: var(--radius); background: #fbfdfd; }
.analysis-picker-panel strong { display: block; color: var(--heading); font-size: 16px; }
.analysis-picker-panel div span { display: block; margin-top: 4px; color: var(--text-muted); font-size: 12px; }
.analysis-select { min-width: 240px; display: grid; gap: 6px; }
.analysis-select span { color: var(--text-muted); font-size: 12px; font-weight: 800; }
.analysis-select select { height: 40px; padding: 0 12px; border: 1px solid var(--border); border-radius: var(--radius); background: #fff; color: var(--heading); font-weight: 800; outline: none; }
.analysis-select select:focus { border-color: var(--primary); box-shadow: 0 0 0 3px rgba(20,112,111,0.10); }
.chart-grid { display: grid; grid-template-columns: 0.9fr 1fr 1.1fr; gap: 12px; margin-bottom: 18px; }
.chart-card { min-width: 0; padding: 16px; border: 1px solid var(--border); border-radius: var(--radius); background: #fbfdfd; }
.chart-card header { display: flex; justify-content: space-between; gap: 12px; margin-bottom: 14px; }
.chart-card header strong { color: var(--heading); }
.chart-card header span, .bar-row span { color: var(--text-muted); font-size: 12px; }
.big-rate { margin-bottom: 10px; color: var(--heading); font-size: 34px; font-weight: 900; line-height: 1; }
.bar-list { display: grid; gap: 10px; }
.bar-row { display: grid; grid-template-columns: minmax(110px, 0.9fr) minmax(100px, 1fr); gap: 12px; align-items: center; }
.bar-row strong { display: block; color: var(--heading); font-size: 13px; }
.mini-bar { height: 8px; overflow: hidden; border-radius: 999px; background: #e0e8ec; }
.mini-bar span { display: block; height: 100%; border-radius: inherit; background: linear-gradient(90deg, var(--primary), var(--accent)); }
.selected-student-title { display: flex; align-items: baseline; gap: 8px; margin: 4px 0 12px; }
.selected-student-title strong { color: var(--heading); font-size: 18px; }
.selected-student-title span { color: var(--text-muted); font-weight: 800; }
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
.error-list.compact { gap: 8px; }
.analysis-list, .sub-list, .error-list { display: grid; gap: 10px; }
.attempt-card { overflow: hidden; border: 1px solid var(--border); border-radius: var(--radius-lg); background: #fff; }
.attempt-main, .sub-main { width: 100%; display: grid; grid-template-columns: minmax(0, 1fr) 130px; gap: 12px; align-items: center; padding: 16px; text-align: left; background: transparent; }
.attempt-main:hover, .sub-main:hover { background: #f8fbfb; }
.attempt-main strong, .sub-main strong { display: block; color: var(--heading); }
.attempt-main span, .sub-main span, .attempt-score span, .sub-score span { display: block; margin-top: 4px; color: var(--text-muted); font-size: 12px; }
.attempt-score, .sub-score { text-align: right; }
.attempt-score strong, .sub-score strong { display: block; color: var(--heading); font-size: 28px; }
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
.muted-box { padding: 14px; border-radius: var(--radius); color: var(--text-muted); background: #f4f7f8; font-weight: 700; }
.dialog-mask { position: fixed; inset: 0; background: rgba(0,0,0,0.40); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.dialog { background: white; border-radius: var(--radius-xl); padding: 28px; box-shadow: 0 20px 40px rgba(0,0,0,0.15); }
.dialog.wide { width: 680px; max-height: 80vh; display: flex; flex-direction: column; }
.dialog h3 { margin: 0 0 8px; }
.dialog-desc { color: var(--text-muted); font-size: 13px; margin-bottom: 16px; }
.form-item { display: grid; gap: 7px; margin-bottom: 14px; }
.form-item span { color: var(--heading); font-size: 13px; font-weight: 800; }
.form-item input { height: 42px; border: 1px solid var(--border); border-radius: var(--radius); padding: 0 12px; font-size: 14px; outline: none; }
.form-item input:focus { border-color: var(--primary); box-shadow: 0 0 0 3px rgba(20,112,111,0.10); }
.dialog-table-wrap { overflow: auto; flex: 1; border: 1px solid var(--border); border-radius: var(--radius); margin-bottom: 16px; max-height: 360px; }
.dialog-table-wrap table { width: 100%; }
.dialog-table-wrap tbody tr { cursor: pointer; transition: background var(--transition-fast); }
.dialog-table-wrap tbody tr.selected { background: var(--primary-soft); }
.dialog-table-wrap input[type="checkbox"] { width: 18px; height: 18px; accent-color: var(--primary); cursor: pointer; }
.dialog-actions { display: flex; align-items: center; justify-content: space-between; gap: 12px; margin-top: 4px; }
.dialog-actions > div { display: flex; gap: 10px; }
.pick-count { color: var(--primary-strong); font-weight: 700; font-size: 13px; }
.cancel-btn, .confirm-btn { height: 38px; padding: 0 20px; border: none; border-radius: var(--radius); font-weight: 700; cursor: pointer; transition: all var(--transition); }
.cancel-btn { background: #e0e4e8; color: #555; }
.confirm-btn { background: var(--primary); color: white; }
.confirm-btn:disabled { opacity: 0.45; cursor: not-allowed; }
@media (max-width: 1180px) {
  .admin-shell { grid-template-columns: 1fr; }
  .sidebar { position: static; height: auto; }
  .side-menu { grid-template-columns: repeat(2, minmax(0, 1fr)); }
  .stat-grid, .analysis-grid, .chart-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
@media (max-width: 760px) {
  .content-area { padding: 18px; }
  .top-bar, .user-area, .panel-header, .analysis-picker-panel { align-items: stretch; flex-direction: column; }
  .user-area div { text-align: left; }
  .side-menu, .stat-grid, .analysis-grid, .chart-grid, .demo-history-grid, .sub-project-main, .bar-row, .attempt-main, .sub-main, .error-item, .step-grid { grid-template-columns: 1fr; }
  .dialog.wide { width: 95vw; }
  .analysis-select { min-width: 0; }
  .attempt-score, .sub-score, .demo-history-stats, .sub-project-stat { text-align: left; }
  .record-summary { grid-template-columns: 1fr; }
  .record-score { text-align: left; }
  .record-toggle { justify-self: start; text-align: left; }
}
</style>
