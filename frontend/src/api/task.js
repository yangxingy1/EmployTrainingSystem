// API 封装 —— 训练任务与分配相关请求
import axios from "axios";
import { api } from "../config";

// 创建训练项目
export function createTask(data) {
  return axios.post(api("/task/create"), data);
}

// 获取训练项目列表
export function getTasks() {
  return axios.get(api("/task/list"));
}

// 获取用户列表
export function getUsers() {
  return axios.get(api("/users"));
}

// 为学员分配训练任务
export function assignTask(data) {
  return axios.post(api("/task/assign"), data);
}

// 获取所有分配记录
export function getAssignments(companyId) {
  const params = companyId ? { company_id: companyId } : {};
  return axios.get(api("/task/assignments"), { params });
}

// 学员查看自己的训练任务
export function getMyTasks(userId) {
  return axios.get(api(`/my-tasks/${userId}`));
}
