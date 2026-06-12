<template>
  <section class="assign-workbench">
    <div class="summary-strip">
      <div>
        <p class="section-kicker">Assignment</p>
        <h2>学员训练派发</h2>
        <span>选择学员后勾选训练项目，系统会自动过滤已分配内容。</span>
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
            <h3>选择学员</h3>
            <p>共 {{ students.length }} 名学员</p>
          </div>

          <input v-model.trim="searchTerm" class="search-input" type="text" placeholder="搜索学员" />
        </div>

        <div class="student-list">
          <article
            v-for="student in filteredStudents"
            :key="student.id"
            class="student-item"
            :class="{ selected: selectedStudent === student.id }"
          >
            <button class="student-row" @click="selectStudent(student.id)">
              <span class="avatar">{{ student.username.slice(0, 1).toUpperCase() }}</span>
              <span class="student-info">
                <strong>{{ student.username }}</strong>
                <small>ID: {{ student.id }} · {{ studentAssignedCount(student.id) }} 项训练</small>
              </span>
              <span
                class="expand-arrow"
                :class="{ expanded: expandedStudent === student.id }"
                @click.stop="toggleExpand(student.id)"
              >
                ^
              </span>
            </button>

            <div v-if="expandedStudent === student.id" class="student-detail">
              <p v-if="studentAssignedCount(student.id) === 0" class="detail-empty">暂无分配记录</p>
              <ul v-else class="detail-list">
                <li v-for="record in getStudentAssignments(student.id)" :key="record.id">
                  <span>{{ record.task_title }}</span>
                  <strong :class="['status-sm', statusClass(record.status)]">
                    {{ statusLabel(record.status) }}
                  </strong>
                </li>
              </ul>
            </div>
          </article>

          <div v-if="!filteredStudents.length" class="empty-block">
            {{ students.length ? "未找到匹配学员" : "暂未注册学员，请先在登录页注册学员账号" }}
          </div>
        </div>
      </section>

      <section class="panel task-panel">
        <div class="panel-heading">
          <div>
            <h3>训练项目</h3>
            <p v-if="selectedStudentInfo">当前学员：{{ selectedStudentInfo.username }}</p>
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
                <td><strong>{{ task.title }}</strong></td>
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
          <span>{{ selectedStudentInfo ? `将分配给 ${selectedStudentInfo.username}` : "未选择学员" }}</span>
          <button class="assign-btn" :disabled="!canAssign" @click="assignSelectedTasks">
            {{ assigning ? "分配中..." : "确认分配" }}
          </button>
        </div>
      </section>
    </div>

    <section class="panel records-panel">
      <div class="panel-heading">
        <div>
          <h3>近期分配记录</h3>
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
              <td><strong>{{ record.username }}</strong></td>
              <td>{{ record.task_title }}</td>
              <td>
                <span :class="['status-tag', statusClass(record.status)]">
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
const selectedStudent = ref(null);
const selectedTasks = ref([]);
const assigning = ref(false);
const notice = ref("");
const noticeType = ref("");
const expandedStudent = ref(null);

const filteredStudents = computed(() => {
  if (!searchTerm.value) return props.students;
  const term = searchTerm.value.toLowerCase();
  return props.students.filter((s) => s.username.toLowerCase().includes(term));
});

const selectedStudentInfo = computed(() => {
  return props.students.find((s) => s.id === selectedStudent.value);
});

const pendingCount = computed(() => {
  return props.assignments.filter((a) => statusClass(a.status) === "pending").length;
});

const recentAssignments = computed(() => {
  return props.assignments.slice(0, 20);
});

const canAssign = computed(() => {
  return selectedStudent.value && selectedTasks.value.length > 0 && !assigning.value;
});

function statusClass(status) {
  if (["done", "completed", "已完成"].includes(status)) return "done";
  if (["running", "进行中"].includes(status)) return "running";
  return "pending";
}

function studentAssignedCount(studentId) {
  return props.assignments.filter((a) => a.user_id === studentId).length;
}

function getStudentAssignments(studentId) {
  return props.assignments
    .filter((a) => a.user_id === studentId)
    .sort((a, b) => b.id - a.id);
}

function isAssigned(taskId) {
  if (!selectedStudent.value) return false;
  return props.assignments.some(
    (a) => a.user_id === selectedStudent.value && a.task_id === taskId
  );
}

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
  display: grid;
  gap: 16px;
}

.summary-strip {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 18px 20px;
  border: 1px solid var(--border);
  border-left: 4px solid var(--accent);
  border-radius: var(--radius);
  background: var(--surface);
  box-shadow: var(--shadow-sm);
}

.section-kicker {
  color: var(--accent);
  font-size: 12px;
  font-weight: 900;
  text-transform: uppercase;
}

.summary-strip h2 {
  margin: 6px 0 4px;
  font-size: 22px;
}

.summary-strip span {
  color: var(--text-muted);
}

.summary-metrics {
  display: grid;
  grid-template-columns: repeat(3, minmax(76px, 1fr));
  gap: 10px;
  min-width: 278px;
}

