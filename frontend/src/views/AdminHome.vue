<template>
  <div class="admin-shell">
    <aside class="sidebar">
      <div class="brand-area">
        <div class="brand-mark">慧</div>
        <div>
          <strong>慧动手</strong>
          <span>管理中心</span>
        </div>
      </div>

      <nav class="side-menu" aria-label="后台导航">
        <button
          v-for="item in menus"
          :key="item.key"
          class="menu-button"
          :class="{ active: currentMenu === item.key }"
          @click="currentMenu = item.key"
        >
          <span class="menu-icon">{{ item.mark }}</span>
          <span>
            <strong>{{ item.title }}</strong>
            <small>{{ item.desc }}</small>
          </span>
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
          <div>
            <span>管理员</span>
            <strong>{{ username }}</strong>
          </div>
          <button class="ghost-btn" @click="loadDashboard">刷新</button>
          <button class="logout-btn" @click="logout">退出</button>
        </div>
      </header>

      <div v-if="dashboardError" class="inline-alert error">
        {{ dashboardError }}
      </div>

      <section class="stat-grid">
        <div class="stat-card">
          <span>学员总数</span>
          <strong>{{ students.length }}</strong>
          <small>可分配训练对象</small>
        </div>
        <div class="stat-card accent">
          <span>训练项目</span>
          <strong>{{ tasks.length }}</strong>
          <small>可派发内容</small>
        </div>
        <div class="stat-card">
          <span>分配记录</span>
          <strong>{{ assignments.length }}</strong>
          <small>累计派发任务</small>
        </div>
        <div class="stat-card success">
          <span>完成率</span>
          <strong>{{ completionRate }}%</strong>
          <small>已完成 / 已分配</small>
        </div>
      </section>

      <AssignTraining
        v-if="currentMenu === 'assign'"
        :students="students"
        :tasks="tasks"
        :assignments="assignments"
        @assigned="loadDashboard"
      />

      <section v-else-if="currentMenu === 'task'" class="workspace-panel">
        <div class="panel-header">
          <div>
            <h2>创建训练项目</h2>
            <p>维护训练名称、训练目标和操作说明，创建后可直接进入分配流程。</p>
          </div>
        </div>

        <div class="task-editor">
          <label>
            <span>训练名称</span>
            <input v-model.trim="taskForm.title" type="text" placeholder="例如：旋转阀门操作训练" />
          </label>

          <label>
            <span>训练说明</span>
            <textarea
              v-model.trim="taskForm.description"
              rows="5"
              placeholder="填写训练目标、标准动作、注意事项等"
            />
          </label>

          <div class="editor-actions">
            <span :class="['form-message', taskMessageType]">{{ taskMessage }}</span>
            <button class="primary-btn" :disabled="creatingTask" @click="createTraining">
              {{ creatingTask ? "创建中..." : "创建训练" }}
            </button>
          </div>
        </div>

        <div class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>训练项目</th>
                <th>说明</th>
                <th>分配次数</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="task in tasks" :key="task.id">
                <td><strong>{{ task.title }}</strong></td>
                <td>{{ task.description || "暂无说明" }}</td>
                <td>{{ taskAssignedCount(task.id) }}</td>
              </tr>
              <tr v-if="!tasks.length">
                <td colspan="3" class="empty-cell">暂无训练项目</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>

      <section v-else-if="currentMenu === 'student'" class="workspace-panel">
        <div class="panel-header">
          <div>
            <h2>学员概览</h2>
            <p>通过分配功能将训练项目分发给学员，实时掌握完成进度。</p>
          </div>
        </div>

        <div class="table-wrap">
          <table>
            <thead>
              <tr>
                <th>学员</th>
                <th>已完成</th>
                <th>完成率</th>
                <th>近期任务</th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="student in students" :key="student.id">
                <td><strong>{{ student.username }}</strong></td>
                <td>{{ studentCompleted(student.id) }} / {{ studentAssigned(student.id) }}</td>
                <td>
                  <div class="progress-cell">
                    <div class="progress-line">
                      <span :style="{ width: `${studentRate(student.id)}%` }"></span>
                    </div>
                    <strong>{{ studentRate(student.id) }}%</strong>
                  </div>
                </td>
                <td>{{ studentLatestStatus(student.id) }}</td>
              </tr>
              <tr v-if="!students.length">
                <td colspan="4" class="empty-cell">暂无学员</td>
              </tr>
            </tbody>
          </table>
        </div>
      </section>
    </main>
  </div>
</template>

<script setup>
import AssignTraining from "../components/admin/AssignTraining.vue";
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { getTasks, getUsers, getAssignments } from "../api/task";

const router = useRouter();
const username = localStorage.getItem("username") || "管理员";
const currentMenu = ref("assign");

const students = ref([]);
const tasks = ref([]);
const assignments = ref([]);
const dashboardError = ref("");
const creatingTask = ref(false);
const taskMessage = ref("");
const taskMessageType = ref("");

