// API 封装 —— 认证相关请求
import api from "./http";

// 登录（需传 company_id 用于公司校验）
export function login(username, password, companyId) {
  return api.post("/login", {
    username,
    password,
    company_id: companyId
  });
}

// 注册学员（需传 company_id 绑定所属公司）
export function register(username, password, companyId) {
  return api.post("/register", {
    username,
    password,
    role: "student",
    company_id: companyId
  });
}
