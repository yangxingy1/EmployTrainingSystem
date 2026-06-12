// Vue 应用入口
import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import './style.css'

createApp(App)
  .use(router)   // 注册路由
  .mount('#app')
