import axios from "axios";
import { API_PUBLIC_URL } from "./http";

export const LAUNCHER_API = "http://127.0.0.1:9000";
export const BACKEND_API = API_PUBLIC_URL;

const launcher = axios.create({
  baseURL: LAUNCHER_API,
  timeout: 10000,
});

export function getLauncherStatus() {
  return launcher.get("/status");
}

export function getTrainingStatus() {
  return launcher.get("/training_status");
}

export function startUnityTraining(data) {
  return launcher.post("/start", data);
}

export function startUnityEntry(data) {
  return launcher.post("/start-entry", data);
}

export function launcherErrorMessage(error) {
  if (!error.response) return "Launcher 未启动，请先启动本地 launcher 后再开始训练。";
  const code = error.response.data?.error_code;
  if (code === "exe_not_found") return "Unity 训练程序不存在，请检查 launcher 配置中的 trainer_exe。";
  if (code === "already_running") return "Unity 训练已在运行，请先完成或关闭当前训练。";
  if (code === "launch_failed") return `Unity 训练启动失败：${error.response.data?.message || "未知错误"}`;
  return error.response.data?.message || "启动 Unity 训练失败。";
}