.summary-metrics div {
  padding: 12px;
  border-radius: var(--radius-sm);
  background: var(--surface-soft);
  text-align: center;
}

.summary-metrics strong {
  display: block;
  color: var(--heading);
  font-size: 24px;
}

.summary-metrics span {
  font-size: 12px;
}

.notice {
  padding: 12px 14px;
  border-radius: var(--radius-sm);
  font-weight: 800;
}

.notice.success {
  color: var(--success);
  background: var(--success-soft);
  border: 1px solid #bfdfcf;
}

.notice.error {
  color: var(--danger);
  background: var(--danger-soft);
  border: 1px solid #ffd1cc;
}

.assign-grid {
  display: grid;
  grid-template-columns: minmax(300px, 0.8fr) minmax(520px, 1.4fr);
  gap: 16px;
}

.panel {
  padding: 18px;
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--surface);
  box-shadow: var(--shadow-sm);
}

.panel-heading {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 14px;
}

.panel-heading h3 {
  margin-bottom: 4px;
  font-size: 18px;
}

.panel-heading p {
  color: var(--text-muted);
  font-size: 14px;
}

.search-input {
  width: 160px;
  height: 38px;
}

.student-list {
  display: grid;
  gap: 8px;
  max-height: 516px;
  overflow: auto;
}

.student-item {
  border: 1px solid var(--border);
  border-radius: var(--radius);
  background: var(--surface-soft);
  overflow: hidden;
}

.student-item.selected {
  border-color: var(--primary);
  box-shadow: 0 0 0 3px rgba(20, 112, 111, 0.10);
}

.student-row {
  width: 100%;
  display: grid;
  grid-template-columns: 36px 1fr 30px;
  gap: 10px;
  align-items: center;
  min-height: 58px;
  padding: 10px 12px;
  color: var(--text);
  background: transparent;
  text-align: left;
}

.avatar {
  width: 36px;
  height: 36px;
  display: grid;
  place-items: center;
  border-radius: 50%;
  color: #ffffff;
  background: var(--primary);
  font-weight: 900;
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

.expand-arrow {
  width: 30px;
  height: 30px;
  display: grid;
  place-items: center;
  border-radius: var(--radius-sm);
  color: var(--text-muted);
  transition: transform var(--transition), background var(--transition);
}

.expand-arrow:hover {
  background: var(--primary-soft);
}

.expand-arrow.expanded {
  transform: rotate(180deg);
}

.student-detail {
  padding: 10px 12px 12px;
  border-top: 1px solid var(--border);
  background: var(--surface);
}

.detail-empty {
  color: var(--text-muted);
  font-size: 13px;
}

.detail-list {
  display: grid;
  gap: 6px;
  padding: 0;
  margin: 0;
  list-style: none;
}

.detail-list li {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 10px;
  padding: 8px 10px;
  border-radius: var(--radius-sm);
  background: var(--surface-soft);
  font-size: 13px;
}

.selection-count,
.status-sm,
.status-tag {
  display: inline-flex;
  align-items: center;
  justify-content: center;
  min-height: 26px;
  padding: 4px 10px;
  border-radius: 999px;
  font-size: 12px;
  font-weight: 900;
  white-space: nowrap;
}

.selection-count,
.status-tag.ready,
.status-tag.running,
.status-sm.running {
  color: var(--primary-strong);
  background: var(--primary-soft);
}

.status-tag.pending,
.status-sm.pending {
  color: var(--warning);
  background: var(--warning-soft);
}

.status-tag.done,
.status-sm.done {
  color: var(--success);
  background: var(--success-soft);
}

.table-frame {
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

tbody tr:last-child td {
  border-bottom: 0;
}

tbody tr:not(.disabled) {
  cursor: pointer;
}

tbody tr:not(.disabled):hover {
  background: #f8fbfb;
}

tbody tr.disabled {
  color: var(--text-muted);
  background: #f7f7f7;
}

input[type="checkbox"] {
  width: 18px;
  height: 18px;
  accent-color: var(--primary);
}

.assign-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin-top: 14px;
  color: var(--text-muted);
}

.assign-btn {
  min-width: 126px;
  height: 40px;
  padding: 0 18px;
  border-radius: var(--radius-sm);
  color: #ffffff;
  background: var(--primary);
  font-weight: 900;
  transition: background var(--transition), box-shadow var(--transition), transform var(--transition);
}

.assign-btn:hover:not(:disabled) {
  background: var(--primary-strong);
  box-shadow: var(--shadow-sm);
  transform: translateY(-1px);
}

.assign-btn:disabled {
  opacity: 0.48;
}

.empty-block,
.empty-cell {
  color: var(--text-muted);
  text-align: center;
}

.empty-block {
  padding: 28px 16px;
  border: 1px dashed var(--border);
  border-radius: var(--radius);
  background: var(--surface-soft);
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

@media (max-width: 1120px) {
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
    align-items: stretch;
    flex-direction: column;
  }

  .search-input {
    width: 100%;
  }
}
</style>
