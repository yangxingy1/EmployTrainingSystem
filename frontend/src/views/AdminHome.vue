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

      <!-- 训练项目 -->
      <section v-else-if="currentMenu === 'task'" class="workspace-panel">
        <div class="panel-header">
          <div>
            <h2>本公司训练项目</h2>
            <p>以下为当前可用的训练项目，可随时从总库添加。</p>
          </div>
          <button class="primary-btn" @click="openTaskLibrary">从总库添加</button>
        </div>
        <div class="table-wrap">
          <table>
            <thead><tr><th>训练项目</th><th>说明</th><th>分配次数</th></tr></thead>
            <tbody>
              <tr v-for="task in companyTasks" :key="task.id">
                <td><strong>{{ task.title }}</strong></td>
                <td>{{ task.description || "暂无说明" }}</td>
                <td>{{ taskAssignedCount(task.id) }}</td>
              </tr>
              <tr v-if="!companyTasks.length"><td colspan="3" class="empty-cell">暂无训练项目，请点击「从总库添加」</td></tr>
            </tbody>
          </table>
        </div>
      </section>

      <!-- 学员总览 -->
      <section v-else-if="currentMenu === 'student'" class="workspace-panel">
        <div class="panel-header"><div><h2>学员概览</h2><p>通过分配功能将训练项目分发给学员，实时掌握完成进度。</p></div></div>
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
    </main>

    <!-- 从总库添加训练项目弹窗 -->
    <div v-if="showTaskLibrary" class="dialog-mask" @click.self="showTaskLibrary=false">
      <div class="dialog wide">
        <h3>从项目总库添加</h3>
        <p class="dialog-desc">勾选需要添加到本公司的训练项目（已添加的不会显示）</p>
        <div class="dialog-table-wrap">
          <table>
            <thead><tr><th>选择</th><th>训练名称</th><th>说明</th></tr></thead>
            <tbody>
              <tr v-for="task in availableGlobalTasks" :key="task.id" @click="toggleGlobalTask(task.id)" :class="{ selected: selectedGlobalTasks.includes(task.id) }">
                <td><input type="checkbox" :checked="selectedGlobalTasks.includes(task.id)" @click.stop="toggleGlobalTask(task.id)" /></td>
                <td><strong>{{ task.title }}</strong></td>
                <td>{{ task.description || "暂无" }}</td>
              </tr>
              <tr v-if="!availableGlobalTasks.length"><td colspan="3" class="empty-cell">总库中暂无更多可用项目</td></tr>
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
import { getUsers, getAssignments } from "../api/task";
import axios from "axios";
import AssignTraining from "../components/admin/AssignTraining.vue";

const router = useRouter();
const students = ref([]);
const companyTasks = ref([]);
const assignments = ref([]);
const dashboardError = ref("");
const username = localStorage.getItem("username") || "管理员";
const adminCompanyId = Number(localStorage.getItem("company_id"));

const menus = [
  { key: "assign", title: "训练分配", desc: "学员训练派发", mark: "分" },
  { key: "task", title: "训练项目", desc: "本公司可用训练", mark: "训" },
  { key: "student", title: "学员总览", desc: "状态与进度", mark: "学" }
];
const currentMenu = ref("assign");
const currentMenuMeta = computed(() => menus.find(m => m.key === currentMenu.value) || menus[0]);

const completionRate = computed(() => {
  if (!assignments.value.length) return 0;
  const done = assignments.value.filter(a => a.status === "done" || a.status === "已完成").length;
  return Math.round((done / assignments.value.length) * 100);
});

function studentAssigned(sid) { return assignments.value.filter(a => a.user_id === sid).length; }
function studentCompleted(sid) { return assignments.value.filter(a => a.user_id === sid && (a.status === "done" || a.status === "已完成")).length; }
function studentRate(sid) { const t = studentAssigned(sid); return t ? Math.round((studentCompleted(sid) / t) * 100) : 0; }
function studentLatestStatus(sid) { const list = assignments.value.filter(a => a.user_id === sid).slice(0, 2); return list.map(a => a.task_title).join("、") || "暂无"; }
function taskAssignedCount(tid) { return assignments.value.filter(a => a.task_id === tid).length; }

// ---- 从总库添加 ----
const showTaskLibrary = ref(false);
const globalTasks = ref([]);
const selectedGlobalTasks = ref([]);
const addingTasks = ref(false);

// 过滤掉已添加的，只显示可添加的
const availableGlobalTasks = computed(() => {
  const existingIds = companyTasks.value.map(t => t.id);
  return globalTasks.value.filter(t => !existingIds.includes(t.id));
});

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
  } catch (e) { alert("加载总库失败"); }
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
    } catch (e) { /* skip */ }
  }
  addingTasks.value = false;
  showTaskLibrary.value = false;
  if (ok > 0) await loadDashboard();
}

// ---- 数据加载 ----
async function loadDashboard() {
  dashboardError.value = "";
  try {
    const [usersRes, assignmentsRes] = await Promise.all([getUsers(), getAssignments()]);
    students.value = (usersRes.data || []).filter(u => u.role === "student" && u.company_id === adminCompanyId);
    assignments.value = assignmentsRes.data || [];
    if (adminCompanyId) {
      const r = await axios.get(`http://127.0.0.1:8000/task/company/${adminCompanyId}`);
      companyTasks.value = r.data || [];
    }
  } catch (e) { dashboardError.value = "数据加载失败"; }
}

