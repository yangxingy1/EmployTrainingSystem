import axios from "axios";

export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || "/api";
export const API_PUBLIC_URL = API_BASE_URL.startsWith("http")
  ? API_BASE_URL
  : new URL(API_BASE_URL, window.location.origin).toString().replace(/\/$/, "");

const api = axios.create({
  baseURL: API_BASE_URL,
  timeout: 10000,
});

export default api;
