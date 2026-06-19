<template>
  <section class="assign-workbench">
    <div class="summary-strip">
      <div>
        <p class="section-kicker">训练分配</p>
        <h3>学员训练派发看板</h3>
        <span>按学员选择训练项目，系统会自动过滤已分配内容并保留记录�?/span>
      </div>

      <div class="summary-metrics">
        <div>
          <strong>{{ students.length }}</strong>
          <span>学员</span>
        </div>
        <div>
          <strong>{{ tasks.length }}</strong>
          <span>训练</span>
        </div>
        <div>
          <strong>{{ pendingCount }}</strong>
          <span>待开�?/span>
        </div>
      </div>
    </div>

    <div v-if="notice" :class="['notice', noticeType]">
      {{ notice }}
    </div>

    <div class="assign-grid">
      <section class="panel student-panel">
        <div class="panel-heading">
          <div>
            <h4>选择学员</h4>
            <p>�?{{ students.length }} 名学�?/p>
          </div>
          <input v-model.trim="searchTerm" class="search-input" type="text" placeholder="搜索学员" />
        </div>

        <div class="student-list">
          <div v-for="student in filteredStudents" :key="student.id" class="student-item" :class="{ selected: selectedStudent === student.id }">
            <button class="student-row" @click="selectStudent(student.id)">
              <span class="student-info">
                <strong>{{ student.username }}</strong>
                <small>ID: {{ student.id }}</small>
              </span>
              <span class="student-count">{{ studentAssignedCount(student.id) }} 项训�?/span>
              <span class="expand-arrow" :class="{ expanded: expandedStudent === student.id }" @click.stop="toggleExpand(student.id)">�?/span>
            </button>
            <div v-if="expandedStudent === student.id" class="student-detail">
              <p v-if="studentAssignedCount(student.id) === 0" class="detail-empty">暂无分配记录</p>
              <ul v-else class="detail-list">
                <li v-for="record in getStudentAssignments(student.id)" :key="record.id" class="detail-item">
                  <span class="detail-title">{{ record.task_title }}</span>
                  <span :class="['status-sm', record.status]">{{ statusLabel(record.status) }}</span>
                </li>
              </ul>
            </div>
          </div>
          <div v-if="!filteredStudents.length" class="empty-block">
            {{ students.length ? '未找到匹配学�? : '暂未注册学员，请先在登录页注册学员账�? }}
          </div>
        </div>
      </section>

      <section class="panel task-panel">
        <div class="panel-heading">
          <div>
            <h4>训练项目</h4>
            <p v-if="selectedStudentInfo">当前学员：{{ selectedStudentInfo.username }}</p>
            <p v-else>请先选择一名学�?/p>
          </div>
          <span class="selection-count">已�?{{ selectedTasks.length }} �?/span>
        </div>

        <div v-if="!selectedStudentInfo" class="empty-block large">
          选择左侧学员后，可以在这里勾选训练项目�?
        </div>

        <div v-else class="table-frame">
          <table>
            <thead>
              <tr>
                <th>选择</th>
                <th>训练项目</th>
                <th>说明</th>
                <th>状�?/th>
              </tr>
            </thead>
            <tbody>
              <tr v-for="task in tasks" :key="task.id" :class="{ disabled: isAssigned(task.id) }" @click="toggleTask(task.id)">
                <td>
                  <input type="checkbox" :value="task.id" v-model="selectedTasks" :disabled="isAssigned(task.id)" @click.stop />
                </td>
                <td><strong>{{ task.title }}</strong></td>
                <td>{{ task.description || "暂无说明" }}</td>
                <td>
                  <span v-if="isAssigned(task.id)" class="status-tag done">已分�?/span>
                  <span v-else class="status-tag ready">可分�?/span>
                </td>
              </tr>
              <tr v-if="!tasks.length">
                <td colspan="4" class="empty-cell">暂无训练项目</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="assign-actions">
          <span>{{ selectedStudentInfo ? `将分配给 ${selectedStudentInfo.username}` : "未选择学员" }}</span>
          <button class="assign-btn" :disabled="!canAssign" @click="assignSelectedTasks">
            {{ assigning ? "分配�?.." : "确认分配" }}
          </button>
        </div>
      </section>
    </div>

    <!-- 近期分配记录（含重新分配�?-->
    <section class="panel records-panel">
      <div class="panel-heading">
        <div>
          <h4>近期分配记录</h4>
          <p>展示最新训练派发与完成状态，已完成任务可重新分配</p>
        </div>
      </div>

      <div class="table-frame">
        <table>
          <thead>
            <tr>
              <th>学员</th>
              <th>训练项目</th>
              <th>状�?/th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="record in recentAssignments" :key="record.id">
              <td><strong>{{ record.username }}</strong></td>
              <td>{{ record.task_title }}</td>
              <td>
                <span :class="['status-tag', record.status]">{{ statusLabel(record.status) }}</span>
              </td>
              <td>
                <button v-if="record.status === 'done'" class="reassign-btn" @click="reassignTask(record.id)">重新分配</button>
                <span v-else class="no-action">-</span>
              </td>
            </tr>
            <tr v-if="!recentAssignments.length">
              <td colspan="4" class="empty-cell">暂无分配记录</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </section>
