// API 封装 —— 训练任务与分配相关请求
import axios from "axios";

const API = "http://127.0.0.1:8000";  // 后端地址，部署时需修改

// 创建训练项目
export function createTask(data) {
  return axios.post(`${API}/task/create`, data);
}

// 获取训练项目列表
export function getTasks() {
  return axios.get(`${API}/task/list`);
}

// 获取用户列表（admin 按 company_id 前端过滤）
export function getUsers() {
  return axios.get(`${API}/users`);
}

// 为学员分配训练任务
export function assignTask(data) {
  return axios.post(`${API}/task/assign`, data);
}

// 获取所有分配记录（联表学员+训练信息）
export function getAssignments() {
  return axios.get(`${API}/task/assignments`);
}

// 学员查看自己的训练任务
export function getMyTasks(userId) {
  return axios.get(`${API}/my-tasks/${userId}`);
}

export function startTrainingAttempt(data) {
  return axios.post(`${API}/training/start`, data);
}

export function cancelTrainingAttempt(attemptId, data) {
  return axios.post(`${API}/training/${attemptId}/cancel`, data);
}

export function getStudentTrainingHistory(userId) {
  return axios.get(`${API}/training/history/student/${userId}`);
}

export function getCompanyTrainingAnalytics(companyId) {
  return axios.get(`${API}/training/analytics/company/${companyId}`);
}
