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
        <div class="panel-header"><div><h2>学员总览</h2><p>按学员查看任务分配与完成进度。</p></div></div>
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
            <p>按训练会话展开子项目成绩与步骤错误，后续可接入 AI 助手生成分析。</p>
          </div>
        </div>

        <section class="analysis-grid">
          <div><span>训练会话</span><strong>{{ analytics.summary.attempt_count }}</strong></div>
          <div><span>完成会话</span><strong>{{ analytics.summary.completed_count }}</strong></div>
          <div><span>完成子项</span><strong>{{ analytics.summary.completed_sub_count || 0 }}</strong></div>
          <div><span>平均分</span><strong>{{ analytics.summary.average_score }}</strong></div>
          <div><span>安全错误</span><strong>{{ analytics.summary.safety_error_count }}</strong></div>
        </section>

        <div class="analysis-list">
          <article v-for="item in analytics.attempts" :key="item.attempt_id" class="attempt-card">
            <button class="attempt-main" @click="toggleAttempt(item)">
              <div>
                <strong>{{ item.username || item.student_id }} · {{ item.task_title || item.scene_name }}</strong>
                <span>#{{ item.attempt_id }} · {{ item.scene_name }} · {{ item.completed_sub_count || 0 }}/{{ item.total_sub_count || item.sub_results?.length || 0 }} 子项目完成</span>
              </div>
              <div class="attempt-score">
                <strong>{{ item.score ?? "-" }}</strong>
                <span>{{ formatTime(item.finished_at || item.started_at) }}</span>
              </div>
            </button>

            <div v-if="isAttemptExpanded(item)" class="sub-list">
              <section v-for="sub in item.sub_results" :key="sub.sub_task_id" class="sub-card" :class="{ pending: sub.status !== 'done' }">
                <button class="sub-main" @click="toggleSub(item, sub)">
                  <div>
                    <strong>{{ sub.sub_task_name }}</strong>
                    <span>{{ sub.status === "done" ? "已完成" : "未完成" }} · 错误 {{ sub.error_count || 0 }} / 安全 {{ sub.safety_error_count || 0 }}</span>
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
                    <div v-for="step in sub.steps" :key="`${item.attempt_id}-${sub.sub_task_id}-step-${step.index}`" class="step-row">
                      <strong>{{ Number(step.index) + 1 }}. {{ step.name || step.stepName || "未命名步骤" }}</strong>
                      <span>{{ step.expectedAction || step.expected_action || "未记录期望动作" }}</span>
                      <small>{{ step.completed ? "已完成" : "未完成" }} · 错误 {{ step.mistakeCount || step.mistake_count || 0 }}</small>
                    </div>
                  </div>

                  <div class="error-list">
                    <article v-for="(error, index) in sub.errors" :key="`${item.attempt_id}-${sub.sub_task_id}-error-${index}`" class="error-item">
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

          <div v-if="!analytics.attempts.length" class="empty-cell">暂无训练数据</div>
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
  </div>
</template>

<script setup>
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { getUsers, getAssignments, getCompanyTrainingAnalytics } from "../api/task";
import axios from "axios";
import AssignTraining from "../components/admin/AssignTraining.vue";

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
const expandedAttempts = ref(new Set());
const expandedSubResults = ref(new Set());

const menus = [
  { key: "assign", title: "训练分配", desc: "学员训练派发", mark: "分" },
  { key: "task", title: "训练项目", desc: "本公司可用训练", mark: "训" },
  { key: "student", title: "学员总览", desc: "状态与进度", mark: "学" },
  { key: "analytics", title: "训练分析", desc: "成绩与错误", mark: "析" }
];
const currentMenu = ref("assign");
const currentMenuMeta = computed(() => menus.find(m => m.key === currentMenu.value) || menus[0]);

const completionRate = computed(() => {
  if (!assignments.value.length) return 0;
  const done = assignments.value.filter(a => a.status === "done" || a.status === "已完成").length;
  return Math.round((done / assignments.value.length) * 100);
});

const showTaskLibrary = ref(false);
const globalTasks = ref([]);
const selectedGlobalTasks = ref([]);
const addingTasks = ref(false);

const availableGlobalTasks = computed(() => {
  const existingIds = companyTasks.value.map(t => t.id);
  return globalTasks.value.filter(t => !existingIds.includes(t.id));
});

function studentAssigned(sid) { return assignments.value.filter(a => a.user_id === sid).length; }
function studentCompleted(sid) { return assignments.value.filter(a => a.user_id === sid && (a.status === "done" || a.status === "已完成")).length; }
function studentRate(sid) { const t = studentAssigned(sid); return t ? Math.round((studentCompleted(sid) / t) * 100) : 0; }
function studentLatestStatus(sid) { const list = assignments.value.filter(a => a.user_id === sid).slice(0, 2); return list.map(a => a.task_title).join("、") || "暂无"; }
function taskAssignedCount(tid) { return assignments.value.filter(a => a.task_id === tid).length; }
function formatTime(value) { return value ? new Date(value).toLocaleString() : "-"; }
function severityText(severity) { return severity === "safety" ? "安全" : severity === "warning" ? "警告" : "普通"; }
function attemptKey(item) { return `attempt-${item.attempt_id}`; }
function subKey(item, sub) { return `${item.attempt_id}-${sub.sub_task_id}`; }
function isAttemptExpanded(item) { return expandedAttempts.value.has(attemptKey(item)); }
function isSubExpanded(item, sub) { return expandedSubResults.value.has(subKey(item, sub)); }
function toggleSet(target, key) {
  const next = new Set(target.value);
  if (next.has(key)) next.delete(key);
  else next.add(key);
  target.value = next;
}
function toggleAttempt(item) { toggleSet(expandedAttempts, attemptKey(item)); }
function toggleSub(item, sub) { toggleSet(expandedSubResults, subKey(item, sub)); }

function toggleGlobalTask(taskId) {
  const idx = selectedGlobalTasks.value.indexOf(taskId);
  if (idx >= 0) selectedGlobalTasks.value.splice(idx, 1);
  else selectedGlobalTasks.value.push(taskId);
}

async function openTaskLibrary() {
  selectedGlobalTasks.value = [];
  try {
    const r = await axios.get("http://127.0.0.1:8000/task/global/list");
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
      await axios.post(`http://127.0.0.1:8000/task/company/${adminCompanyId}/add`, { task_id: tid });
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
    students.value = (usersRes.data || []).filter(u => u.role === "student" && u.company_id === adminCompanyId);
    assignments.value = assignmentsRes.data || [];
    analytics.value = analyticsRes.data || analytics.value;
    if (adminCompanyId) {
      const r = await axios.get(`http://127.0.0.1:8000/task/company/${adminCompanyId}`);
      companyTasks.value = r.data || [];
    }
  } catch (e) {
    dashboardError.value = "数据加载失败";
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
  .stat-grid, .analysis-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); }
}
@media (max-width: 760px) {
  .content-area { padding: 18px; }
  .top-bar, .user-area { align-items: stretch; flex-direction: column; }
  .user-area div { text-align: left; }
  .side-menu, .stat-grid, .analysis-grid, .attempt-main, .sub-main, .error-item, .step-grid { grid-template-columns: 1fr; }
  .dialog.wide { width: 95vw; }
  .attempt-score, .sub-score { text-align: left; }
}
</style>
