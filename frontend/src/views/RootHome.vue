<template>
  <div class="root-layout">
    <aside class="sidebar">
      <div class="logo-area">
        <div class="logo-icon">R</div>
        <div>
          <h3>Root Console</h3>
          <span>平台管理中心</span>
        </div>
      </div>
      <nav class="menu">
        <button class="menu-item" :class="{ active: currentMenu==='dashboard' }" @click="currentMenu='dashboard'">控制台首�?/button>
        <button class="menu-item" :class="{ active: currentMenu==='company' }" @click="currentMenu='company'">公司管理</button>
        <button class="menu-item" :class="{ active: currentMenu==='admin' }" @click="currentMenu='admin'">管理员管�?/button>
        <button class="menu-item" :class="{ active: currentMenu==='tasklib' }" @click="currentMenu='tasklib'">项目总库</button>
      </nav>
      <button class="logout-btn" @click="logout">退出登�?/button>
    </aside>

    <main class="content">
      <!-- 首页 -->
      <div v-if="currentMenu==='dashboard'">
        <div class="header-card">
          <h1>欢迎回来，Root</h1>
          <p>当前账号：{{ username }}</p>
        </div>

        <div class="stats-grid">
          <div class="stat-card"><span>公司总数</span><strong>{{ companyCount }}</strong></div>
          <div class="stat-card"><span>管理员总数</span><strong>{{ adminCount }}</strong></div>
          <div class="stat-card"><span>学员总数</span><strong>{{ studentCount }}</strong></div>
        </div>

        <div class="welcome-card">
          <h2>系统概览</h2>
          <p>Root账号用于维护企业信息、企业管理员、训练项目总库以及系统统计数据�?/p>
          <p>当前版本已构建企业管理体系和训练项目分配体系�?/p>
        </div>
      </div>

      <!-- 公司管理 -->
      <div v-if="currentMenu==='company'">
        <div class="section-header">
          <h2>公司管理</h2>
          <button class="add-btn" @click="showCreate=true">新建公司</button>
        </div>
        <table class="data-table">
          <thead><tr><th>ID</th><th>公司名称</th><th>公司编码</th><th>状�?/th><th>操作</th></tr></thead>
          <tbody>
            <tr v-for="item in companies" :key="item.id">
              <td>{{ item.id }}</td><td>{{ item.name }}</td><td>{{ item.code }}</td>
              <td><span :class="['status-badge', item.status]">{{ item.status === 'active' ? '启用' : '停用' }}</span></td>
              <td>
                <button class="action-btn" @click="changeStatus(item)">{{ item.status === 'active' ? '停用' : '启用' }}</button>
                <button class="danger-btn" @click="deleteCompany(item)">删除</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- 管理员管�?-->
      <div v-if="currentMenu==='admin'">
        <div class="section-header">
          <h2>管理员管�?/h2>
          <button class="add-btn" @click="showAdminCreate=true">新建管理�?/button>
        </div>
        <table class="data-table">
          <thead><tr><th>ID</th><th>管理�?/th><th>所属公�?/th><th>操作</th></tr></thead>
          <tbody>
            <tr v-for="item in admins" :key="item.id">
              <td>{{ item.id }}</td><td>{{ item.username }}</td><td>{{ item.company_name }}</td>
              <td><button class="danger-btn" @click="deleteAdmin(item)">删除</button></td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- 项目总库 -->
      <div v-if="currentMenu==='tasklib'">
        <div class="section-header">
          <h2>项目总库</h2>
          <button class="add-btn" @click="showTaskCreate=true">新建训练项目</button>
        </div>

        <!-- 训练项目表格 -->
        <table class="data-table">
          <thead><tr><th>ID</th><th>训练名称</th><th>Unity 场景</th><th>说明</th><th>已分配公�?/th><th>操作</th></tr></thead>
          <tbody>
            <tr v-for="task in allTasks" :key="task.id">
              <td>{{ task.id }}</td>
              <td><strong>{{ task.title }}</strong></td>
              <td>{{ task.scene_name || '-' }}</td>
              <td>{{ task.description || '暂无' }}</td>
              <td>{{ getTaskCompanies(task.id) }}</td>
              <td>
                <button class="action-btn" @click="openEditTask(task)">编辑</button>
                <button class="action-btn assign" @click="openAssignCompany(task)">分配公司</button>
                <button class="danger-btn" @click="deleteTaskItem(task)">删除</button>
              </td>
            </tr>
            <tr v-if="!allTasks.length"><td colspan="6" class="empty-cell">暂无训练项目</td></tr>
          </tbody>
        </table>

        <!-- 公司-训练项目关联�?-->
        <div class="section-header" style="margin-top:28px">
          <h2>公司训练项目分配</h2>
        </div>
        <table class="data-table">
          <thead><tr><th>ID</th><th>公司</th><th>训练项目</th><th>操作</th></tr></thead>
          <tbody>
            <tr v-for="ct in companyTasks" :key="ct.id">
              <td>{{ ct.id }}</td><td>{{ ct.company_name }}</td><td>{{ ct.task_title }}</td>
              <td><button class="danger-btn" @click="removeCompanyTask(ct)">取消分配</button></td>
            </tr>
            <tr v-if="!companyTasks.length"><td colspan="4" class="empty-cell">暂无分配记录</td></tr>
          </tbody>
        </table>
      </div>
    </main>

    <!-- 新建公司弹窗 -->
    <div v-if="showCreate" class="dialog-mask" @click.self="showCreate=false">
      <div class="dialog">
        <h3>新建公司</h3>
        <div class="form-item"><span>公司名称</span><input v-model="newCompany.name" placeholder="例如：河北第一工厂" /></div>
        <div class="form-item"><span>公司编码</span><input v-model="newCompany.code" placeholder="例如：hb001" /></div>
        <div class="dialog-actions">
          <button class="cancel-btn" @click="showCreate=false">取消</button>
          <button class="confirm-btn" @click="createCompany">确认创建</button>
        </div>
      </div>
    </div>

    <!-- 新建管理员弹�?-->
    <div v-if="showAdminCreate" class="dialog-mask" @click.self="showAdminCreate=false">
      <div class="dialog">
        <h3>新建管理�?/h3>
        <div class="form-item"><span>管理员账�?/span><input v-model="newAdmin.username" placeholder="请输入账�? /></div>
        <div class="form-item"><span>管理员密�?/span><input type="password" v-model="newAdmin.password" placeholder="请输入密�? /></div>
        <div class="form-item"><span>所属公�?/span><select v-model="newAdmin.company_id"><option :value="null" disabled>请选择公司</option><option v-for="c in companies" :key="c.id" :value="c.id">{{ c.name }}</option></select></div>
        <div class="dialog-actions">
          <button class="cancel-btn" @click="showAdminCreate=false">取消</button>
          <button class="confirm-btn" @click="createAdmin">确认创建</button>
        </div>
      </div>
    </div>

    <!-- 新建/编辑训练项目弹窗 -->
    <div v-if="showTaskCreate || editingTask" class="dialog-mask" @click.self="closeTaskDialog">
      <div class="dialog">
        <h3>{{ editingTask ? '编辑训练项目' : '新建训练项目' }}</h3>
        <div class="form-item"><span>训练名称</span><input v-model="taskForm.title" placeholder="例如：旋转阀门操作训�? /></div>
        <div class="form-item">
          <span>Unity 场景</span>
          <select v-model="taskForm.scene_name">
            <option value="lead-train1">lead-train1</option>
            <option value="train2">train2</option>
          </select>
        </div>
        <div class="form-item"><span>训练说明</span><textarea v-model="taskForm.description" rows="3" placeholder="填写训练目标、标准动作等" /></div>
        <div class="dialog-actions">
          <button class="cancel-btn" @click="closeTaskDialog">取消</button>
          <button class="confirm-btn" @click="saveTask">{{ editingTask ? '保存修改' : '创建项目' }}</button>
        </div>
      </div>
    </div>

    <!-- 分配公司弹窗 -->
    <div v-if="assigningTask" class="dialog-mask" @click.self="assigningTask=null">
      <div class="dialog">
        <h3>分配训练项目: {{ assigningTask.title }}</h3>
        <div class="form-item"><span>选择公司</span><select v-model="assignCompanyId"><option :value="null" disabled>请选择公司</option><option v-for="c in companies" :key="c.id" :value="c.id">{{ c.name }}</option></select></div>
        <div class="dialog-actions">
          <button class="cancel-btn" @click="assigningTask=null">取消</button>
          <button class="confirm-btn" @click="doAssignCompany">确认分配</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
