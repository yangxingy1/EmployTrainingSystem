// 全局 API 配置 —— 从 Vite 环境变量读取，打包后不可变
const API_BASE = import.meta.env.VITE_API_BASE_URL || "http://127.0.0.1:8000";
const LAUNCHER_URL = import.meta.env.VITE_LAUNCHER_URL || "http://127.0.0.1:9000";

// 拼接 API URL 辅助函数
function api(path) {
  return `${API_BASE}${path}`;
}

export { API_BASE, LAUNCHER_URL, api };
