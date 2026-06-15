<template>
  <div class="admin-shell">
    <header class="top-bar">
      <div class="brand-area">
        <div class="brand-mark">慧</div>
        <div>
          <h1>慧动手管理中心</h1>
          <span>基于手势识别的工厂员工手部作业虚拟仿真培训平台</span>
        </div>
      </div>

      <div class="user-area">
        <span class="role-text">管理员</span>
        <strong>{{ username }}</strong>
        <button class="logout-btn" @click="logout">退出登录</button>
      </div>
    </header>

    <div class="main-area">
      <aside class="side-menu">
        <button
          v-for="item in menus"
          :key="item.key"
          class="menu-button"
          :class="{ active: currentMenu === item.key }"
          @click="currentMenu = item.key"
        >
          <span class="menu-icon">{{ item.icon }}</span>
          <span>
            <strong>{{ item.title }}</strong>
            <small>{{ item.desc }}</small>
          </span>
        </button>
      </aside>

      <main class="content-area">
        <section class="page-heading">
          <div>
            <p class="eyebrow">后台管理</p>
            <h2>{{ currentMenuMeta.title }}</h2>
            <span>{{ currentMenuMeta.subtitle }}</span>
          </div>

          <button class="refresh-btn" @click="loadDashboard">刷新数据</button>
        </section>

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
            <small>已建训练内容</small>
          </div>

          <div class="stat-card">
            <span>分配记录</span>
            <strong>{{ assignments.length }}</strong>
            <small>累计训练派发</small>
          </div>

          <div class="stat-card success">
            <span>完成率</span>
            <strong>{{ completionRate }}%</strong>
            <small>已完成 / 已分配</small>
          </div>
        </section>

        <AssignTraining v-if="currentMenu === 'assign'" :students="students" :tasks="tasks" :assignments="assignments" @assigned="loadDashboard" />

        <section v-else-if="currentMenu === 'task'" class="workspace-panel">
          <div class="panel-header">
            <div>
              <h3>创建训练项目</h3>
              <p>维护训练名称、训练目标和操作说明，创建后可直接进入分配流程。</p>
            </div>
          </div>

          <div class="task-editor">
            <label>
              训练名称
              <input
                v-model.trim="taskForm.title"
                type="text"
                placeholder="例如：装配工位手势校准"
              />
            </label>

            <label>
              训练说明
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
                  <td>
                    <strong>{{ task.title }}</strong>
                  </td>
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
              <h3>学员概览</h3>
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
                  <td>
                    <strong>{{ student.username }}</strong>
                  </td>
                  <td>{{ studentCompleted(student.id) }} / {{ studentAssigned(student.id) }}</td>
                  <td>
                    <div class="progress-line">
                      <span :style="{ width: `${studentRate(student.id)}%` }"></span>
                    </div>
                    {{ studentRate(student.id) }}%
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
  </div>
</template>

<script setup>
import AssignTraining from "../components/admin/AssignTraining.vue";
import { computed, onMounted, ref } from "vue";
import { useRouter } from "vue-router";
import { getTasks, getUsers, getAssignments } from "../api/task";

const router = useRouter();
const username = localStorage.getItem("username") || "管理员";
const currentMenu = ref("assign");  // 当前激活的菜单页签

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
  { key: "assign", icon: "📋", title: "训练分配", desc: "批量为学员派发训练任务" },
  { key: "task", icon: "📚", title: "训练维护", desc: "新建/管理训练项目库" },
  { key: "student", icon: "👥", title: "学员概览", desc: "查看学员进度与统计" }
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

// 计算全局任务完成率
const completionRate = computed(() => {
  const assigned = assignments.value.length;
  const done = assignments.value.filter((a) => a.status === "done").length;
  return assigned ? Math.round((done / assigned) * 100) : 0;
});

// 加载所有数据：任务、学员、分配记录
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

// 创建新训练项目
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
    const detail = error.response?.data?.detail;
    taskMessage.value = detail || "创建失败，请重试";
    taskMessageType.value = "error";
  } finally {
    creatingTask.value = false;
  }
}

