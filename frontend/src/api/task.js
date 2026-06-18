// API 封装 —— 训练任务与分配相关请求
import api from "./http";

// 创建训练项目
export function createTask(data) {
  return api.post("/task/create", data);
}

// 获取训练项目列表
export function getTasks() {
  return api.get("/task/list");
}

// 获取用户列表（admin 按 company_id 前端过滤）
export function getUsers() {
  return api.get("/users");
}

// 为学员分配训练任务
export function assignTask(data) {
  return api.post("/task/assign", data);
}

// 获取所有分配记录（联表学员+训练信息）
export function getAssignments() {
  return api.get("/task/assignments");
}

// 学员查看自己的训练任务
export function getMyTasks(userId) {
  return api.get(`/my-tasks/${userId}`);
}

export function startTrainingAttempt(data) {
  return api.post("/training/start", data);
}

export function cancelTrainingAttempt(attemptId, data) {
  return api.post(`/training/${attemptId}/cancel`, data);
}

export function getStudentTrainingHistory(userId) {
  return api.get(`/training/history/student/${userId}`);
}

export function getCompanyTrainingAnalytics(companyId) {
  return api.get(`/training/analytics/company/${companyId}`);
}