</template>

<script setup>
// 训练分配组件 —�?学员选择、任务勾选、批量分配、重新分�?
import axios from "axios";
import { ref, computed, watch } from "vue";
import { assignTask } from "../../api/task";

const emit = defineEmits(["assigned"]);

// 父组件传入的数据
const props = defineProps({
  tasks: { type: Array, default: () => [] },
  students: { type: Array, default: () => [] },
  assignments: { type: Array, default: () => [] }
});

// ---- 响应式状�?----
const searchTerm = ref("");          // 学员搜索关键�?
const selectedStudent = ref(null);   // 当前选中的学�?ID
const selectedTasks = ref([]);       // 勾选的训练任务 ID 数组
const assigning = ref(false);        // 分配进行中标�?
const notice = ref("");              // 操作提示消息
const noticeType = ref("");          // 消息类型: success / error
const expandedStudent = ref(null);   // 展开详情的学�?ID

// 搜索过滤: 按用户名模糊匹配
const filteredStudents = computed(() => {
  if (!searchTerm.value) return props.students;
  const term = searchTerm.value.toLowerCase();
  return props.students.filter((s) => s.username.toLowerCase().includes(term));
});

// 当前选中学员的完整信�?
const selectedStudentInfo = computed(() => {
  return props.students.find((s) => s.id === selectedStudent.value);
});

