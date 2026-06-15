// Vue 应用入口 —— 挂载路由 + 全局样式
import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import "./style.css";

createApp(App).use(router).mount("#app");