// 退出登录：清空本地存储并跳转
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
  display: flex;
  flex-direction: column;
}

.top-bar {
  min-height: 80px;
  padding: 18px 34px;
  color: white;
  background: linear-gradient(135deg, #1a3a4a, #2f4f6f);
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
}

.brand-area {
  display: flex;
  align-items: center;
  gap: 16px;
}

.brand-mark {
  width: 52px;
  height: 52px;
  border-radius: var(--radius);
  display: grid;
  place-items: center;
  color: white;
  background: linear-gradient(135deg, #3e8e91, #2a6b6f);
  font-weight: 800;
  font-size: 24px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.18);
}

.eyebrow {
  color: #ffd9bf;
  font-size: 12px;
  font-weight: 800;
  letter-spacing: 0;
}

.top-bar h1 {
  color: white;
  font-size: 22px;
  margin: 3px 0 6px;
}

.top-bar span { color: #b8d4f0;
  font-size: 22px;
}

.user-area {
  display: flex;
  align-items: center;
  gap: 14px;
}

.role-text {
  color: #ffd9bf;
  font-size: 17px;
  font-weight: 800;
}

.logout-btn,
.refresh-btn {
  height: 38px;
  padding: 0 18px;
  border-radius: var(--radius-sm);
  font-weight: 700;
  cursor: pointer;
  transition: all var(--transition);
}

.logout-btn {
  color: white;
  background: rgba(255, 255, 255, 0.12);
  border: 1px solid rgba(255, 255, 255, 0.18);
}

.logout-btn:hover {
  background: rgba(255, 255, 255, 0.22);
}

.refresh-btn {
  min-width: 100px;
  color: var(--primary);
  background: #eaf4f3;
}

.refresh-btn:hover {
  background: #d6ece9;
}

.primary-btn {
  min-width: 120px;
  height: 42px;
  padding: 0 20px;
  border-radius: var(--radius-sm);
  color: #e8f5f3;
  background: #1a5c60;
  font-weight: 800;
  cursor: pointer;
  box-shadow: 0 6px 16px rgba(26, 92, 96, 0.28);
  transition: all var(--transition);
}

.primary-btn:hover {
  background: #14484b;
  box-shadow: 0 10px 24px rgba(26, 92, 96, 0.38);
}

.primary-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.main-area {
  flex: 1;
  display: flex;
}

.side-menu {
  width: 260px;
  padding: 24px 14px;
  background: var(--surface);
  border-right: 1px solid var(--border);
}

.menu-button {
  width: 100%;
  display: grid;
  grid-template-columns: 44px 1fr;
  gap: 12px;
  align-items: center;
  min-height: 68px;
  padding: 12px 14px;
  margin-bottom: 8px;
  border: 1px solid transparent;
  border-radius: var(--radius);
  color: var(--text);
  background: transparent;
  text-align: left;
  cursor: pointer;
  transition: all var(--transition);
}

.menu-button:hover {
  border-color: var(--border);
  background: var(--surface-soft);
  transform: translateX(2px);
}

.menu-button.active {
  border-color: var(--primary);
  background: #eef8f7;
}

.menu-icon {
  width: 44px;
  height: 44px;
  border-radius: var(--radius-sm);
  display: grid;
  place-items: center;
  color: white;
  background: var(--primary);
  font-weight: 800;
  font-size: 18px;
}

.menu-button small {
  display: block;
  margin-top: 2px;
  color: var(--text-muted);
  font-size: 12px;
}

.content-area {
  flex: 1;
  padding: 28px;
  overflow: auto;
}

.page-heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 18px;
  margin-bottom: 20px;
}

.page-heading h2 {
  font-size: 30px;
  margin: 4px 0 6px;
}

.page-heading span {
  color: var(--text-muted);
}

.inline-alert {
  margin-bottom: 16px;
  padding: 12px 14px;
  border-radius: var(--radius-sm);
  font-weight: 700;
}

.inline-alert.error {
  color: var(--danger);
  background: #fff1ef;
  border: 1px solid #ffd5ce;
}

