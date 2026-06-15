// API 封装 —— 认证相关
import axios from 'axios'

const API = 'http://127.0.0.1:8000'

// 登录：返回 token + 用户信息
export function login(username, password) {
  return axios.post(`${API}/login`, { username, password, role: '' })
}

// 注册新用户
export function register(username, password, role) {
  return axios.post(`${API}/register`, { username, password, role })
}