// 待开始分配数
const pendingCount = computed(() => {
  return props.assignments.filter((a) => a.status === "pending" || a.status === "未开�?).length;
});

// 近期分配记录（最�?20 条）
const recentAssignments = computed(() => {
  return props.assignments.slice(0, 20);
});

// 是否可以执行分配: 需选中学员 + 勾选任�?+ 非分配中
const canAssign = computed(() => {
  return selectedStudent.value && selectedTasks.value.length > 0 && !assigning.value;
});

// 统计某学员的分配数量
function studentAssignedCount(studentId) {
  return props.assignments.filter((a) => a.user_id === studentId).length;
}

// 获取某学员的所有分配记录（�?ID 倒序�?
function getStudentAssignments(studentId) {
  return props.assignments.filter((a) => a.user_id === studentId).sort((a, b) => b.id - a.id);
}

// 判断任务是否已分配给当前选中学员（防止重复分配）
function isAssigned(taskId) {
  if (!selectedStudent.value) return false;
  return props.assignments.some((a) => a.user_id === selectedStudent.value && a.task_id === taskId);
}

// 切换学员详情面板展开/收起
function toggleExpand(studentId) {
  expandedStudent.value = expandedStudent.value === studentId ? null : studentId;
}

// 切换任务勾选（已分配的任务不可勾选）
function toggleTask(taskId) {
  if (isAssigned(taskId)) return;
  const index = selectedTasks.value.indexOf(taskId);
  if (index >= 0) {
    selectedTasks.value.splice(index, 1);
  } else {
    selectedTasks.value.push(taskId);
  }
}

// 选择学员: 切换时清空已选任�?
function selectStudent(studentId) {
  selectedStudent.value = studentId;
  selectedTasks.value = [];
}

// 状态值转中文标签
function statusLabel(status) {
  const map = { pending: "待开�?, "未开�?: "待开�?, running: "进行�?, done: "已完�? };
  return map[status] || status;
}

// 重新分配: 将已完成任务重置�?pending，学员可再次训练
async function reassignTask(assignmentId) {
  if (!confirm("确定重新分配此任务？学员可再次训练�?)) return;
  try {
    await axios.post(`http://60.205.176.200:8000/task/reassign/${assignmentId}`);
    notice.value = "任务已重新分�?;
    noticeType.value = "success";
    emit("assigned");   // 通知父组件刷新数�?
  } catch (err) {
    notice.value = err.response?.data?.detail || "操作失败";
    noticeType.value = "error";
  }
}

// 批量分配: 将勾选的所有任务分配给当前学员
async function assignSelectedTasks() {
  if (!canAssign.value) return;
  assigning.value = true;
  notice.value = "";
  let successCount = 0;

  try {
    for (const taskId of selectedTasks.value) {
      try {
        await assignTask({ user_id: selectedStudent.value, task_id: taskId });
        successCount++;
      } catch (e) { /* 单条失败不影响其�?*/ }
    }
    if (successCount > 0) {
      notice.value = `成功分配 ${successCount} 项训练`;
      noticeType.value = "success";
      selectedTasks.value = [];
    }
    emit("assigned");
  } catch (e) {
    notice.value = "分配失败";
    noticeType.value = "error";
  } finally {
    assigning.value = false;
  }
}

// 切换学员时清空搜索和已选任�?
watch(selectedStudent, () => {
  searchTerm.value = "";
});
</script>

<style scoped>
/* ---- 分配工作�?---- */
.assign-workbench { display: grid; gap: 18px; }

/* ---- 摘要�?---- */
.summary-strip {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  gap: 18px; align-items: center;
  padding: 22px 24px;
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  background: var(--surface);
  box-shadow: var(--shadow-sm);
}

.section-kicker {
  color: var(--accent);
  font-size: 12px; font-weight: 900;
  text-transform: uppercase;
  margin-bottom: 6px;
}

.summary-strip h3 { margin-bottom: 6px; font-size: 22px; }
.summary-strip > div:first-child > span { color: var(--text-muted); font-size: 14px; }

.summary-metrics {
  display: grid;
  grid-template-columns: repeat(3, auto);
  gap: 24px; min-width: 280px;
}

.summary-metrics div { text-align: center; }
.summary-metrics strong { display: block; font-size: 28px; color: var(--heading); }
.summary-metrics span { display: block; margin-top: 4px; font-size: 13px; color: var(--text-muted); }

/* ---- 通知 ---- */
.notice {
  padding: 12px 16px;
  border-radius: var(--radius);
  font-size: 14px; font-weight: 700;
}

.notice.success { color: var(--success); background: #e8f6ef; border: 1px solid #c3e6d6; }
.notice.error { color: var(--danger); background: #fff0ef; border: 1px solid #ffd1cc; }

/* ---- 分配网格 ---- */
.assign-grid {
  display: grid;
  grid-template-columns: minmax(300px, 0.95fr) minmax(440px, 1.05fr);
  gap: 16px;
}

/* ---- 面板 ---- */
.panel {
  padding: 22px;
  border: 1px solid var(--border);
  border-radius: var(--radius-lg);
  background: var(--surface);
  box-shadow: var(--shadow-sm);
}

.panel-heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px; margin-bottom: 16px;
}

.panel-heading h4 { font-size: 18px; margin-bottom: 4px; }
.panel-heading p { color: var(--text-muted); font-size: 14px; }

.search-input {
  width: 192px; height: 38px;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  padding: 0 12px; font-size: 14px;
  background: var(--surface-soft);
  outline: none;
  transition: border-color var(--transition), box-shadow var(--transition);
}

.search-input:focus {
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(20,112,111,0.10);
}

/* ---- 学员列表 ---- */
.student-list {
  max-height: 440px; overflow-y: auto;
  border: 1px solid var(--border);
  border-radius: var(--radius);
}

.student-item { border-bottom: 1px solid #eef2f6; }
.student-item:last-child { border-bottom: 0; }

.student-row {
  width: 100%;
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto auto;
  gap: 12px; align-items: center;
  padding: 13px 16px;
  background: transparent; text-align: left;
  transition: background var(--transition);
}

.student-row:hover { background: #f6f9fb; }

.student-item.selected .student-row {
  background: #eaf5f3;
  border-left: 4px solid var(--primary);
  border-radius: 0 var(--radius) var(--radius) 0;
}

.student-info strong { display: block; font-size: 15px; color: var(--heading); }
.student-info small { display: block; margin-top: 2px; font-size: 12px; color: var(--text-muted); }

.student-count {
  font-size: 13px; color: var(--text-muted);
  font-weight: 700; white-space: nowrap;
  padding: 4px 10px;
  border-radius: var(--radius-full);
  background: #eef2f5;
}

.expand-arrow {
  width: 28px; height: 28px;
  display: grid; place-items: center;
  border-radius: var(--radius);
  font-size: 10px; color: var(--text-muted);
  transition: transform var(--transition), color var(--transition), background var(--transition);
}

.expand-arrow:hover { color: var(--primary); background: rgba(47,111,115,0.08); }
.expand-arrow.expanded { transform: rotate(180deg); color: var(--primary); }

.student-detail {
  border-top: 1px solid var(--border);
  padding: 12px 14px; background: #f8fbfd;
  animation: slideDown 0.2s ease;
}

@keyframes slideDown {
  from { opacity: 0; max-height: 0; }
  to { opacity: 1; max-height: 300px; }
}

.detail-empty { color: var(--text-muted); font-size: 13px; padding: 8px 0; }

.detail-list { list-style: none; margin: 0; padding: 0; display: flex; flex-direction: column; gap: 6px; }

.detail-item {
  display: flex; align-items: center; justify-content: space-between;
  gap: 10px; padding: 8px 12px;
  border-radius: var(--radius);
  background: white; border: 1px solid var(--border);
  font-size: 13px;
}

.detail-title { color: var(--text); font-weight: 600; }

.status-sm {
  display: inline-flex; align-items: center;
  min-height: 24px; padding: 2px 10px;
  border-radius: var(--radius-full);
  font-size: 12px; font-weight: 700; white-space: nowrap;
}

.status-sm.pending { color: var(--warning); background: #fff7e6; }
.status-sm.running { color: var(--primary-strong); background: #e9f4f3; }
.status-sm.done { color: var(--success); background: #e8f6ef; }

.selection-count {
  padding: 7px 14px;
  border-radius: var(--radius-full);
  color: var(--primary-strong); background: #e9f4f3;
  font-size: 13px; font-weight: 800;
}

/* ---- 表格 ---- */
.table-frame {
  width: 100%; overflow: auto;
  border: 1px solid var(--border);
  border-radius: var(--radius);
}

th, td {
  padding: 13px 14px;
  border-bottom: 1px solid var(--border);
  text-align: left; vertical-align: middle;
}

th { color: var(--text-muted); background: #f6f8fb; font-size: 13px; font-weight: 800; }
tbody tr:last-child td { border-bottom: 0; }
tbody tr:not(.disabled) { cursor: pointer; transition: background var(--transition); }
tbody tr:not(.disabled):hover { background: #f0f8f7; }
tbody tr.disabled { color: var(--text-muted); background: #fafafa; }

input[type="checkbox"] { width: 18px; height: 18px; accent-color: var(--primary); cursor: pointer; }

.status-tag {
  display: inline-flex; align-items: center;
  min-height: 28px; padding: 4px 12px;
  border-radius: var(--radius-full);
  font-size: 13px; font-weight: 800; white-space: nowrap;
}

.status-tag.ready { color: var(--primary-strong); background: #e9f4f3; }
.status-tag.pending { color: var(--warning); background: #fff7e6; }
.status-tag.running { color: var(--primary-strong); background: #e9f4f3; }
.status-tag.done { color: var(--success); background: #e8f6ef; }

/* ---- 分配操作 ---- */
.assign-actions {
  display: flex; align-items: center; justify-content: space-between;
  gap: 16px; margin-top: 16px; padding-top: 14px;
  border-top: 1px solid #eef2f5;
  color: var(--text-muted);
}

.assign-btn {
  min-width: 128px; height: 44px; padding: 0 22px;
  border-radius: var(--radius-lg);
  color: #ffffff; background: var(--primary);
  font-weight: 700; font-size: 15px;
  transition: all var(--transition);
}

.assign-btn:hover:not(:disabled) {
  background: var(--primary-strong);
  box-shadow: 0 8px 22px rgba(20,112,111,0.28);
  transform: translateY(-1px);
}

.assign-btn:disabled { cursor: not-allowed; opacity: 0.4; box-shadow: none; }

/* ---- 记录面板 ---- */
.records-panel { padding-bottom: 20px; }

.reassign-btn {
  padding: 5px 14px;
  border-radius: var(--radius);
  color: #ffffff; background: var(--accent);
  font-size: 12px; font-weight: 700;
  transition: all var(--transition);
}

.reassign-btn:hover { background: #c0682a; box-shadow: 0 4px 10px rgba(214,120,54,0.28); }

.no-action { color: var(--text-muted); font-size: 13px; }

/* ---- 空状�?---- */
.empty-block, .empty-cell { color: var(--text-muted); text-align: center; }

.empty-block {
  padding: 32px 16px;
  border: 1px dashed var(--border);
  border-radius: var(--radius-lg);
  background: #fbfcfe;
  font-size: 14px; line-height: 1.6;
}

.empty-block.large { min-height: 260px; display: grid; place-items: center; }
.empty-cell { height: 90px; }

/* ---- 响应�?---- */
@media (max-width: 1100px) {
  .summary-strip, .assign-grid { grid-template-columns: 1fr; }
  .summary-metrics { min-width: 0; }
}
@media (max-width: 680px) {
  .summary-metrics { grid-template-columns: 1fr; }
  .panel-heading, .assign-actions { flex-direction: column; align-items: stretch; }
  .search-input { width: 100%; }
  .student-row { grid-template-columns: 1fr auto; }
  .expand-arrow { grid-column: 1 / -1; justify-self: center; }
}
</style>
