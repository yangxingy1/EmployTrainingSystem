const DEFAULT_BACKEND_API = "http://60.205.176.200:8000";

export const BACKEND_API = (
  import.meta.env.VITE_BACKEND_API ||
  window.__BACKEND_API__ ||
  DEFAULT_BACKEND_API
).replace(/\/$/, "");

export const LAUNCHER_API = (
  import.meta.env.VITE_LAUNCHER_API ||
  window.__LAUNCHER_API__ ||
  "http://127.0.0.1:9000"
).replace(/\/$/, "");
