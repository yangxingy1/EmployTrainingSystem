// API 封装 —— 任务与分配
import axios from 'axios'

const API = 'http://127.0.0.1:8000'

export function createTask(data) {
  return axios.post(`${API}/task/create`, data)
}

export function getTasks() {
  return axios.get(`${API}/task/list`)
}

export function getUsers() {
  return axios.get(`${API}/users`)
}

// 为学员分配训练
export function assignTask(data) {
  return axios.post(`${API}/task/assign`, data)
}

export function getAssignments() {
  return axios.get(`${API}/task/assignments`)
}

// 学员查看自己的任务
export function getMyTasks(userId) {
  return axios.get(`${API}/my-tasks/${userId}`)
}
