<template>
  <section class="assign-workbench">
    <div class="summary-strip">
      <div>
        <p class="section-kicker">训练分配</p>
        <h3>学员训练派发看板</h3>
        <span>按学员选择训练项目，系统会自动过滤已分配内容并保留记录。</span>
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
          <span>待开始</span>
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
            <p>共 {{ students.length }} 名学员</p>
          </div>

          <input
            v-model.trim="searchTerm"
            class="search-input"
            type="text"
            placeholder="搜索学员"
          />
        </div>

        <div class="student-list">
          <div
            v-for="student in filteredStudents"
            :key="student.id"
            class="student-item"
            :class="{ selected: selectedStudent === student.id }"
          >
            <button
              class="student-row"
              @click="selectStudent(student.id)"
            >
              <span class="student-info">
                <strong>{{ student.username }}</strong>
                <small>ID: {{ student.id }}</small>
              </span>
              <span class="student-count">
                {{ studentAssignedCount(student.id) }} 项训练
              </span>
              <span
                class="expand-arrow"
                :class="{ expanded: expandedStudent === student.id }"
                @click.stop="toggleExpand(student.id)"
              >▼</span>
            </button>

            <div v-if="expandedStudent === student.id" class="student-detail">
              <p v-if="studentAssignedCount(student.id) === 0" class="detail-empty">
                暂无分配记录
              </p>
              <ul v-else class="detail-list">
                <li
                  v-for="record in getStudentAssignments(student.id)"
                  :key="record.id"
                  class="detail-item"
                >
                  <span class="detail-title">{{ record.task_title }}</span>
                  <span :class="['status-sm', record.status]">
                    {{ statusLabel(record.status) }}
                  </span>
                </li>
              </ul>
            </div>
          </div>

          <div v-if="!filteredStudents.length" class="empty-block">
            {{ students.length ? '未找到匹配学员' : '暂未注册学员，请先在登录页注册学员账号' }}
          </div>
        </div>
      </section>

      <section class="panel task-panel">
        <div class="panel-heading">
          <div>
            <h4>训练项目</h4>
            <p v-if="selectedStudentInfo">
              当前学员：{{ selectedStudentInfo.username }}
            </p>
            <p v-else>请先选择一名学员</p>
          </div>

          <span class="selection-count">已选 {{ selectedTasks.length }} 项</span>
        </div>

        <div v-if="!selectedStudentInfo" class="empty-block large">
          选择左侧学员后，可以在这里勾选训练项目。
        </div>

        <div v-else class="table-frame">
          <table>
            <thead>
              <tr>
                <th>选择</th>
                <th>训练项目</th>
                <th>说明</th>
                <th>状态</th>
              </tr>
            </thead>
            <tbody>
              <tr
                v-for="task in tasks"
                :key="task.id"
                :class="{ disabled: isAssigned(task.id) }"
                @click="toggleTask(task.id)"
              >
                <td>
                  <input
                    type="checkbox"
                    :value="task.id"
                    v-model="selectedTasks"
                    :disabled="isAssigned(task.id)"
                    @click.stop
                  />
                </td>
                <td>
                  <strong>{{ task.title }}</strong>
                </td>
                <td>{{ task.description || "暂无说明" }}</td>
                <td>
                  <span v-if="isAssigned(task.id)" class="status-tag done">已分配</span>
                  <span v-else class="status-tag ready">可分配</span>
                </td>
              </tr>
              <tr v-if="!tasks.length">
                <td colspan="4" class="empty-cell">暂无训练项目</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="assign-actions">
          <span>
            {{ selectedStudentInfo ? `将分配给 ${selectedStudentInfo.username}` : "未选择学员" }}
          </span>
          <button class="assign-btn" :disabled="!canAssign" @click="assignSelectedTasks">
            {{ assigning ? "分配中..." : "确认分配" }}
          </button>
        </div>
      </section>
    </div>

    <section class="panel records-panel">
      <div class="panel-heading">
        <div>
          <h4>近期分配记录</h4>
          <p>展示最新训练派发与完成状态</p>
        </div>
      </div>

      <div class="table-frame">
        <table>
          <thead>
            <tr>
              <th>学员</th>
              <th>训练项目</th>
              <th>状态</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="record in recentAssignments" :key="record.id">
              <td>
                <strong>{{ record.username }}</strong>
              </td>
              <td>{{ record.task_title }}</td>
              <td>
                <span :class="['status-tag', record.status]">
                  {{ statusLabel(record.status) }}
                </span>
              </td>
            </tr>
            <tr v-if="!recentAssignments.length">
              <td colspan="3" class="empty-cell">暂无分配记录</td>
            </tr>
          </tbody>
        </table>
      </div>
    </section>
  </section>