// Root 控制�?—�?平台级管�? 公司、管理员、训练项目总库
import { onMounted, ref, reactive } from "vue";
import { useRouter } from "vue-router";
import axios from "axios";
import { BACKEND_API } from "../api/config";

const router = useRouter();
const username = ref(localStorage.getItem("username") || "Root");
const currentMenu = ref("dashboard");

// ---- 公司 ----
const companies = ref([]);
const showCreate = ref(false);
const newCompany = ref({ name: "", code: "" });

// ---- 管理�?----
const admins = ref([]);
const showAdminCreate = ref(false);
const newAdmin = ref({ username: "", password: "", company_id: null });

// ---- 统计数据 ----
const companyCount = ref(0);
const adminCount = ref(0);
const studentCount = ref(0);

// ---- 项目总库 ----
const allTasks = ref([]);
const companyTasks = ref([]);
const showTaskCreate = ref(false);
const editingTask = ref(null);
const assigningTask = ref(null);
const assignCompanyId = ref(null);
const taskForm = reactive({ title: "", description: "", scene_name: "lead-train1" });

// ============ 数据加载 ============
async function loadCompanies() {
  try { const r = await axios.get(`${BACKEND_API}/root/companies"); companies.value = r.data || []; } catch (e) {}
}
async function loadAdmins() {
  try { const r = await axios.get(`${BACKEND_API}/root/admins"); admins.value = r.data || []; } catch (e) {}
}
async function loadStatistics() {
  try {
    const r = await axios.get(`${BACKEND_API}/root/statistics");
    companyCount.value = r.data.companies || 0;
    adminCount.value = r.data.admins || 0;
    studentCount.value = r.data.students || 0;
  } catch (e) {}
}
async function loadAllTasks() {
  try { const r = await axios.get(`${BACKEND_API}/task/list"); allTasks.value = r.data || []; } catch (e) {}
}
async function loadCompanyTasks() {
  try { const r = await axios.get(`${BACKEND_API}/root/company-tasks"); companyTasks.value = r.data || []; } catch (e) {}
}

// ============ 公司操作 ============
async function createCompany() {
  try { await axios.post(`${BACKEND_API}/root/companies", newCompany.value); showCreate.value = false; newCompany.value = { name: "", code: "" }; await loadCompanies(); await loadStatistics(); } catch (e) { alert(e.response?.data?.detail || "创建失败"); }
}
async function changeStatus(item) {
  try { await axios.patch(`${BACKEND_API}/root/companies/${item.id}`, { status: item.status === "active" ? "inactive" : "active" }); await loadCompanies(); } catch (e) { alert(e.response?.data?.detail || "操作失败"); }
}
async function deleteCompany(item) {
  if (!confirm(`确定删除公司"${item.name}"？`)) return;
  try { await axios.delete(`${BACKEND_API}/root/companies/${item.id}`); await loadCompanies(); await loadAdmins(); await loadStatistics(); await loadCompanyTasks(); } catch (e) { alert(e.response?.data?.detail || "删除失败"); }
}

// ============ 管理员操�?============
async function createAdmin() {
  try { await axios.post(`${BACKEND_API}/root/admins", newAdmin.value); showAdminCreate.value = false; newAdmin.value = { username: "", password: "", company_id: null }; await loadAdmins(); await loadStatistics(); } catch (e) { alert(e.response?.data?.detail || "创建失败"); }
}
async function deleteAdmin(admin) {
  if (!confirm(`确定删除管理�?${admin.username}"吗？`)) return;
  try { await axios.delete(`${BACKEND_API}/root/admins/${admin.id}`); await loadAdmins(); await loadStatistics(); } catch (e) { alert(e.response?.data?.detail || "删除失败"); }
}

// ============ 训练项目操作 ============
function openEditTask(task) {
  editingTask.value = task;
  taskForm.title = task.title;
  taskForm.description = task.description || "";
  taskForm.scene_name = task.scene_name || "lead-train1";
}
function closeTaskDialog() {
  showTaskCreate.value = false;
  editingTask.value = null;
  taskForm.title = "";
  taskForm.description = "";
  taskForm.scene_name = "lead-train1";
}
async function saveTask() {
  if (!taskForm.title.trim()) return alert("训练名称不能为空");
  try {
    if (editingTask.value) {
      await axios.put(`${BACKEND_API}/task/${editingTask.value.id}`, { title: taskForm.title, description: taskForm.description, scene_name: taskForm.scene_name });
    } else {
      await axios.post(`${BACKEND_API}/task/create", { title: taskForm.title, description: taskForm.description, scene_name: taskForm.scene_name });
    }
    closeTaskDialog();
    await loadAllTasks();
  } catch (e) { alert(e.response?.data?.detail || "操作失败"); }
}
async function deleteTaskItem(task) {
  if (!confirm(`确定删除训练项目"${task.title}"？`)) return;
  try { await axios.delete(`${BACKEND_API}/task/${task.id}`); await loadAllTasks(); await loadCompanyTasks(); } catch (e) { alert(e.response?.data?.detail || "删除失败"); }
}

// ============ 公司-训练项目关联 ============
function getTaskCompanies(taskId) {
  const names = companyTasks.value.filter(ct => ct.task_id === taskId).map(ct => ct.company_name);
  return names.length ? names.join("�?) : "未分�?;
}
function openAssignCompany(task) {
  assigningTask.value = task;
  assignCompanyId.value = null;
}
async function doAssignCompany() {
  if (!assignCompanyId.value) return alert("请选择公司");
  try {
    await axios.post(`${BACKEND_API}/root/company-tasks", { company_id: assignCompanyId.value, task_id: assigningTask.value.id });
    assigningTask.value = null;
    await loadCompanyTasks();
  } catch (e) { alert(e.response?.data?.detail || "分配失败"); }
}
async function removeCompanyTask(ct) {
  if (!confirm("确定取消此分配？")) return;
  try { await axios.delete(`${BACKEND_API}/root/company-tasks/${ct.id}`); await loadCompanyTasks(); } catch (e) { alert(e.response?.data?.detail || "操作失败"); }
}

// ============ 退�?============
function logout() {
  localStorage.removeItem("token"); localStorage.removeItem("username"); localStorage.removeItem("role"); localStorage.removeItem("user_id");
  router.replace("/rootlogin");
}

// ============ 初始�?============
onMounted(async () => {
  await Promise.all([loadCompanies(), loadAdmins(), loadStatistics(), loadAllTasks(), loadCompanyTasks()]);
});
</script>

<style scoped>
.root-layout { display: flex; min-height: 100vh; background: #f6f8fb; }
.sidebar { width: 252px; background: #162238; color: white; display: flex; flex-direction: column; padding: 24px; }
.logo-area { display: flex; align-items: center; gap: 12px; margin-bottom: 36px; }
.logo-icon { width: 46px; height: 46px; border-radius: var(--radius-lg); background: #f0c674; color: #111; display: grid; place-items: center; font-weight: 900; }
.logo-area h3 { margin: 0; color: #ffffff; }
.logo-area span { font-size: 13px; opacity: .7; }
.menu { display: flex; flex-direction: column; gap: 4px; flex: 1; }
.menu-item { height: 44px; border: none; border-radius: var(--radius-lg); background: transparent; color: rgba(255,255,255,0.68); text-align: left; padding-left: 14px; cursor: pointer; font-size: 14px; transition: all var(--transition); }
.menu-item:hover { background: rgba(255,255,255,0.08); color: #fff; }
.menu-item.active { background: #2c4f8f; color: #fff; font-weight: 700; }
.logout-btn { margin-top: auto; height: 44px; border: none; border-radius: var(--radius-lg); background: #c44b4b; color: white; font-weight: 700; cursor: pointer; transition: all var(--transition); }
.logout-btn:hover { background: #a83a3a; }

.content { flex: 1; min-width: 0; padding: 32px; }
.header-card { padding: 28px; border-radius: var(--radius-lg); background: white; box-shadow: 0 8px 20px rgba(0,0,0,0.05); margin-bottom: 24px; }
.header-card h1 { margin: 0 0 8px; }
.header-card p { margin: 0; color: #666; }

.stats-grid { display: grid; grid-template-columns: repeat(3, 1fr); gap: 20px; margin-bottom: 24px; }
.stat-card { padding: 24px; border-radius: var(--radius-lg); background: white; box-shadow: 0 8px 20px rgba(0,0,0,0.05); transition: box-shadow var(--transition); }
.stat-card:hover { box-shadow: 0 12px 28px rgba(0,0,0,0.08); }
.stat-card span { color: #777; font-size: 14px; }
.stat-card strong { display: block; margin-top: 10px; font-size: 34px; color: #23395d; }

.welcome-card { padding: 28px; border-radius: var(--radius-lg); background: white; box-shadow: 0 8px 20px rgba(0,0,0,0.05); }
.welcome-card h2 { margin-top: 0; }
.welcome-card p { line-height: 1.8; color: #555; }

.section-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 18px; }
.section-header h2 { font-size: 20px; }

.add-btn { padding: 10px 18px; border: none; border-radius: var(--radius); background: #2c4f8f; color: white; font-weight: 700; cursor: pointer; transition: all var(--transition); }
.add-btn:hover { background: #234078; box-shadow: 0 4px 12px rgba(44,79,143,0.28); }

.data-table { width: 100%; border-collapse: collapse; background: white; border-radius: var(--radius-lg); overflow: hidden; box-shadow: 0 4px 12px rgba(0,0,0,0.04); }
.data-table th, .data-table td { padding: 13px 16px; border-bottom: 1px solid #eee; text-align: left; font-size: 14px; }
.data-table th { background: #f8fafb; font-weight: 700; color: #555; font-size: 13px; }
.data-table tbody tr:hover { background: #f8fbfc; }
.data-table tbody tr:last-child td { border-bottom: 0; }

.action-btn, .danger-btn { padding: 5px 14px; border: none; border-radius: var(--radius); font-weight: 700; font-size: 12px; cursor: pointer; transition: all var(--transition); margin-right: 6px; }
.action-btn { background: #2c4f8f; color: white; }
.action-btn:hover { background: #234078; }
.action-btn.assign { background: #14706f; }
.action-btn.assign:hover { background: #0d5655; }
.danger-btn { background: #d9534f; color: white; }
.danger-btn:hover { background: #c44b4b; }

.status-badge { display: inline-block; padding: 3px 10px; border-radius: var(--radius-full); font-size: 12px; font-weight: 700; }
.status-badge.active { color: #238457; background: #e4f5ed; }
.status-badge.inactive { color: #ba3c3c; background: #fff0ef; }

.empty-cell { height: 80px; color: #999; text-align: center; vertical-align: middle; }

.dialog-mask { position: fixed; inset: 0; background: rgba(0,0,0,0.40); display: flex; align-items: center; justify-content: center; z-index: 1000; }
.dialog { width: 440px; background: white; border-radius: var(--radius-xl); padding: 28px; box-shadow: 0 20px 40px rgba(0,0,0,0.15); }
.dialog h3 { margin-top: 0; margin-bottom: 20px; }
.form-item { display: grid; gap: 6px; margin-bottom: 14px; }
.form-item span { font-size: 13px; font-weight: 700; color: var(--heading); }
.form-item input, .form-item select, .form-item textarea { height: 44px; border: 1px solid #ddd; border-radius: var(--radius); padding: 0 12px; font-size: 14px; outline: none; width: 100%; transition: border-color var(--transition); }
.form-item textarea { height: auto; min-height: 80px; padding: 10px 12px; resize: vertical; }
.form-item input:focus, .form-item select:focus, .form-item textarea:focus { border-color: #2c4f8f; }
.dialog-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 20px; }
.cancel-btn, .confirm-btn { padding: 10px 20px; border: none; border-radius: var(--radius); font-weight: 700; cursor: pointer; transition: all var(--transition); }
.cancel-btn { background: #e0e4e8; color: #555; }
.cancel-btn:hover { background: #d0d5da; }
.confirm-btn { background: #2c4f8f; color: white; }
.confirm-btn:hover { background: #234078; }

@media (max-width: 900px) { .root-layout { flex-direction: column; } .sidebar { width: 100%; padding: 16px; } .menu { flex-direction: row; flex-wrap: wrap; } .content { padding: 18px; } .stats-grid { grid-template-columns: 1fr; } }
</style>