function logout() {
  localStorage.removeItem("token"); localStorage.removeItem("username"); localStorage.removeItem("role"); localStorage.removeItem("user_id"); localStorage.removeItem("company_id");
  router.replace("/login");
}

onMounted(() => { loadDashboard(); });
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
.menu-button strong { display: block; }
.menu-button small { display: block; margin-top: 2px; color: rgba(255,255,255,0.48); font-size: 12px; }
.content-area { min-width: 0; padding: 26px clamp(20px, 3vw, 34px); }
.top-bar { display: flex; align-items: flex-start; justify-content: space-between; gap: 18px; margin-bottom: 22px; }
.eyebrow { color: var(--accent); font-size: 12px; font-weight: 900; text-transform: uppercase; }
.top-bar h1 { margin: 6px 0; font-size: 30px; }
.top-bar span { color: var(--text-muted); }
.user-area { display: flex; align-items: center; gap: 10px; }
.user-area div { min-width: 100px; padding-right: 8px; text-align: right; }
.user-area span { display: block; color: var(--text-muted); font-size: 12px; }
.user-area strong { display: block; }
.ghost-btn, .logout-btn { height: 38px; padding: 0 16px; border-radius: var(--radius); font-weight: 700; transition: all var(--transition); }
.ghost-btn { color: var(--primary-strong); background: var(--primary-soft); }
.ghost-btn:hover { color: #ffffff; background: var(--primary); }
.logout-btn { color: var(--text); background: var(--surface); border: 1px solid var(--border); }
.logout-btn:hover { border-color: var(--danger); color: var(--danger); }
.ghost-btn:hover, .logout-btn:hover { transform: translateY(-1px); }
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
.panel-header p { color: var(--text-muted); }
.primary-btn { height: 38px; padding: 0 18px; border-radius: var(--radius); color: #ffffff; background: var(--primary); font-weight: 700; transition: all var(--transition); white-space: nowrap; }
.primary-btn:hover { background: var(--primary-strong); box-shadow: 0 6px 18px rgba(20,112,111,0.24); transform: translateY(-1px); }
.table-wrap { width: 100%; overflow: auto; border: 1px solid var(--border); border-radius: var(--radius-lg); }
th, td { padding: 13px 14px; border-bottom: 1px solid var(--border); text-align: left; vertical-align: middle; }
th { color: var(--text-muted); background: var(--surface-soft); font-size: 13px; font-weight: 900; }
tbody tr:hover { background: #f8fbfb; }
tr:last-child td { border-bottom: 0; }
.empty-cell { height: 92px; color: var(--text-muted); text-align: center; }
.progress-cell { display: grid; grid-template-columns: minmax(120px, 1fr) 44px; gap: 10px; align-items: center; max-width: 260px; }
.progress-line { height: 8px; border-radius: 999px; overflow: hidden; background: #e0e8ec; }
.progress-line span { display: block; height: 100%; border-radius: inherit; background: linear-gradient(90deg, var(--primary), var(--accent)); transition: width 0.4s ease; }

/* ---- 弹窗 ---- */
.dialog-mask { position: fixed; inset: 0; background: rgba(0,0,0,0.40); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.dialog { background: white; border-radius: var(--radius-xl); padding: 28px; box-shadow: 0 20px 40px rgba(0,0,0,0.15); }
.dialog.wide { width: 600px; max-height: 80vh; display: flex; flex-direction: column; }
.dialog h3 { margin: 0 0 8px; }
.dialog-desc { color: var(--text-muted); font-size: 13px; margin-bottom: 16px; }
.dialog-table-wrap { overflow: auto; flex: 1; border: 1px solid var(--border); border-radius: var(--radius); margin-bottom: 16px; max-height: 360px; }
.dialog-table-wrap table { width: 100%; }
.dialog-table-wrap th { position: sticky; top: 0; z-index: 1; }
.dialog-table-wrap tbody tr { cursor: pointer; transition: background var(--transition-fast); }
.dialog-table-wrap tbody tr:hover { background: #f4f9f8; }
.dialog-table-wrap tbody tr.selected { background: var(--primary-soft); }
.dialog-table-wrap input[type="checkbox"] { width: 18px; height: 18px; accent-color: var(--primary); cursor: pointer; }
.dialog-actions { display: flex; align-items: center; justify-content: space-between; gap: 12px; margin-top: 4px; }
.dialog-actions > div { display: flex; gap: 10px; }
.pick-count { color: var(--primary-strong); font-weight: 700; font-size: 13px; }
.cancel-btn, .confirm-btn { height: 38px; padding: 0 20px; border: none; border-radius: var(--radius); font-weight: 700; cursor: pointer; transition: all var(--transition); }
.cancel-btn { background: #e0e4e8; color: #555; }
.cancel-btn:hover { background: #d0d5da; }
.confirm-btn { background: var(--primary); color: white; }
.confirm-btn:hover:not(:disabled) { background: var(--primary-strong); }
.confirm-btn:disabled { opacity: 0.45; cursor: not-allowed; }

@media (max-width: 1180px) { .admin-shell { grid-template-columns: 1fr; } .sidebar { position: static; height: auto; } .side-menu { grid-template-columns: repeat(3, minmax(0, 1fr)); } .stat-grid { grid-template-columns: repeat(2, minmax(0, 1fr)); } }
@media (max-width: 760px) { .content-area { padding: 18px; } .top-bar, .user-area { align-items: stretch; flex-direction: column; } .user-area div { text-align: left; } .side-menu, .stat-grid { grid-template-columns: 1fr; } .dialog.wide { width: 95vw; } }
</style>