const taskForm = ref({
  title: "",
  description: ""
});

const menus = [
  { key: "assign", mark: "A", title: "训练分配", desc: "为学员派发任务" },
  { key: "task", mark: "T", title: "训练维护", desc: "管理训练项目库" },
  { key: "student", mark: "S", title: "学员概览", desc: "查看训练进度" }
];

const currentMenuMeta = computed(() => {
  const menu = menus.find((m) => m.key === currentMenu.value);
  const subtitles = {
    assign: "选择学员并分配对应的手势训练任务",
    task: "维护可分配的训练内容",
    student: "查看每位学员的完成进度"
  };
  return {
    title: menu ? menu.title : "",
    subtitle: subtitles[currentMenu.value] || ""
  };
});

const completionRate = computed(() => {
  const assigned = assignments.value.length;
  const done = assignments.value.filter((a) => a.status === "done").length;
  return assigned ? Math.round((done / assigned) * 100) : 0;
});

async function loadDashboard() {
  dashboardError.value = "";
  try {
    const [taskRes, userRes, assignmentRes] = await Promise.all([
      getTasks(),
      getUsers(),
      getAssignments()
    ]);
    tasks.value = taskRes.data.filter((t) => t && t.id);
    students.value = userRes.data.filter((u) => u.role === "student");
    assignments.value = assignmentRes.data;
  } catch (error) {
    dashboardError.value = "数据加载失败，请重试";
  }
}

function taskAssignedCount(taskId) {
  return assignments.value.filter((a) => a.task_id === taskId).length;
}

function studentAssigned(studentId) {
  return assignments.value.filter((a) => a.user_id === studentId).length;
}

function studentCompleted(studentId) {
  return assignments.value.filter((a) => a.user_id === studentId && a.status === "done").length;
}

function studentRate(studentId) {
  const total = studentAssigned(studentId);
  return total ? Math.round((studentCompleted(studentId) / total) * 100) : 0;
}

function studentLatestStatus(studentId) {
  const record = assignments.value
    .filter((a) => a.user_id === studentId)
    .sort((a, b) => b.id - a.id)[0];
  if (!record) return "暂无任务";
  const statusMap = { pending: "待开始", running: "进行中", done: "已完成" };
  return statusMap[record.status] || record.status;
}

async function createTraining() {
  if (!taskForm.value.title) {
    taskMessage.value = "请输入训练名称";
    taskMessageType.value = "error";
    return;
  }

  creatingTask.value = true;
  taskMessage.value = "";

  try {
    const { createTask } = await import("../api/task");
    await createTask({
      title: taskForm.value.title,
      description: taskForm.value.description
    });
    taskForm.value.title = "";
    taskForm.value.description = "";
    taskMessage.value = "训练项目创建成功";
    taskMessageType.value = "success";
    await loadDashboard();
  } catch (error) {
    taskMessage.value = error.response?.data?.detail || "创建失败，请重试";
    taskMessageType.value = "error";
  } finally {
    creatingTask.value = false;
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
  loadDashboard();
});
</script>

<style scoped>
.admin-shell {
  min-height: 100vh;
  display: grid;
  grid-template-columns: 264px minmax(0, 1fr);
  background: var(--page-bg);
}

.sidebar {
  position: sticky;
  top: 0;
  height: 100vh;
  padding: 22px 16px;
  border-right: 1px solid var(--border);
  background: var(--ink);
  color: rgba(255, 255, 255, 0.72);
}

.brand-area {
  display: flex;
  align-items: center;
  gap: 12px;
  margin-bottom: 28px;
  padding: 0 8px;
}

.brand-mark {
  width: 42px;
  height: 42px;
  display: grid;
  place-items: center;
  border-radius: var(--radius);
  color: var(--ink);
  background: #dff5f1;
  font-size: 22px;
  font-weight: 900;
}

.brand-area strong,
.brand-area span {
  display: block;
}

.brand-area strong {
  color: #ffffff;
  font-size: 18px;
}

.brand-area span {
  margin-top: 2px;
  font-size: 12px;
}

.side-menu {
  display: grid;
  gap: 8px;
}

.menu-button {
  width: 100%;
  display: grid;
  grid-template-columns: 38px 1fr;
  gap: 10px;
  align-items: center;
  min-height: 62px;
  padding: 10px;
  border-radius: var(--radius);
  color: rgba(255, 255, 255, 0.74);
  background: transparent;
  text-align: left;
  transition: background var(--transition), color var(--transition);
}

.menu-button:hover,
.menu-button.active {
  color: #ffffff;
  background: rgba(255, 255, 255, 0.09);
}

.menu-button.active {
  box-shadow: inset 3px 0 0 #f2b173;
}

