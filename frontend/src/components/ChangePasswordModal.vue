<template>
  <div class="password-mask" @click.self="emit('close')">
    <div class="password-dialog">
      <h3>修改密码</h3>
      <label class="form-item">
        <span>新密码</span>
        <input v-model="newPassword" type="password" placeholder="请输入新密码" />
      </label>
      <label class="form-item">
        <span>确认新密码</span>
        <input v-model="confirmPassword" type="password" placeholder="请再次输入新密码" />
      </label>
      <div v-if="message" :class="['message', messageType]">{{ message }}</div>
      <div class="dialog-actions">
        <button class="cancel-btn" @click="emit('close')">取消</button>
        <button class="confirm-btn" :disabled="submitting" @click="submit">
          {{ submitting ? "保存中..." : "确认修改" }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from "vue";
import api from "../api/http";

const emit = defineEmits(["close", "changed"]);

const newPassword = ref("");
const confirmPassword = ref("");
const message = ref("");
const messageType = ref("error");
const submitting = ref(false);

async function submit() {
  message.value = "";
  if (!newPassword.value || newPassword.value.length < 3) {
    message.value = "密码至少需要3位";
    messageType.value = "error";
    return;
  }
  if (newPassword.value !== confirmPassword.value) {
    message.value = "两次输入的密码不一致";
    messageType.value = "error";
    return;
  }

  submitting.value = true;
  try {
    await api.patch("/change-password", {
      new_password: newPassword.value,
      confirm_password: confirmPassword.value,
    });
    message.value = "密码修改成功";
    messageType.value = "success";
    emit("changed");
    setTimeout(() => emit("close"), 500);
  } catch (error) {
    message.value = error.response?.data?.detail || "修改失败";
    messageType.value = "error";
  } finally {
    submitting.value = false;
  }
}
</script>

<style scoped>
.password-mask { position: fixed; inset: 0; z-index: 2000; display: grid; place-items: center; background: rgba(0,0,0,0.42); }
.password-dialog { width: min(92vw, 420px); padding: 26px; border-radius: var(--radius-xl); background: #fff; box-shadow: 0 20px 42px rgba(0,0,0,0.18); }
.password-dialog h3 { margin: 0 0 18px; color: var(--heading); font-size: 22px; }
.form-item { display: grid; gap: 7px; margin-bottom: 14px; }
.form-item span { color: var(--heading); font-size: 13px; font-weight: 800; }
.form-item input { height: 44px; border: 1px solid var(--border); border-radius: var(--radius); padding: 0 12px; font-size: 14px; outline: none; }
.form-item input:focus { border-color: var(--primary); box-shadow: 0 0 0 3px rgba(47,111,115,0.12); }
.message { margin: 8px 0 14px; padding: 10px 12px; border-radius: var(--radius); font-size: 14px; font-weight: 700; }
.message.error { color: var(--danger); background: var(--danger-soft); border: 1px solid #ffd1cc; }
.message.success { color: var(--success); background: #e8f6ef; border: 1px solid #c3e6d6; }
.dialog-actions { display: flex; justify-content: flex-end; gap: 10px; margin-top: 18px; }
.cancel-btn, .confirm-btn { height: 38px; padding: 0 18px; border: 0; border-radius: var(--radius); font-weight: 800; cursor: pointer; }
.cancel-btn { color: #555; background: #e0e4e8; }
.confirm-btn { color: #fff; background: var(--primary); }
.confirm-btn:disabled { opacity: 0.5; cursor: not-allowed; }
</style>