</template>

<script setup>
import { ref, computed, watch } from "vue";
import { assignTask } from "../../api/task";

const emit = defineEmits(["assigned"]);

const props = defineProps({
  tasks: { type: Array, default: () => [] },
  students: { type: Array, default: () => [] },
  assignments: { type: Array, default: () => [] }
});

const searchTerm = ref("");
const selectedStudent = ref(null);  // 当前选中的学员 ID
const selectedTasks = ref([]);
const assigning = ref(false);
const notice = ref("");
const noticeType = ref("");
const expandedStudent = ref(null);  // 展开详情的学员 ID

// 搜索过滤：按用户名模糊匹配
const filteredStudents = computed(() => {
  if (!searchTerm.value) return props.students;
  const term = searchTerm.value.toLowerCase();
  return props.students.filter((s) => s.username.toLowerCase().includes(term));
});

const selectedStudentInfo = computed(() => {
  return props.students.find((s) => s.id === selectedStudent.value);
});

const pendingCount = computed(() => {
  return props.assignments.filter((a) => a.status === "pending" || a.status === "未开始").length;
});

const recentAssignments = computed(() => {
  return props.assignments.slice(0, 20);
});

const canAssign = computed(() => {
  return selectedStudent.value && selectedTasks.value.length > 0 && !assigning.value;
});

function studentAssignedCount(studentId) {
  return props.assignments.filter((a) => a.user_id === studentId).length;
}

function getStudentAssignments(studentId) {
  return props.assignments
    .filter((a) => a.user_id === studentId)
    .sort((a, b) => b.id - a.id);
}

// 判断该任务是否已分配给当前选中的学员
function isAssigned(taskId) {
  if (!selectedStudent.value) return false;
  return props.assignments.some(
    (a) => a.user_id === selectedStudent.value && a.task_id === taskId
  );
}

// 切换学员详情展开/收起
function toggleExpand(studentId) {
  expandedStudent.value = expandedStudent.value === studentId ? null : studentId;
}

function toggleTask(taskId) {
  if (isAssigned(taskId)) return;
  const index = selectedTasks.value.indexOf(taskId);
  if (index >= 0) {
    selectedTasks.value.splice(index, 1);
  } else {
    selectedTasks.value.push(taskId);
  }
}

function selectStudent(studentId) {
  selectedStudent.value = studentId;
  selectedTasks.value = [];
}

function statusLabel(status) {
  const map = { pending: "待开始", "未开始": "待开始", running: "进行中", done: "已完成" };
  return map[status] || status;
}

// 批量分配选中任务给当前学员
async function assignSelectedTasks() {
  if (!canAssign.value) return;
  assigning.value = true;
  notice.value = "";
  let successCount = 0;

  try {
    for (const taskId of selectedTasks.value) {
      await assignTask({
        user_id: selectedStudent.value,
        task_id: taskId
      });
      successCount++;
    }
    notice.value = `成功分配 ${successCount} 项训练`;
    noticeType.value = "success";
    selectedTasks.value = [];
    emit("assigned");
  } catch (error) {
    notice.value = "分配失败，请重试";
    noticeType.value = "error";
  } finally {
    assigning.value = false;
  }
}

watch(selectedStudent, () => {
  selectedTasks.value = [];
});

watch(filteredStudents, (newVal) => {
  if (expandedStudent.value && !newVal.find((s) => s.id === expandedStudent.value)) {
    expandedStudent.value = null;
  }
});
</script>