.stat-grid {
  display: grid;
  grid-template-columns: repeat(4, minmax(0, 1fr));
  gap: 16px;
  margin-bottom: 22px;
}

.stat-card {
  min-height: 128px;
  padding: 22px;
  border-radius: var(--radius);
  background: var(--surface);
  border: 1px solid var(--border);
  box-shadow: var(--shadow-sm);
  transition: all var(--transition);
}

.stat-card:hover {
  box-shadow: var(--shadow);
  transform: translateY(-2px);
}

.stat-card span,
.stat-card small {
  display: block;
  color: var(--text-muted);
}

.stat-card strong {
  display: block;
  margin: 12px 0 4px;
  color: var(--heading);
  font-size: 36px;
  line-height: 1;
}

.stat-card.accent {
  border-top: 4px solid var(--accent);
}

.stat-card.success {
  border-top: 4px solid var(--success);
}

.workspace-panel {
  padding: 24px;
  border-radius: var(--radius);
  background: var(--surface);
  border: 1px solid var(--border);
  box-shadow: var(--shadow-sm);
}

.panel-header {
  display: flex;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 18px;
}

.panel-header h3 {
  font-size: 22px;
  margin-bottom: 6px;
}

.panel-header p {
  color: var(--text-muted);
}

.task-editor {
  display: grid;
  grid-template-columns: 1fr 1.5fr;
  gap: 16px;
  padding: 20px;
  margin-bottom: 18px;
  border-radius: var(--radius);
  background: var(--surface-soft);
  border: 1px solid var(--border);
  max-width: 100%;
  overflow: hidden;
}

.task-editor label:first-child {
  max-width: 320px;
}

.task-editor label {
  display: flex;
  flex-direction: column;
  gap: 8px;
  color: var(--heading);
  font-weight: 700;
}

.task-editor input,
.task-editor textarea {
  width: 100%;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  padding: 11px 12px;
  color: var(--text);
  background: white;
  outline: none;
  transition: all var(--transition);
}

.task-editor textarea {
  resize: none; height: 120px;
}

.task-editor input:focus,
.task-editor textarea:focus {
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(47, 111, 115, 0.14);
}

.editor-actions {
  grid-column: 1 / -1;
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
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
  border-radius: var(--radius-sm);
}

table {
  width: 100%;
  border-collapse: collapse;
  background: white;
}

th,
td {
  padding: 14px 16px;
  border-bottom: 1px solid var(--border);
  text-align: left;
  vertical-align: middle;
}

th {
  color: var(--text-muted);
  background: #f6f8fb;
  font-size: 13px;
  font-weight: 800;
}

tbody tr {
  transition: background var(--transition);
}

tbody tr:hover {
  background: #f8fbfd;
}

tr:last-child td {
  border-bottom: 0;
}

.empty-cell {
  height: 92px;
  color: var(--text-muted);
  text-align: center;
}

.progress-line {
  width: min(180px, 100%);
  height: 8px;
  margin-bottom: 6px;
  border-radius: var(--radius-full);
  overflow: hidden;
  background: #e6ebf1;
}

.progress-line span {
  display: block;
  height: 100%;
  border-radius: inherit;
  background: linear-gradient(90deg, var(--primary), var(--accent));
  transition: width 0.6s ease;
}

.status-pill {
  display: inline-flex;
  align-items: center;
  min-height: 28px;
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

@media (max-width: 1100px) {
  .top-bar,
  .main-area,
  .page-heading {
    flex-direction: column;
  }

  .side-menu {
    width: 100%;
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 10px;
  }

  .menu-button {
    margin-bottom: 0;
  }

  .stat-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (max-width: 720px) {
  .top-bar,
  .content-area {
    padding: 18px;
  }

  .brand-area,
  .user-area {
    width: 100%;
    flex-wrap: wrap;
  }

  .side-menu,
  .stat-grid,
  .task-editor { grid-template-columns: 1fr; } .task-editor label:first-child { max-width: 100%; }

  .page-heading h2 {
    font-size: 24px;
  }
}
</style>