.menu-icon {
  width: 38px;
  height: 38px;
  display: grid;
  place-items: center;
  border-radius: var(--radius-sm);
  color: #dff5f1;
  background: rgba(20, 112, 111, 0.38);
  font-weight: 900;
}

.menu-button strong,
.menu-button small {
  display: block;
}

.menu-button small {
  margin-top: 2px;
  color: rgba(255, 255, 255, 0.48);
  font-size: 12px;
}

.content-area {
  min-width: 0;
  padding: 26px clamp(20px, 3vw, 34px);
}

.top-bar {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
  margin-bottom: 22px;
}

.eyebrow {
  color: var(--accent);
  font-size: 12px;
  font-weight: 900;
  letter-spacing: 0;
  text-transform: uppercase;
}

.top-bar h1 {
  margin: 6px 0;
  font-size: 30px;
}

.top-bar span {
  color: var(--text-muted);
}

.user-area {
  display: flex;
  align-items: center;
  gap: 10px;
}

.user-area div {
  min-width: 100px;
  padding-right: 8px;
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
.primary-btn {
  height: 38px;
  padding: 0 16px;
  border-radius: var(--radius-sm);
  font-weight: 800;
  transition: background var(--transition), color var(--transition), box-shadow var(--transition), transform var(--transition);
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

.primary-btn {
  min-width: 118px;
  color: #ffffff;
  background: var(--primary);
}

.ghost-btn:hover,
.logout-btn:hover,
.primary-btn:hover:not(:disabled) {
  transform: translateY(-1px);
  box-shadow: var(--shadow-sm);
}

.primary-btn:hover:not(:disabled) {
  background: var(--primary-strong);
}

.inline-alert {
  margin-bottom: 16px;
  padding: 12px 14px;
  border-radius: var(--radius-sm);
  font-weight: 700;
}

.inline-alert.error {
  color: var(--danger);
  background: var(--danger-soft);
  border: 1px solid #ffd1cc;
}

.stat-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 14px;
  margin-bottom: 20px;
}

.stat-card {
  min-height: 118px;
  padding: 18px;
  border: 1px solid var(--border);
  border-left: 4px solid var(--primary);
  border-radius: var(--radius);
  background: var(--surface);
  box-shadow: var(--shadow-sm);
}

.stat-card.accent {
  border-left-color: var(--accent);
}

.stat-card.success {
  border-left-color: var(--success);
}

.stat-card span,
.stat-card small {
  display: block;
  color: var(--text-muted);
}

.stat-card strong {
  display: block;
  margin: 10px 0 4px;
  color: var(--heading);
  font-size: 34px;
  line-height: 1;
}

.workspace-panel {
  padding: 20px;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--surface);
  box-shadow: var(--shadow-sm);
}

.panel-header {
  margin-bottom: 16px;
}

.panel-header h2 {
  margin-bottom: 6px;
  font-size: 22px;
}

.panel-header p {
  color: var(--text-muted);
}

.task-editor {
  display: grid;
  grid-template-columns: minmax(220px, 0.8fr) minmax(320px, 1.2fr);
  gap: 14px;
  margin-bottom: 18px;
  padding: 16px;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--surface-soft);
}

.task-editor label {
  display: grid;
  gap: 8px;
  color: var(--heading);
  font-weight: 800;
}

.task-editor textarea {
  min-height: 112px;
  resize: vertical;
}

.editor-actions {
  grid-column: 1 / -1;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 14px;
}

.form-message {
  color: var(--text-muted);
  min-height: 20px;
}

.form-message.success {
  color: var(--success);
}

.form-message.error {
  color: var(--danger);
}

.table-wrap {
  width: 100%;
  overflow: auto;
  border: 1px solid var(--border);
  border-radius: var(--radius);
}

th,
td {
  padding: 13px 14px;
  border-bottom: 1px solid var(--border);
  text-align: left;
  vertical-align: middle;
}

th {
  color: var(--text-muted);
  background: var(--surface-soft);
  font-size: 13px;
  font-weight: 900;
}

tbody tr:hover {
  background: #f8fbfb;
}

tr:last-child td {
  border-bottom: 0;
}

.empty-cell {
  height: 92px;
  color: var(--text-muted);
  text-align: center;
}

.progress-cell {
  display: grid;
  grid-template-columns: minmax(120px, 1fr) 44px;
  gap: 10px;
  align-items: center;
  max-width: 260px;
}

.progress-line {
  height: 8px;
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

@media (max-width: 1180px) {
  .admin-shell {
    grid-template-columns: 1fr;
  }

  .sidebar {
    position: static;
    height: auto;
  }

  .side-menu {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }

  .stat-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 760px) {
  .content-area {
    padding: 18px;
  }

  .top-bar,
  .user-area,
  .editor-actions {
    align-items: stretch;
    flex-direction: column;
  }

  .user-area div {
    text-align: left;
  }

  .side-menu,
  .stat-grid,
  .task-editor {
    grid-template-columns: 1fr;
  }
}
</style>