<style scoped>
.assign-workbench {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.summary-strip {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 24px;
  padding: 22px 24px;
  border-radius: var(--radius);
  background: linear-gradient(135deg, #f0f8f7, #e9f4f3);
  border: 1px solid #d4e8e6;
}

.section-kicker {
  color: var(--primary-strong);
  font-size: 12px;
  font-weight: 800;
  margin-bottom: 4px;
}

.summary-strip h3 {
  font-size: 22px;
  margin: 2px 0 6px;
}

.summary-strip span {
  color: var(--text-muted);
}

.summary-metrics {
  display: grid;
  grid-template-columns: repeat(3, minmax(0, 1fr));
  gap: 18px;
  min-width: 280px;
  text-align: center;
}

.summary-metrics strong {
  display: block;
  color: var(--heading);
  font-size: 28px;
  margin-bottom: 4px;
}

.summary-metrics span {
  color: var(--text-muted);
  font-size: 13px;
}

.notice {
  padding: 12px 16px;
  border-radius: var(--radius-sm);
  font-weight: 700;
}

.notice.success {
  color: var(--success);
  background: #e8f6ef;
  border: 1px solid #c3e6d6;
}

.notice.error {
  color: var(--danger);
  background: #fff1ef;
  border: 1px solid #ffd5ce;
}

.assign-grid {
  display: grid;
  grid-template-columns: minmax(320px, 1fr) minmax(520px, 1.8fr);
  gap: 18px;
  align-items: stretch;
}

.panel {
  padding: 20px;
  border-radius: var(--radius);
  background: var(--surface);
  border: 1px solid var(--border);
  box-shadow: var(--shadow-sm);
  transition: box-shadow var(--transition);
}

.panel:hover {
  box-shadow: var(--shadow);
}

.panel-heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 14px;
  margin-bottom: 14px;
}

.panel-heading h4 {
  margin: 0 0 4px;
  color: var(--heading);
  font-size: 19px;
}

.panel-heading p {
  color: var(--text-muted);
  font-size: 14px;
}

.search-input {
  width: 160px;
  height: 38px;
  border: 1px solid var(--border);
  border-radius: var(--radius-sm);
  padding: 0 12px;
  outline: none;
  transition: all var(--transition);
}

.search-input:focus {
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(47, 111, 115, 0.14);
}

/* ---- 学员列表 ---- */
.student-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-height: 520px;
  overflow: auto;
}

.student-item {
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: #fff;
  overflow: hidden;
  transition: all var(--transition);
}

.student-item:hover {
  border-color: var(--primary);
}

.student-item.selected {
  border-color: var(--primary);
  background: #edf7f5;
}

.student-row {
  width: 100%;
  display: grid;
  grid-template-columns: 1fr auto 32px;
  gap: 10px;
  align-items: center;
  min-height: 56px;
  padding: 10px 14px;
  border: none;
  color: var(--text);
  background: transparent;
  text-align: left;
  cursor: pointer;
  transition: all var(--transition);
}

.student-row:hover {
  transform: none;
}

.student-info strong,
.student-info small {
  display: block;
}

.student-info small {
  margin-top: 2px;
  color: var(--text-muted);
  font-size: 12px;
}

.student-count {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-width: 72px;
  height: 28px;
  padding: 0 10px;
  border-radius: var(--radius-full);
  color: var(--primary-strong);
  background: #e9f4f3;
  font-size: 13px;
  font-weight: 700;
  white-space: nowrap;
}

.expand-arrow {
  width: 32px;
  height: 32px;
  display: grid;
  place-items: center;
  border-radius: var(--radius-sm);
  color: var(--text-muted);
  font-size: 12px;
  transition: all var(--transition);
  user-select: none;
}

.expand-arrow:hover {
  color: var(--primary);
  background: rgba(47, 111, 115, 0.08);
}

.expand-arrow.expanded {
  transform: rotate(180deg);
  color: var(--primary);
}

/* ---- 学员展开详情 ---- */
.student-detail {
  border-top: 1px solid var(--border);
  padding: 12px 14px;
  background: #f8fbfd;
  animation: slideDown 0.2s ease;
}

@keyframes slideDown {
  from { opacity: 0; max-height: 0; }
  to { opacity: 1; max-height: 300px; }
}

