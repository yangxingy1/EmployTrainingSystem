// API 封装 —— 认证相关请求
import axios from "axios";

const API = "http://127.0.0.1:8000";  // 后端地址，部署时需修改

// 登录（需传 company_id 用于公司校验）
export function login(username, password, companyId) {
  return axios.post(`${API}/login`, {
    username,
    password,
    company_id: companyId
  });
}

// 注册学员（需传 company_id 绑定所属公司）
export function register(username, password, companyId) {
  return axios.post(`${API}/register`, {
    username,
    password,
    role: "student",
    company_id: companyId
  });
}