.detail-empty {
  color: var(--text-muted);
  font-size: 13px;
  padding: 8px 0;
}

.detail-list {
  list-style: none;
  margin: 0;
  padding: 0;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.detail-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 8px 10px;
  border-radius: var(--radius-sm);
  background: white;
  border: 1px solid var(--border);
  font-size: 13px;
}

.detail-title {
  color: var(--text);
  font-weight: 600;
}

.status-sm {
  display: inline-flex;
  align-items: center;
  min-height: 24px;
  padding: 2px 10px;
  border-radius: var(--radius-full);
  font-size: 12px;
  font-weight: 700;
  white-space: nowrap;
}

.status-sm.pending {
  color: var(--warning);
  background: #fff7e6;
}

.status-sm.running {
  color: var(--primary-strong);
  background: #e9f4f3;
}

.status-sm.done {
  color: var(--success);
  background: #e8f6ef;
}

/* ---- 训练项目面板 ---- */
.selection-count {
  padding: 7px 12px;
  border-radius: var(--radius-full);
  color: var(--primary-strong);
  background: #e9f4f3;
  font-size: 13px;
  font-weight: 800;
}

.table-frame {
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
  padding: 13px 14px;
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

tbody tr:last-child td {
  border-bottom: 0;
}

tbody tr:not(.disabled) {
  cursor: pointer;
  transition: background var(--transition);
}

tbody tr:not(.disabled):hover {
  background: #f0f8f7;
}

tbody tr.disabled {
  color: var(--text-muted);
  background: #fafafa;
}

input[type="checkbox"] {
  width: 18px;
  height: 18px;
  accent-color: var(--primary);
  cursor: pointer;
}

.status-tag {
  display: inline-flex;
  align-items: center;
  min-height: 28px;
  padding: 4px 12px;
  border-radius: var(--radius-full);
  font-size: 13px;
  font-weight: 800;
  white-space: nowrap;
}

.status-tag.ready {
  color: var(--primary-strong);
  background: #e9f4f3;
}

.status-tag.pending {
  color: var(--warning);
  background: #fff7e6;
}

.status-tag.running {
  color: var(--primary-strong);
  background: #e9f4f3;
}

.status-tag.done {
  color: var(--success);
  background: #e8f6ef;
}

.assign-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-top: 16px;
  color: var(--text-muted);
}

.assign-btn {
  min-width: 128px;
  height: 44px;
  padding: 0 22px;
  border-radius: var(--radius-sm);
  color: #e8f5f3;
  background: #1a5c60;
  font-weight: 800;
  font-size: 15px;
  cursor: pointer;
  box-shadow: 0 6px 18px rgba(26, 92, 96, 0.28);
  transition: all var(--transition);
}

.assign-btn:hover:not(:disabled) {
  background: #14484b;
  box-shadow: 0 10px 26px rgba(26, 92, 96, 0.38);
}

.assign-btn:disabled {
  cursor: not-allowed;
  opacity: 0.45;
  box-shadow: none;
}

.records-panel {
  padding-bottom: 20px;
}

.empty-block,
.empty-cell {
  color: var(--text-muted);
  text-align: center;
}

.empty-block {
  padding: 32px 16px;
  border: 1px dashed var(--border);
  border-radius: var(--radius);
  background: #fbfcfe;
  font-size: 14px;
  line-height: 1.6;
}

.empty-block.large {
  min-height: 260px;
  display: grid;
  place-items: center;
}

.empty-cell {
  height: 90px;
}

@media (max-width: 1100px) {
  .summary-strip,
  .assign-grid {
    grid-template-columns: 1fr;
  }

  .summary-strip {
    display: grid;
  }

  .summary-metrics {
    min-width: 0;
  }
}

@media (max-width: 680px) {
  .summary-metrics {
    grid-template-columns: 1fr;
  }

  .panel-heading,
  .assign-actions {
    flex-direction: column;
    align-items: stretch;
  }

  .search-input {
    width: 100%;
  }

  .student-row {
    grid-template-columns: 1fr auto;
  }

  .expand-arrow {
    grid-column: 1 / -1;
    justify-self: center;
  }
}
</style>
